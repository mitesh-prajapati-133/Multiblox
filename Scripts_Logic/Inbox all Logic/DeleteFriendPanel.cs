using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.UI;

public class DeleteFriendPanel : MonoBehaviour
{
 public static DeleteFriendPanel Instance;

    [Header("=== PANEL REFERENCES ===")]
    public Button noButton;
    public Button yesButton;
[Header("=== BUTTON SOUND ===")]
public AudioSource buttonAudioSource;
public AudioClip buttonClickSound;
    private FirebaseFirestore db;

    private string currentUserId;
    private string friendUserId;
    private GameObject cardToRemove;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        gameObject.SetActive(false);

        if (noButton != null)
            noButton.onClick.AddListener(OnNoClicked);

        if (yesButton != null)
            yesButton.onClick.AddListener(OnYesClicked);
    }

    public void Open(string myUserId, string friendId, GameObject card)
    {
        currentUserId = myUserId;
        friendUserId  = friendId;
        cardToRemove  = card;

        gameObject.SetActive(true);
    }

    public void OnNoClicked()
    {

    PlayButtonSound();
        gameObject.SetActive(false);
    }

    public void OnYesClicked()
    {

    PlayButtonSound();
        if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(friendUserId))
        {
            Debug.LogError("DeleteFriendPanel: user IDs not set!");
            return;
        }

        string docA = currentUserId + "_" + friendUserId;
        string docB = friendUserId  + "_" + currentUserId;

        db.Collection("friends").Document(docA).DeleteAsync()
            .ContinueWithOnMainThread(taskA =>
        {
            if (taskA.Exception != null)
                Debug.LogError("Failed to delete friendDoc A: " + taskA.Exception);
            else
                Debug.Log("✅ Deleted " + docA);
        });

        db.Collection("friends").Document(docB).DeleteAsync()
            .ContinueWithOnMainThread(taskB =>
        {
            if (taskB.Exception != null)
                Debug.LogError("Failed to delete friendDoc B: " + taskB.Exception);
            else
                Debug.Log("✅ Deleted " + docB);
        });

        gameObject.SetActive(false);

        if (cardToRemove != null)
        {
            Destroy(cardToRemove);
            cardToRemove = null;
        }

        if (FriendManager.Instance != null)
            FriendManager.Instance.CheckFriendListEmpty();

        currentUserId = "";
        friendUserId  = "";
    }
    void PlayButtonSound()
{
    if (buttonAudioSource != null && buttonClickSound != null)
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
    }
}
}
