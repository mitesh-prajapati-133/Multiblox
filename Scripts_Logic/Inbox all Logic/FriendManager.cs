        using System.Collections;
        using System.Collections.Generic;
        using UnityEngine;
        using Firebase.Firestore;
        using Firebase.Extensions;
        using TMPro;
        using UnityEngine.UI;
        using System;
        public class FriendManager : MonoBehaviour
        {
            public static FriendManager Instance;

            [Header("=== ADD FRIEND PANEL ===")]
            public TMP_InputField searchInputField;
            public GameObject searchResultPanel;      // The card that shows found user
            public TMP_Text resultNameText;
            public TMP_Text resultIdText;
            public Image resultAvatarImage;
            public Image resultFlagImage;
            public GameObject addButton;              // The ADD (+) button
            public GameObject tickObject;            // The tick image (shown after request sent)
            public GameObject notFoundText;          // "User not found" text

            [Header("=== NOTIFICATION PANEL ===")]
            public Transform requestListParent;      // The scroll content where request cards go
            public GameObject requestCardPrefab;     // Prefab for each friend request card
            public GameObject noRequestsText;        // "No requests" text

            [Header("=== AVATAR SPRITES ===")]
            public List<Sprite> avatarSprites;       // Same 12 avatar sprites as in AuthManager
            public List<Sprite> flagSprites;         // All country flag sprites

            private FirebaseFirestore db;
            private string currentUserId;
            private string foundUserId;     
            
            [Header("=== FRIENDS INBOX PANEL ===")]
        public Transform friendListParent;      // scroll content in inbox panel
        public GameObject friendCardPrefab;     // friend card prefab
        public GameObject noFriendsText;        // "No friends yet" text
        private ListenerRegistration friendsListener;
            
            
            
                    // ID of user found in search
        public GameObject staticRequestCard;
        public GameObject staticFriendCard;  
        [Header("=== INVITE PANEL ===")]
    public Transform inviteListParent;
    public GameObject inviteCardPrefab;
    public GameObject noInviteFriendsText;
    public GameObject staticInviteCard;
    public GameObject notificationBadge; // the red circle GameObject
public TMP_Text notificationCountText; 
        private ListenerRegistration inviteListener;
        private ListenerRegistration badgeListener;
        


        [Header("=== FRIEND SEARCH ===")]
public TMP_InputField friendSearchInput;
            void Awake()
            {
                Instance = this;
            }

            void Start()
            {
                db = FirebaseFirestore.DefaultInstance;

                // Hide result panel and notFound by default
                if (searchResultPanel != null) searchResultPanel.SetActive(false);
                if (notFoundText != null) notFoundText.SetActive(false);
                if (tickObject != null) tickObject.SetActive(false);
                if (addButton != null) addButton.SetActive(true);
                 if (friendSearchInput != null)
    {
        friendSearchInput.onValueChanged.AddListener(FilterFriendList);
    }
            }

            // ─────────────────────────────────────────
            // Called when Search button clicked
            // ─────────────────────────────────────────
            public void OnSearchClicked()
            {
                if (notFoundText != null)
            notFoundText.GetComponent<TMP_Text>().text = "NOT FOUND!!";
                string searchId = searchInputField.text.Trim();
                    Debug.Log("Search clicked! ID = " + searchId);

                if (string.IsNullOrEmpty(searchId))
                {
                    Debug.Log("Search field is empty.");
                    return;
                }

                currentUserId = AuthManager.Instance.userId;

                // Can't search yourself
                if (searchId == currentUserId)
                {
                    if (notFoundText != null) notFoundText.SetActive(true);
                    if (searchResultPanel != null) searchResultPanel.SetActive(false);
                    return;
                }

                // Search in Firestore users collection
                db.Collection("users").Document(searchId).GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                {
                    if (task.Result.Exists)
                    {
                        foundUserId = searchId;

                        // Get data
                        string name = task.Result.ContainsField("name")
                            ? task.Result.GetValue<string>("name") : "Unknown";

                        string avatarId = task.Result.ContainsField("avatar")
                            ? task.Result.GetValue<string>("avatar") : "avatar1";

                        int countryIndex = task.Result.ContainsField("countryIndex")
                            ? task.Result.GetValue<int>("countryIndex") : 0;

                        // Update UI
                        resultNameText.text = name;
                        resultIdText.text = "ID: " + searchId;

                        // Set avatar sprite
                        int avatarIndex = int.Parse(avatarId.Replace("avatar", "")) - 1;
                        if (avatarIndex >= 0 && avatarIndex < avatarSprites.Count)
                            resultAvatarImage.sprite = avatarSprites[avatarIndex];

                        // Set flag sprite
                        if (countryIndex >= 0 && countryIndex < flagSprites.Count)
                            resultFlagImage.sprite = flagSprites[countryIndex];

                        // Show result, hide not found
                        searchResultPanel.SetActive(true);
                        notFoundText.SetActive(false);

                        // Reset add/tick state
            addButton.SetActive(false);
        tickObject.SetActive(false);

                        // Check if request already sent
                        CheckIfRequestAlreadySent(currentUserId, foundUserId);
                    }
                    else
                    {
                        // Not found
                        searchResultPanel.SetActive(false);
                        notFoundText.SetActive(true);
                    }
                });
            }

            // ─────────────────────────────────────────
            // Check if already sent request
            // ─────────────────────────────────────────
            void CheckIfRequestAlreadySent(string fromId, string toId)
            {
                string docId = fromId + "_" + toId;

            // First check if already friends
            string friendDocId = fromId + "_" + toId;
            db.Collection("friends").Document(friendDocId).GetSnapshotAsync()
                .ContinueWithOnMainThread(friendTask =>
            {
                    if (friendTask.Exception != null)
                {
                    // Error checking friends → just show add button
                    addButton.SetActive(true);
                    tickObject.SetActive(false);
                    return;
                }
                if (friendTask.Result.Exists)
                {
                    // Already friends → hide add button completely
                    addButton.SetActive(false);
                    tickObject.SetActive(false);
                    if (notFoundText != null)
                    {
                        notFoundText.gameObject.SetActive(true);
                        notFoundText.GetComponent<TMP_Text>().text = "Already Friends ✅";
                    }
                    return;
                }

                // Not friends → check if request already sent
                db.Collection("friendRequests").Document(docId).GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                {
                    if (task.Result.Exists)
                    {
                        // Already sent → show tick
                        addButton.SetActive(false);
                        tickObject.SetActive(true);
                    }

                    else
                    {
                        // Not sent → show add button
                        addButton.SetActive(true);
                        tickObject.SetActive(false);
                    }
                });
            });
            }

            // ─────────────────────────────────────────
            // Called when ADD button clicked
            // ─────────────────────────────────────────
            public void OnAddFriendButtonClicked()
            {
                if (string.IsNullOrEmpty(foundUserId)) return;

                currentUserId = AuthManager.Instance.userId;

                string docId = currentUserId + "_" + foundUserId;

                // Get sender's details from AuthManager
                string senderName = AuthManager.Instance.playerNameText.text;
                string senderAvatar = AuthManager.Instance.selectedAvatarId;
                string senderFrame = AuthManager.Instance.selectedFrameId;
                int senderCountry = PlayerPrefs.GetInt("selectedCountry", 0);

                Dictionary<string, object> requestData = new Dictionary<string, object>()
                {
                    { "fromUserId", currentUserId },
                    { "toUserId", foundUserId },
                    { "status", "pending" },
                    { "timestamp", FieldValue.ServerTimestamp },
                    { "fromName", senderName },
                    { "fromAvatar", senderAvatar },
                    { "fromFrame", senderFrame },
                    { "fromCountryIndex", senderCountry }
                };

                db.Collection("friendRequests").Document(docId).SetAsync(requestData)
                    .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted)
                    {
                        // Show tick, hide add button
                        addButton.SetActive(false);
                        tickObject.SetActive(true);
                        Debug.Log("Friend request sent ✅");
                    }
                    else
                    {
                        Debug.Log("Failed to send request ❌");
                    }
                });
            }
        private ListenerRegistration requestListener;
            // ─────────────────────────────────────────
            // Called when Notification bell clicked
            // ─────────────────────────────────────────
            public void LoadFriendRequests()
            {
                if (staticRequestCard != null) staticRequestCard.SetActive(false);

            currentUserId = AuthManager.Instance.userId;

            if (string.IsNullOrEmpty(currentUserId))
            {
                Debug.Log("UserId not ready ❌");
                return;
            }

            // ✅ Stop previous listener if any
            if (requestListener != null)
            {
                requestListener.Stop();
                requestListener = null;
            }

            Debug.Log("🔍 Listening for requests: " + currentUserId);

            requestListener = db.Collection("friendRequests")
                .WhereEqualTo("toUserId", currentUserId)
                .Listen(snapshot =>
            {
                // Clear existing cards
                foreach (Transform child in requestListParent)
                    Destroy(child.gameObject);

                int count = 0;

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    if (!doc.ContainsField("status")) continue;
                    if (doc.GetValue<string>("status") != "pending") continue;

                    string fromUserId = doc.GetValue<string>("fromUserId");
                    string requestDocId = doc.Id;
                    Timestamp timestamp = doc.ContainsField("timestamp")
                        ? doc.GetValue<Timestamp>("timestamp")
                        : Timestamp.GetCurrentTimestamp();

                    db.Collection("users").Document(fromUserId).GetSnapshotAsync()
                        .ContinueWithOnMainThread(userTask =>
                    {
                        if (userTask.Exception != null) return;

                        DocumentSnapshot userDoc = userTask.Result;
                        if (!userDoc.Exists) return;

                        string latestName = userDoc.ContainsField("name")
                            ? userDoc.GetValue<string>("name") : "Unknown";

                        string latestAvatar = userDoc.ContainsField("avatar")
                            ? userDoc.GetValue<string>("avatar") : "avatar1";

                        int latestCountry = userDoc.ContainsField("countryIndex")
                            ? userDoc.GetValue<int>("countryIndex") : 0;

                        GameObject card = Instantiate(requestCardPrefab, requestListParent);
                        card.SetActive(true);

                        FriendRequestCard cardScript = card.GetComponent<FriendRequestCard>();
                        cardScript.Setup(
                            latestName,
                            fromUserId,
                            latestAvatar,
                            latestCountry,
                            timestamp,
                            requestDocId,
                            avatarSprites,
                            flagSprites,
                            this
                        );
                    });

                    count++;
                }

                if (count == 0)
                {
                    Debug.Log("❌ No friend requests found");
                    if (noRequestsText != null)
                        noRequestsText.SetActive(true);
                         // ← add this
                }
                else
                {
                    Debug.Log("🎉 Total requests: " + count);
                    if (noRequestsText != null)
                        noRequestsText.SetActive(false);
                     
                }
            });
            }
        void CheckIfListEmpty()
        {
            int remaining = requestListParent.childCount;
            if (noRequestsText != null)
                noRequestsText.SetActive(remaining == 0);
        }
            // ─────────────────────────────────────────
            // Accept friend request
            // ─────────────────────────────────────────
            public void AcceptRequest(string docId, GameObject card)
            {
                
            GameObject cardRef = card;

            Debug.Log("🔄 AcceptRequest called for docId: " + docId);

            db.Collection("friendRequests").Document(docId).GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
            {
                if (task.Exception != null)
                {
                    Debug.LogError("❌ Error reading friendRequest: " + task.Exception);
                    return;
                }

                if (!task.Result.Exists)
                {
                    Debug.LogError("❌ friendRequest doc not found: " + docId);
                    return;
                }

                string fromUserId = task.Result.GetValue<string>("fromUserId");
                string toUserId = task.Result.GetValue<string>("toUserId");

                Debug.Log("✅ fromUserId: " + fromUserId + " toUserId: " + toUserId);

                Dictionary<string, object> friendData1 = new Dictionary<string, object>()
                {
                    { "userId", fromUserId },
                    { "friendId", toUserId },
                    { "since", FieldValue.ServerTimestamp }
                };

                Dictionary<string, object> friendData2 = new Dictionary<string, object>()
                {
                    { "userId", toUserId },
                    { "friendId", fromUserId },
                    { "since", FieldValue.ServerTimestamp }
                };

                string friendDoc1 = fromUserId + "_" + toUserId;
                string friendDoc2 = toUserId + "_" + fromUserId;

                Debug.Log("📝 Writing friends: " + friendDoc1 + " and " + friendDoc2);

                db.Collection("friends").Document(friendDoc1).SetAsync(friendData1)
                    .ContinueWithOnMainThread(t1 =>
                {
                    if (t1.Exception != null)
                        Debug.LogError("❌ Failed writing friendDoc1: " + t1.Exception);
                    else
                        Debug.Log("✅ friendDoc1 written");
                });

                db.Collection("friends").Document(friendDoc2).SetAsync(friendData2)
                    .ContinueWithOnMainThread(t2 =>
                {
                    if (t2.Exception != null)
                        Debug.LogError("❌ Failed writing friendDoc2: " + t2.Exception);
                    else
                        Debug.Log("✅ friendDoc2 written");
                });

                db.Collection("friendRequests").Document(docId).DeleteAsync()
                    .ContinueWithOnMainThread(deleteTask =>
                {
                    if (deleteTask.Exception != null)
                    {
                        Debug.LogError("❌ Failed deleting request: " + deleteTask.Exception);
                        return;
                    }

                    Debug.Log("✅ Friend request deleted, friendship saved!");
                    DestroyImmediate(cardRef);
                    CheckIfListEmpty();
                });
            });
            }

            // ─────────────────────────────────────────
            // Reject friend request
            // ─────────────────────────────────────────
            public void RejectRequest(string docId, GameObject card)
            {
                db.Collection("friendRequests").Document(docId).DeleteAsync()
                .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Request rejected and deleted ✅");
            DestroyImmediate(card);
                        CheckIfListEmpty();
                }
            });
            }

            // ─────────────────────────────────────────
            // Reset Add Friend Panel when opened
            // ─────────────────────────────────────────
            public void ResetAddFriendPanel()
            {
                if (searchInputField != null) searchInputField.text = "";
        if (searchResultPanel != null) searchResultPanel.SetActive(false);
        if (notFoundText != null)
        {
            notFoundText.SetActive(false);
            notFoundText.GetComponent<TMP_Text>().text = "NOT FOUND!!";  // ✅ reset text
        }
        if (addButton != null) addButton.SetActive(true);
        if (tickObject != null) tickObject.SetActive(false);
        foundUserId = "";
            }
            public void StopRequestListener()
        {
            if (requestListener != null)
            {
                requestListener.Stop();
                requestListener = null;
                Debug.Log("Request listener stopped ✅");
            }
        }
        public void LoadFriends()
        {
                if (staticFriendCard != null) staticFriendCard.SetActive(false); 
            currentUserId = AuthManager.Instance.userId;

            if (string.IsNullOrEmpty(currentUserId))
            {
                Debug.Log("UserId not ready ❌");
                return;
            }

            // Stop previous listener
            if (friendsListener != null)
            {
                friendsListener.Stop();
                friendsListener = null;
            }

            // Clear existing cards
            foreach (Transform child in friendListParent)
                Destroy(child.gameObject);

            friendsListener = db.Collection("friends")
                .WhereEqualTo("userId", currentUserId)
                .Listen(snapshot =>
            {
                if (snapshot == null) return;
                if (friendListParent == null) return; // ✅ ADD THIS

                // Clear and rebuild
                foreach (Transform child in friendListParent)
                    Destroy(child.gameObject);

                int count = 0;

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    string friendId = doc.ContainsField("friendId")
                        ? doc.GetValue<string>("friendId") : "";

                    if (string.IsNullOrEmpty(friendId)) continue;

                    GameObject card = Instantiate(friendCardPrefab, friendListParent);
                    card.SetActive(true);

                    FriendCard cardScript = card.GetComponent<FriendCard>();
                    cardScript.Setup(friendId, avatarSprites, flagSprites);
             

                    count++;
                }

                if (noFriendsText != null)
                    noFriendsText.SetActive(count == 0);

                if (count == 0)
                    Debug.Log("❌ No friends found");
                else
                    Debug.Log("🎉 Friends found: " + count);
            });
        }

        // ─────────────────────────────────────────
        // Stop Friends Listener
        // ─────────────────────────────────────────
        public void StopFriendsListener()
        {
            if (friendsListener != null)
            {
                friendsListener.Stop();
                friendsListener = null;
                Debug.Log("Friends listener stopped ✅");
            }
        }
        public void LoadInviteFriends()
    {
        if (staticInviteCard != null) staticInviteCard.SetActive(false);

        currentUserId = AuthManager.Instance.userId;
        if (string.IsNullOrEmpty(currentUserId)) return;

        if (inviteListener != null) { inviteListener.Stop(); inviteListener = null; }

        foreach (Transform child in inviteListParent) Destroy(child.gameObject);

        inviteListener = db.Collection("friends")
            .WhereEqualTo("userId", currentUserId)
            .Listen(snapshot =>
        {
            if (snapshot == null || inviteListParent == null) return;

            foreach (Transform child in inviteListParent) Destroy(child.gameObject);

            int count = 0;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                string friendId = doc.ContainsField("friendId") ? doc.GetValue<string>("friendId") : "";
                if (string.IsNullOrEmpty(friendId)) continue;

                GameObject card = Instantiate(inviteCardPrefab, inviteListParent);
                card.SetActive(true);

                InviteFriendCard cardScript = card.GetComponent<InviteFriendCard>();
                cardScript.OnStatusChanged += _ => SortInviteCards();
                cardScript.Setup(friendId, avatarSprites, flagSprites);
                count++;
            }

            if (noInviteFriendsText != null) noInviteFriendsText.SetActive(count == 0);
        });
    }

    public void StopInviteListener()
    {
        if (inviteListener != null)
        {
            inviteListener.Stop();
            inviteListener = null;
        }
    }
    public void CheckFriendListEmpty()
{
    if (friendListParent == null) return;
    int remaining = friendListParent.childCount;
    if (noFriendsText != null)
        noFriendsText.SetActive(remaining == 0);
}
void SortInviteCards()
{
    if (inviteListParent == null) return;
    List<Transform> children = new List<Transform>();
    foreach (Transform child in inviteListParent)
        children.Add(child);

    children.Sort((a, b) =>
    {
        bool onlineA = a.GetComponent<InviteFriendCard>()?.IsOnline ?? false;
        bool onlineB = b.GetComponent<InviteFriendCard>()?.IsOnline ?? false;
        if (onlineA == onlineB) return 0;
        return onlineA ? -1 : 1;
    });

    for (int i = 0; i < children.Count; i++)
        children[i].SetSiblingIndex(i);
}
public void UpdateNotificationBadge(int count)
{
    if (notificationBadge == null) return;

    if (count <= 0)
    {
        notificationBadge.SetActive(false);
    }
    else
    {
        notificationBadge.SetActive(true);
        if (notificationCountText != null)
            notificationCountText.text = count.ToString();
    }
}
public void StartBadgeListener()
{
        StartCoroutine(StartBadgeListenerWhenReady());
}
IEnumerator StartBadgeListenerWhenReady()
{
    // Wait until userId and db are ready
    while (string.IsNullOrEmpty(AuthManager.Instance.userId) || db == null)
        yield return null;

    if (badgeListener != null) yield break;

    currentUserId = AuthManager.Instance.userId;

    badgeListener = db.Collection("friendRequests")
        .WhereEqualTo("toUserId", currentUserId)
        .Listen(snapshot =>
    {
        int count = 0;
        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (!doc.ContainsField("status")) continue;
            if (doc.GetValue<string>("status") != "pending") continue;
            count++;
        }
        UpdateNotificationBadge(count);
    });
}
public void FilterFriendList(string searchText)
{
    searchText = searchText.ToLower().Trim();

    int visibleCount = 0;

    foreach (Transform child in friendListParent)
    {
        FriendCard card = child.GetComponent<FriendCard>();

        if (card == null)
            continue;

        TMP_Text nameText = card.nameText;

        if (nameText == null)
            continue;

        string friendName = nameText.text.ToLower();

        bool shouldShow = string.IsNullOrEmpty(searchText) ||
                          friendName.StartsWith(searchText);

        child.gameObject.SetActive(shouldShow);

        if (shouldShow)
            visibleCount++;
    }

    // ✅ Show "No Friends" text if nothing matched
    if (noFriendsText != null)
    {
        noFriendsText.SetActive(visibleCount == 0);
    }
}
        }
