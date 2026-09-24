using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
public class InGameChatManager : MonoBehaviourPunCallbacks
{
       public static InGameChatManager Instance;
 
    [Header("Chat Button (speech bubble)")]
    public Button chatButton;
 
    [Header("Message Slots (slot1 = older, slot2 = newer)")]
    public TMP_Text slot1Text;   // older message
    public TMP_Text slot2Text;   // newer message
 
    [Header("Settings")]
    [Range(1, 120)]
    public int maxMessageLength = 65;
    [Range(1f, 30f)]
    public float messageVisibleSeconds = 5f;
 
    const byte CHAT_EVENT = 30;
 
    TouchScreenKeyboard keyboard;
    bool keyboardOpen = false;
 
    Coroutine hideSlot1Coroutine;
    Coroutine hideSlot2Coroutine;
 
    // ══════════════════════════════════════════════════════════════════════
    void Awake() { Instance = this; }
 
    void Start()
    {
        SetVisible(slot1Text, false);
        SetVisible(slot2Text, false);
 
        if (chatButton != null)
            chatButton.onClick.AddListener(OnChatButtonClicked);
    }
 
    void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
    }
 
    void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
    }
 
    void Update()
    {
        if (!keyboardOpen || keyboard == null) return;
 
        if (keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            string typed = keyboard.text.Trim();
            keyboardOpen = false;
            if (!string.IsNullOrEmpty(typed))
                SendChatMessage(typed);
        }
        else if (keyboard.status == TouchScreenKeyboard.Status.Canceled ||
                 keyboard.status == TouchScreenKeyboard.Status.LostFocus)
        {
            keyboardOpen = false;
        }
    }
 
    // ══════════════════════════════════════════════════════════════════════
    public void OnChatButtonClicked()
    {
        keyboard = TouchScreenKeyboard.Open(
            "", TouchScreenKeyboardType.Default,
            false, false, false, false, ""
        );
        keyboardOpen = true;
    }
 
    // ══════════════════════════════════════════════════════════════════════
    void SendChatMessage(string rawText)
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;
 
        if (rawText.Length > maxMessageLength)
            rawText = rawText.Substring(0, maxMessageLength);
 
     string playerLabel = PlayerPrefs.GetString("playerName", 
    PhotonNetwork.IsMasterClient ? "Player 1" : "Player 2");
        string fullLine = playerLabel + ": " + rawText;
 
        // Show on own screen immediately
        PushMessage(fullLine);
 
        // Send to remote
        PhotonNetwork.RaiseEvent(
            CHAT_EVENT,
            fullLine,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable
        );
    }
 
    // ══════════════════════════════════════════════════════════════════════
    void OnPhotonEvent(EventData ev)
    {
        if (ev.Code != CHAT_EVENT) return;
        string fullLine = (string)ev.CustomData;
        PushMessage(fullLine);
    }
 
    // ══════════════════════════════════════════════════════════════════════
    // Core logic: push new message into slot2, move old slot2 → slot1
    // ══════════════════════════════════════════════════════════════════════
    void PushMessage(string line)
    {
        // Stop all existing timers first
        if (hideSlot1Coroutine != null) StopCoroutine(hideSlot1Coroutine);
        if (hideSlot2Coroutine != null) StopCoroutine(hideSlot2Coroutine);
 
        // Move slot2 content → slot1 (only if slot2 was visible)
        if (slot2Text != null && slot2Text.gameObject.activeSelf)
        {
            if (slot1Text != null)
            {
                slot1Text.text = slot2Text.text;
                SetVisible(slot1Text, true);
                // Give slot1 a fresh full timer
                hideSlot1Coroutine = StartCoroutine(HideAfterDelay(slot1Text, messageVisibleSeconds));
            }
        }
        else
        {
            // slot2 was empty — ensure slot1 is also hidden (clean state)
            SetVisible(slot1Text, false);
        }
 
        // Put new message into slot2
        if (slot2Text != null)
        {
            slot2Text.text = line;
            SetVisible(slot2Text, true);
            hideSlot2Coroutine = StartCoroutine(HideAfterDelay(slot2Text, messageVisibleSeconds));
        }
    }
 
    IEnumerator HideAfterDelay(TMP_Text label, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetVisible(label, false);
    }
 
    static void SetVisible(TMP_Text label, bool visible)
    {
        if (label == null) return;
        label.gameObject.SetActive(visible);
    }
}
