using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class LobbyChatManager : MonoBehaviour
{
  public static LobbyChatManager Instance;
 
    [Header("Chat Display Text (single TMP inside Scroll View Content)")]
    public TMP_Text chatText;         // drag Text (TMP) inside Content here
 
    [Header("Scroll View")]
    public ScrollRect scrollRect;     // drag Scroll View here
 
    [Header("Chat Panel")]
    public GameObject chatPanel;      // drag chat box here
 
    const string CHAT_KEY    = "ChatLog";
    const int    MAX_MSG_LEN = 65;
 
    TouchScreenKeyboard keyboard;
    bool keyboardOpen = false;
 
    void Awake()
    {
        Instance = this;
    }
 
    void Start()
    {
        if (chatPanel != null)
            chatPanel.SetActive(true);
 
        if (chatText != null)
            chatText.text = "";
 
        // Load existing messages for late joiner
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CHAT_KEY))
        {
            string existing =
                (string)PhotonNetwork.CurrentRoom.CustomProperties[CHAT_KEY];
            RefreshChatDisplay(existing);
        }
    }
 
    // ── Chat button OnClick ───────────────────────────────────
    public void OpenChatKeyboard()
    {
        if (chatPanel != null)
            chatPanel.SetActive(true);
 
        keyboard = TouchScreenKeyboard.Open(
            "",
            TouchScreenKeyboardType.Default,
            false, false, false, false, ""
        );
 
        keyboardOpen = true;
    }
 
    // ── Poll keyboard ─────────────────────────────────────────
    void Update()
    {
        if (!keyboardOpen || keyboard == null) return;
 
        if (keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            string typed = keyboard.text.Trim();
            keyboardOpen = false;
 
            if (!string.IsNullOrEmpty(typed))
                SendMessage_Chat(typed);
        }
        else if (keyboard.status == TouchScreenKeyboard.Status.Canceled ||
                 keyboard.status == TouchScreenKeyboard.Status.LostFocus)
        {
            keyboardOpen = false;
        }
    }
 
    // ── Send message ──────────────────────────────────────────
    void SendMessage_Chat(string rawText)
    {
        if (PhotonNetwork.CurrentRoom == null) return;
 
        if (rawText.Length > MAX_MSG_LEN)
            rawText = rawText.Substring(0, MAX_MSG_LEN);
 string playerName = PlayerPrefs.GetString("playerName", "");
if (string.IsNullOrEmpty(playerName))
    playerName = PhotonNetwork.LocalPlayer.NickName;
if (string.IsNullOrEmpty(playerName))
    playerName = PhotonNetwork.IsMasterClient ? "Player 1" : "Player 2";
 
        string newLine = playerName + ": " + rawText;
 
        // Read existing log
        string currentLog = "";
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CHAT_KEY))
            currentLog = (string)PhotonNetwork.CurrentRoom.CustomProperties[CHAT_KEY];
 
        // Append — no line limit
        if (string.IsNullOrEmpty(currentLog))
            currentLog = newLine;
        else
            currentLog = currentLog + "\n" + newLine;
 
        // Push to Photon room properties
        Hashtable props = new Hashtable();
        props[CHAT_KEY] = currentLog;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }
 
    // ── Called by PhotonLobbyManager + Start for late joiners ─
    public void RefreshChatDisplay(string chatLog)
    {
          if (chatText == null) return;

    if (string.IsNullOrEmpty(chatLog))
    {
        chatText.text = "";
        return;
    }

    chatText.text = chatLog;

    chatText.ForceMeshUpdate(); // 🔥 VERY IMPORTANT

    StopAllCoroutines();
   if (scrollRect.verticalNormalizedPosition <= 0.05f)
    {
        StartCoroutine(ScrollToBottom());
    }
    }
 
    IEnumerator ScrollToBottom()
    {
           // Wait for UI to rebuild properly
    yield return null;
    yield return null;

    Canvas.ForceUpdateCanvases();

    if (scrollRect != null)
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
