using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
public class FriendCard : MonoBehaviour
{
    public Image avatarImage;
    public TMP_Text nameText;
    public Image flagImage;
    private string friendUserId;
    private ListenerRegistration userListener;
    private List<Sprite> avatarSprites;
    private List<Sprite> flagSprites;
    public GameObject onlineIndicator;
    public GameObject offlineIndicator;
    public TMP_Text lastSeenText;
    public Button avatarButton;
public Button deleteButton;
[Header("Delete Button Sound")]
public AudioSource buttonAudioSource;
public AudioClip buttonClickSound;
    // ✅ Cache data for instant profile load
    private string cachedName = "";
    private string cachedAvatar = "avatar1";
    private int cachedCountryIndex = 0;
public GameObject inGameIndicator;



    public void Setup(
        string friendId,
        List<Sprite> avatarSpriteList,
        List<Sprite> flagSpriteList)
    {
        friendUserId = friendId;
        avatarSprites = avatarSpriteList;
        flagSprites = flagSpriteList;

        if (avatarButton == null)
            avatarButton = GetComponent<Button>();

        if (avatarButton != null)
        {
            avatarButton.onClick.RemoveAllListeners();
            avatarButton.onClick.AddListener(OpenProfile);
            if (onlineIndicator != null) onlineIndicator.SetActive(false);
if (offlineIndicator != null) offlineIndicator.SetActive(true);
if (lastSeenText != null) lastSeenText.gameObject.SetActive(false);
        }
if (deleteButton != null)
{
    deleteButton.onClick.RemoveAllListeners();
    deleteButton.onClick.AddListener(OnDeleteClicked);
}
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        userListener = db.Collection("users").Document(friendUserId)
            .Listen(snapshot =>
        {
            if (snapshot == null || !snapshot.Exists) return;

            // ✅ Cache the data
            cachedName = snapshot.ContainsField("name") ? snapshot.GetValue<string>("name") : "Unknown";
            cachedAvatar = snapshot.ContainsField("avatar") ? snapshot.GetValue<string>("avatar") : "avatar1";
            cachedCountryIndex = snapshot.ContainsField("countryIndex") ? snapshot.GetValue<int>("countryIndex") : 0;

            if (nameText != null) nameText.text = cachedName;

            int avatarIndex = int.Parse(cachedAvatar.Replace("avatar", "")) - 1;
            if (avatarImage != null && avatarSprites != null && avatarIndex >= 0 && avatarIndex < avatarSprites.Count)
                avatarImage.sprite = avatarSprites[avatarIndex];

            if (flagImage != null && flagSprites != null && cachedCountryIndex >= 0 && cachedCountryIndex < flagSprites.Count)
                flagImage.sprite = flagSprites[cachedCountryIndex];

            bool isOnline = snapshot.ContainsField("isOnline") && snapshot.GetValue<bool>("isOnline");
         string currentScene = snapshot.ContainsField("currentScene") ? snapshot.GetValue<string>("currentScene") : "";


            System.DateTime lastSeen = System.DateTime.UtcNow;
if (snapshot.ContainsField("lastSeen"))
{
    Timestamp ts = snapshot.GetValue<Timestamp>("lastSeen");
    lastSeen = ts.ToDateTime();
}
System.TimeSpan diff2 = System.DateTime.UtcNow - lastSeen;
bool trulyOnline = isOnline && diff2.TotalSeconds < 30;

if (!trulyOnline)
{
    if (onlineIndicator  != null) onlineIndicator.SetActive(false);
    if (inGameIndicator  != null) inGameIndicator.SetActive(false);
    if (offlineIndicator != null) offlineIndicator.SetActive(true);
}
else if (currentScene != "Main Menu" && !string.IsNullOrEmpty(currentScene))
{
    if (onlineIndicator  != null) onlineIndicator.SetActive(false);
    if (offlineIndicator != null) offlineIndicator.SetActive(false);
    if (inGameIndicator  != null) inGameIndicator.SetActive(true);
}
else
{
    if (offlineIndicator != null) offlineIndicator.SetActive(false);
    if (inGameIndicator  != null) inGameIndicator.SetActive(false);
    if (onlineIndicator  != null) onlineIndicator.SetActive(true);
}

            if (lastSeenText != null)
            {
               if (trulyOnline)
                {
                    lastSeenText.gameObject.SetActive(false);
                }
                else
                {
                    lastSeenText.gameObject.SetActive(true);
                    if (snapshot.ContainsField("lastSeen"))
                    {
                        Timestamp ts = snapshot.GetValue<Timestamp>("lastSeen");
                        System.DateTime lastSeenTime = ts.ToDateTime();
                        System.TimeSpan diff = System.DateTime.UtcNow - lastSeenTime;
                        if (diff.TotalSeconds < 0) diff = System.TimeSpan.Zero;

                      if (diff.TotalSeconds < 60)
    lastSeenText.text = (int)diff.TotalSeconds + "s ago";
else if (diff.TotalMinutes < 60)
    lastSeenText.text = (int)diff.TotalMinutes + "m ago";
else if (diff.TotalHours < 24)
    lastSeenText.text = (int)diff.TotalHours + "h ago";
else
    lastSeenText.text = (int)diff.TotalDays + "d ago";
                    }
                    else
                    {
                        lastSeenText.text = "";
                    }
                }
            }
        });
    }

    void OnDestroy()
    {
        if (userListener != null)
        {
            userListener.Stop();
            userListener = null;
        }
    }

    void OpenProfile()
    {
        inboxcontroller controller = FindObjectOfType<inboxcontroller>(true);
        if (controller != null) controller.openprofie();

        // ✅ Instant - uses cached data no Firestore call!
        ProfilePanel.Instance.LoadProfileInstant(
            friendUserId,
            cachedName,
            cachedAvatar,
            cachedCountryIndex,
            avatarSprites,
            flagSprites
        );
    }
    void OnDeleteClicked()
{
      if (buttonAudioSource != null && buttonClickSound != null)
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
    }
    string myId = AuthManager.Instance.userId;
    if (DeleteFriendPanel.Instance != null)
        DeleteFriendPanel.Instance.Open(myId, friendUserId, this.gameObject);
    else
        Debug.LogError("DeleteFriendPanel instance not found!");
}

}
