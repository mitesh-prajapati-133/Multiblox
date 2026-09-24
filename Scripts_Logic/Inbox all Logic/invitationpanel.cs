using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using Photon.Pun;
public class invitationpanel : MonoBehaviour
{
   public GameObject invitationPanel;
    public TMP_Text nameText;
    public Image avatarImage;
    public Image flagImage;
    public List<Sprite> avatarSprites;
    public List<Sprite> flagSprites;

    private ListenerRegistration listener;
    private string currentDocId;
    private Coroutine hideCoroutine;
public GameObject rejectedPanel;      // new panel showing rejection message
public TMP_Text rejectedMessageText;  // text inside that panel
private ListenerRegistration sentInviteListener; // listens for rejection
private Coroutine rejectedHideCoroutine;
[Header("=== INVITE SOUND ===")]
public AudioSource inviteAudioSource;
public AudioClip inviteSound;
    IEnumerator Start()
    {
        // Don't show panel on start
        if (invitationPanel != null) invitationPanel.SetActive(false);

        // Wait until userId is ready
        while (string.IsNullOrEmpty(AuthManager.Instance.userId))
            yield return null;

        string myId = AuthManager.Instance.userId;
        Debug.Log("🟢 Listener started for: " + myId);

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        listener = db.Collection("invitations")
            .WhereEqualTo("toUserId", myId)
            .WhereEqualTo("status", "pending")
            .Listen(snapshot =>
        {
            Debug.Log("🔵 Snapshot received. Count: " + snapshot.Count);

            var firstDoc = snapshot.Documents.FirstOrDefault();
           if (firstDoc != null)
{
    if (invitationPanel.activeSelf) return; // 🔥 ADD THIS
    ShowInvite(firstDoc);
}
        });
    }

    void ShowInvite(DocumentSnapshot doc)
    {
        // Prevent duplicate popup
        if (invitationPanel.activeSelf && currentDocId == doc.Id) return;
   if (doc.ContainsField("timestamp"))
    {
        Timestamp ts = doc.GetValue<Timestamp>("timestamp");
        System.DateTime sentTime = ts.ToDateTime();
        if ((System.DateTime.UtcNow - sentTime).TotalSeconds > 30)
        {
            // Delete stale invite silently
            FirebaseFirestore.DefaultInstance
                .Collection("invitations")
                .Document(doc.Id)
                .DeleteAsync();
            return;
        }
    }
        currentDocId = doc.Id;

        string name = doc.ContainsField("fromName") ? doc.GetValue<string>("fromName") : "Unknown";
        string avatar = doc.ContainsField("fromAvatar") ? doc.GetValue<string>("fromAvatar") : "avatar1";
        int countryIndex = doc.ContainsField("fromCountryIndex") ? doc.GetValue<int>("fromCountryIndex") : 0;

        // Update UI on main thread
        nameText.text = name;

        int index = int.Parse(avatar.Replace("avatar", "")) - 1;
        if (avatarImage != null && index >= 0 && index < avatarSprites.Count)
            avatarImage.sprite = avatarSprites[index];

        // Set flag
        if (flagImage != null && countryIndex >= 0 && countryIndex < flagSprites.Count)
            flagImage.sprite = flagSprites[countryIndex];

        invitationPanel.SetActive(true);
if (inviteAudioSource != null && inviteSound != null)
{
    inviteAudioSource.loop = false;
    inviteAudioSource.PlayOneShot(inviteSound);
}
        // Auto hide after 5 seconds
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay(5f));

        Debug.Log("📩 Invite received from: " + name);
    }

    IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        HidePanel(true);
    }

  void HidePanel(bool delete = true)
{
    if (invitationPanel != null)
        invitationPanel.SetActive(false);

    if (delete && !string.IsNullOrEmpty(currentDocId))
    {
        FirebaseFirestore.DefaultInstance
            .Collection("invitations")
            .Document(currentDocId)
            .DeleteAsync();
    }

    currentDocId = null;
}

    // Cross button → hide immediately
    public void RejectInvite()
    {
          if (hideCoroutine != null) StopCoroutine(hideCoroutine);

    // ── NEW: write rejected status so sender gets notified ──
    if (!string.IsNullOrEmpty(currentDocId))
    {
        FirebaseFirestore.DefaultInstance.Collection("invitations")
            .Document(currentDocId)
            .SetAsync(new Dictionary<string, object> {
                { "status", "rejected" },
                { "rejectedAt", FieldValue.ServerTimestamp }
            }, SetOptions.MergeAll);
    }

    HidePanel(false); // don't delete yet — sender needs to read it first
    Debug.Log("❌ Invite rejected");

    }
public void AcceptInvite()
{
    if (hideCoroutine != null) StopCoroutine(hideCoroutine);

    string docId = currentDocId;
    if (string.IsNullOrEmpty(docId)) return;

    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

    db.Collection("invitations").Document(docId)
        .GetSnapshotAsync()
        .ContinueWithOnMainThread(task =>
    {
        if (task.Exception != null || !task.Result.Exists)
        {
            Debug.Log("❌ Could not read invite doc");
            return;
        }

        if (!task.Result.ContainsField("roomCode"))
        {
            Debug.Log("❌ No roomCode in invite");
            return;
        }

        string roomCode = task.Result.GetValue<string>("roomCode");
        Debug.Log("✅ Joining room: " + roomCode);

        // ✅ Delete invite
        db.Collection("invitations").Document(docId).DeleteAsync();

        // ✅ Join safely
        StartCoroutine(JoinRoomSafely(roomCode));
    });

    // ✅ Close panel without deleting again
    HidePanel(false);
}
IEnumerator JoinRoomSafely(string roomCode)
{
    float timeout = 10f;
    float t = 0;

    while (!PhotonNetwork.IsConnected && t < timeout)
    {
        t += Time.deltaTime;
        yield return null;
    }

    // Wait for lobby if not already in one
    if (!PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
    {
        PhotonNetwork.JoinLobby();
        t = 0;
        while (!PhotonNetwork.InLobby && t < timeout)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    if (PhotonNetwork.IsConnected)
    {
        RoomManager.Instance.JoinGame(roomCode);
    }
    else
    {
        Debug.Log("❌ Photon not ready");
    }
}
public void ListenForRejection(string docId)
{
    if (sentInviteListener != null) { sentInviteListener.Stop(); sentInviteListener = null; }

    FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
    sentInviteListener = db.Collection("invitations").Document(docId)
        .Listen(snapshot =>
    {
        if (snapshot == null || !snapshot.Exists) return;

        if (!snapshot.ContainsField("status")) return;
        string status = snapshot.GetValue<string>("status");

        // Check timestamp — ignore stale docs
        if (snapshot.ContainsField("timestamp"))
        {
            Timestamp ts = snapshot.GetValue<Timestamp>("timestamp");
            System.DateTime sentTime = ts.ToDateTime();
            if ((System.DateTime.UtcNow - sentTime).TotalSeconds > 30) return;
        }

        if (status == "rejected")
        {
            StartCoroutine(ShowRejectedPanel());
           var listenerRef = sentInviteListener;
sentInviteListener = null;
listenerRef?.Stop();
        }
    });
}

IEnumerator ShowRejectedPanel()
{
    yield return null; // main thread

    if (rejectedPanel == null) yield break;

    if (rejectedMessageText != null)
        rejectedMessageText.text = "Player has rejected your invite!!";

    rejectedPanel.SetActive(true);
    PlayerPrefs.SetString("inviteRejected", "true");

    if (rejectedHideCoroutine != null) StopCoroutine(rejectedHideCoroutine);
    rejectedHideCoroutine = StartCoroutine(HideRejectedAfterDelay(5f));
}

IEnumerator HideRejectedAfterDelay(float seconds)
{
    yield return new WaitForSeconds(seconds);
    if (rejectedPanel != null) rejectedPanel.SetActive(false);
}
    void OnDestroy()
    {
       if (listener != null) { listener.Stop(); listener = null; }
    if (sentInviteListener != null) { sentInviteListener.Stop(); sentInviteListener = null; }
    }
}
