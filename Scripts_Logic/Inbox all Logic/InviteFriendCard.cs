using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using Photon.Pun;
public class InviteFriendCard : MonoBehaviour
{
 public Image avatarImage;
    public TMP_Text nameText;
    public Image flagImage;
    public GameObject onlineIndicator;
    public GameObject offlineIndicator;
    public Button inviteButton;
    public TMP_Text statusText;
public bool IsOnline { get; private set; } = false;
public System.Action<InviteFriendCard> OnStatusChanged;
    private List<Sprite> avatarSprites;
    private List<Sprite> flagSprites;
    private ListenerRegistration onlineListener;
    public GameObject inGameIndicator; 
    private const string MAIN_MENU_SCENE = "Main menu";
public string friendId;
private bool isCoolingDown = false; 

[Header("Invite Button Sound")]
public AudioSource buttonAudioSource;
public AudioClip buttonClickSound;
    public void Setup(string friendId, List<Sprite> avatarSpriteList, List<Sprite> flagSpriteList)
    {
        this.friendId = friendId;
        if (inviteButton != null)
{
    inviteButton.onClick.RemoveAllListeners();
    inviteButton.onClick.AddListener(OnInviteClicked);
}
        avatarSprites = avatarSpriteList;
        flagSprites = flagSpriteList;

        // ✅ Set offline by default immediately on start
        if (onlineIndicator != null) onlineIndicator.SetActive(false);
        if (offlineIndicator != null) offlineIndicator.SetActive(true);
        if (inviteButton != null) inviteButton.interactable = false;
        if (statusText != null)
        {
            statusText.text = "Offline";
            statusText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

       onlineListener = db.Collection("users").Document(friendId)
    .Listen(snapshot =>
{
    if (snapshot == null || !snapshot.Exists) return;

    string fetchedName = snapshot.ContainsField("name") ? snapshot.GetValue<string>("name") : "Unknown";
    string avatar = snapshot.ContainsField("avatar") ? snapshot.GetValue<string>("avatar") : "avatar1";
    int countryIndex = snapshot.ContainsField("countryIndex") ? snapshot.GetValue<int>("countryIndex") : 0;
    bool isOnline = snapshot.ContainsField("isOnline") && snapshot.GetValue<bool>("isOnline");
string currentScene = snapshot.ContainsField("currentScene") ? snapshot.GetValue<string>("currentScene") : "";
    System.DateTime lastSeen = System.DateTime.UtcNow;
    if (snapshot.ContainsField("lastSeen"))
    {
        Timestamp ts = snapshot.GetValue<Timestamp>("lastSeen");
        lastSeen = ts.ToDateTime();
    }

  StartCoroutine(UpdateUI(fetchedName, avatar, countryIndex, isOnline, currentScene, lastSeen));
});
    }

   IEnumerator UpdateUI(string fetchedName, string avatar, int countryIndex, bool isOnline, string currentScene, System.DateTime lastSeen)
    {
        yield return null; // wait one frame → ensures main thread

        if (this == null || gameObject == null) yield break;

        if (nameText != null) nameText.text = fetchedName;

        int avatarIndex = int.Parse(avatar.Replace("avatar", "")) - 1;
        if (avatarImage != null && avatarSprites != null && avatarIndex >= 0 && avatarIndex < avatarSprites.Count)
            avatarImage.sprite = avatarSprites[avatarIndex];

        if (flagImage != null && flagSprites != null && countryIndex >= 0 && countryIndex < flagSprites.Count)
            flagImage.sprite = flagSprites[countryIndex];
System.TimeSpan diff = System.DateTime.UtcNow - lastSeen;
bool trulyOnline = isOnline && diff.TotalSeconds < 30;

if (!trulyOnline)
{
    if (onlineIndicator  != null) onlineIndicator.SetActive(false);
    if (inGameIndicator  != null) inGameIndicator.SetActive(false);
    if (offlineIndicator != null) offlineIndicator.SetActive(true);
    if (inviteButton     != null) inviteButton.interactable = false;
    if (statusText       != null) { statusText.text = "Offline"; statusText.color = new Color(0.5f,0.5f,0.5f,1f); }
    IsOnline = false;
}
else if (currentScene != MAIN_MENU_SCENE && !string.IsNullOrEmpty(currentScene))
{
    if (onlineIndicator  != null) onlineIndicator.SetActive(false);
    if (offlineIndicator != null) offlineIndicator.SetActive(false);
    if (inGameIndicator  != null) inGameIndicator.SetActive(true);
    if (inviteButton     != null) inviteButton.interactable = false;
    if (statusText       != null) { statusText.text = "In Game"; statusText.color = new Color(0.5f,0.5f,0.5f,1f); }
    IsOnline = false;
}
else
{
    if (offlineIndicator != null) offlineIndicator.SetActive(false);
    if (inGameIndicator  != null) inGameIndicator.SetActive(false);
    if (onlineIndicator  != null) onlineIndicator.SetActive(true);
    if (inviteButton     != null) inviteButton.interactable = true;
    if (statusText       != null) { statusText.text = "Online"; statusText.color = Color.green; }
    IsOnline = true;
}

        Debug.Log("✅ UI updated: " + fetchedName + " trulyOnline: " + trulyOnline);
    
    }

    void OnDestroy()
    {
        if (onlineListener != null)
        {
            onlineListener.Stop();
            onlineListener = null;
        }
    }
    void OnInviteClicked()
{
        if (inviteButton != null && inviteButton.interactable)
    {
        if (buttonAudioSource != null && buttonClickSound != null)
        {
            buttonAudioSource.PlayOneShot(buttonClickSound);
        }
    }

 string myId = AuthManager.Instance.userId;
    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

    // ✅ Step 1 — Host creates room first
    RoomManager.Instance.HostGame();

    // ✅ Step 2 — Wait for room to be created then send invite
    StartCoroutine(SendInviteAfterRoomCreated(myId, db));
}
IEnumerator SendInviteAfterRoomCreated(string myId, FirebaseFirestore db)
{
    // Wait until room is created and activeRoomCode is saved
    float timeout = 10f;
    float elapsed = 0f;

    while (elapsed < timeout)
    {
        if (PhotonNetwork.InRoom && !string.IsNullOrEmpty(PhotonNetwork.CurrentRoom.Name))
            break;

        elapsed += Time.deltaTime;
        yield return null;
    }

    if (!PhotonNetwork.InRoom)
    {
        Debug.Log("❌ Room creation timed out");
        yield break;
    }

    string roomCode = PhotonNetwork.CurrentRoom.Name;
string docId = System.Guid.NewGuid().ToString();

    // ✅ Step 3 — Send invite with roomCode
    Dictionary<string, object> inviteData = new Dictionary<string, object>()
    {
        { "fromUserId", myId },
        { "toUserId", friendId },
        { "status", "pending" },
        { "timestamp", FieldValue.ServerTimestamp },
        { "fromName", AuthManager.Instance.playerNameText.text },
        { "fromAvatar", AuthManager.Instance.selectedAvatarId },
        { "fromCountryIndex", PlayerPrefs.GetInt("selectedCountry", 0) },
        { "roomCode", roomCode } // ✅ store roomCode directly in invite
    };

    db.Collection("invitations").Document(docId).SetAsync(inviteData)
        .ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
           {
             Debug.Log("✅ Invite sent with roomCode: " + roomCode);
            invitationpanel panel = FindObjectOfType<invitationpanel>(true);
if (panel != null) panel.ListenForRejection(docId);
    }
        else
            Debug.Log("❌ Failed to send invite");
    });

    // ✅ Step 4 — Sender loads to lobby
    StartCoroutine(CooldownInviteButton());
       
}
IEnumerator CooldownInviteButton()
{
       isCoolingDown = true;

    if (inviteButton != null)
        inviteButton.interactable = false;

    yield return new WaitForSeconds(8f);

    isCoolingDown = false;

    if (IsOnline && !isCoolingDown && inviteButton != null)
        inviteButton.interactable = true;
}
}

