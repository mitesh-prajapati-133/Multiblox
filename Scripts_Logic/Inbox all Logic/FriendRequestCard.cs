using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;
using System;
public class FriendRequestCard : MonoBehaviour
{
     public TMP_Text nameText;
    public TMP_Text timeAgoText;
    public Image avatarImage;
    public Image flagImage;
    public GameObject acceptButton;
    public GameObject rejectButton;
private Timestamp savedTimestamp;
    private string docId;
    private FriendManager friendManager;


    public Button profileAvatarButton;
private string cachedName = "";
private string cachedAvatar = "avatar1";
private int cachedCountryIndex = 0;
private string cachedUserId = "";
private List<Sprite> cachedAvatarSprites;
private List<Sprite> cachedFlagSprites;

    public void Setup(
        string fromName,
        string fromUserId,
        string fromAvatar,
        int fromCountryIndex,
        Timestamp timestamp,
        string requestDocId,
        List<Sprite> avatarSprites,
        List<Sprite> flagSprites,
        FriendManager manager)
    {
        docId = requestDocId;
        cachedName = fromName;
cachedAvatar = fromAvatar;
cachedCountryIndex = fromCountryIndex;
cachedUserId = fromUserId;
cachedAvatarSprites = avatarSprites;
cachedFlagSprites = flagSprites;

if (profileAvatarButton == null)
    profileAvatarButton = GetComponent<Button>();

if (profileAvatarButton != null)
{
    profileAvatarButton.onClick.RemoveAllListeners();
    profileAvatarButton.onClick.AddListener(OpenProfile);
}
        friendManager = manager;

        // Set name
        nameText.text = fromName;

        // Set time ago
    savedTimestamp = timestamp;
StartCoroutine(UpdateTimeLoop());

        // Set avatar
        int avatarIndex = int.Parse(fromAvatar.Replace("avatar", "")) - 1;
        if (avatarIndex >= 0 && avatarIndex < avatarSprites.Count)
            avatarImage.sprite = avatarSprites[avatarIndex];

        // Set flag
        if (fromCountryIndex >= 0 && fromCountryIndex < flagSprites.Count)
            flagImage.sprite = flagSprites[fromCountryIndex];
    }
IEnumerator UpdateTimeLoop()
{
    while (true)
    {
        DateTime requestTime = savedTimestamp.ToDateTime().ToLocalTime();
        timeAgoText.text = GetTimeAgo(requestTime);
        yield return new WaitForSeconds(60f);
    }
}
    // ─────────────────────────────────────────
    // Accept Button
    // ─────────────────────────────────────────


    void OpenProfile()
{
    inboxcontroller controller = FindObjectOfType<inboxcontroller>(true);
    if (controller != null) controller.openprofie();

    ProfilePanel.Instance.LoadProfileInstant(
        cachedUserId,
        cachedName,
        cachedAvatar,
        cachedCountryIndex,
        cachedAvatarSprites,
        cachedFlagSprites
    );
}
    public void OnAcceptClicked()
    {
        friendManager.AcceptRequest(docId, this.gameObject);
    }

    // ─────────────────────────────────────────
    // Reject (X) Button
    // ─────────────────────────────────────────
    public void OnRejectClicked()
    {
        friendManager.RejectRequest(docId, this.gameObject);
    }

    // ─────────────────────────────────────────
    // Time ago helper
    // ─────────────────────────────────────────
    string GetTimeAgo(DateTime pastTime)
    {
        TimeSpan diff = DateTime.Now - pastTime;

        if (diff.TotalMinutes < 1)
            return "Just now";
        else if (diff.TotalMinutes < 60)
            return Mathf.FloorToInt((float)diff.TotalMinutes) + "m ago";
        else if (diff.TotalHours < 24)
            return Mathf.FloorToInt((float)diff.TotalHours) + "h ago";
        else
            return Mathf.FloorToInt((float)diff.TotalDays) + "d ago";
    }
}
