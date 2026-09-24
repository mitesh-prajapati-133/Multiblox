    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Firebase.Auth;
    using TMPro;
    using Google;
    using Firebase.Firestore;
    using Firebase.Extensions; 
    using UnityEngine.SceneManagement;
    public class AuthManager : MonoBehaviour
    {
    FirebaseAuth auth;
        FirebaseFirestore db;

        public GameObject signInPanel, profilePanel, mainMenuPanel;
        public TMP_InputField nameInput;
        public TMP_Text userIdText;
        public TMP_Text playerNameText;
        GoogleSignInConfiguration configuration;
    public TMP_Text loginTypeText;
    public UnityEngine.UI.Image topAvatarImage;   // The avatar shown next to name
    public List<UnityEngine.UI.Image> avatarOptions; // The 12 avatar buttons/images
    public string selectedAvatarId = "avatar1"; 
    public UnityEngine.UI.Image mainScreenAvatarImage;
    public UnityEngine.UI.Button chooseAvatarButton;
    public UnityEngine.UI.Button selectFrameButton;
    public GameObject avatarPanel;
    public GameObject framePanel;
    public List<UnityEngine.UI.Image> frameOptions; // list of selectable frames
    public UnityEngine.UI.Image topFrameImage;      // frame around avatar in profile
    public UnityEngine.UI.Image mainScreenFrameImage; // frame around avatar in main menu
    public string selectedFrameId = "frame1";
    public TMP_Text mainScreenNameText;
    public TMP_Text mainScreenIdText;
    public GameObject loadingCircle;
    public CountrySelector countrySelector;
public GameObject signInHelpPanel;
public GameObject signInTermsPanel;
    public string userId;
        public string loginType;
        public GameObject logOutPopup;
    public static AuthManager Instance;
    void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
        void Start()
        {
            if (PlayerPrefs.HasKey("playerName"))
        mainScreenNameText.text = PlayerPrefs.GetString("playerName");
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                InvokeRepeating("RefreshOnlineStatus", 0f, 5f);

                configuration = new GoogleSignInConfiguration
                {
                    WebClientId = "289754443224-ghskblafh2gt4urjfns4pjo345loti3r.apps.googleusercontent.com",
                    RequestIdToken = true,
                    RequestEmail = true 
                };

                if (PlayerPrefs.HasKey("userId") && PlayerPrefs.GetString("offlineEntry") != "true")
                {
                    userId = PlayerPrefs.GetString("userId");
            loginType = PlayerPrefs.GetString("loginType", "guest");
                    userIdText.text = "ID: " + userId;
                    mainScreenIdText.text = "ID: " + userId; 

                    if (loginType == "google")
                    {
                        LoadUserFromFirebase(userId);
                    }
                    else
                    {
                        if (PlayerPrefs.HasKey("playerName"))
                        {
                            playerNameText.text = PlayerPrefs.GetString("playerName");
                            mainScreenNameText.text = PlayerPrefs.GetString("playerName"); 
                        }
    db.Collection("users").Document(userId).SetAsync(
        new Dictionary<string, object> {
            { "isOnline", true },
            { "lastSeen", FieldValue.ServerTimestamp }
        
        },
        SetOptions.MergeAll
    );
                
                    }
                    if (PlayerPrefs.HasKey("avatar"))
                    {
                        selectedAvatarId = PlayerPrefs.GetString("avatar");
                        int index = int.Parse(selectedAvatarId.Replace("avatar", "")) - 1;
                        topAvatarImage.sprite = avatarOptions[index].sprite;
                        mainScreenAvatarImage.sprite = avatarOptions[index].sprite;
                        OnAvatarClick(index); // re‑apply tick + alpha
                    }
                    if (PlayerPrefs.HasKey("frame"))
    {
        selectedFrameId = PlayerPrefs.GetString("frame");
        int frameIndex = int.Parse(selectedFrameId.Replace("frame", "")) - 1;
        topFrameImage.sprite = frameOptions[frameIndex].sprite;
        mainScreenFrameImage.sprite = frameOptions[frameIndex].sprite;

        // Optional: re‑apply highlight if you want the tick/alpha effect
        for (int i = 0; i < frameOptions.Count; i++)
        {
            frameOptions[i].color = Color.white;
            frameOptions[i].transform.GetChild(0).gameObject.SetActive(false);
        }
        Color fc = frameOptions[frameIndex].color;
        fc.a = 120f / 255f;
        frameOptions[frameIndex].color = fc;
        frameOptions[frameIndex].transform.GetChild(0).gameObject.SetActive(true);
    }



                        // ✅ Show main menu only
        signInPanel.SetActive(false);
        profilePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        loadingCircle.SetActive(false);  

                }
                else{
                // First time user → check internet
        bool isOnline = Application.internetReachability != NetworkReachability.NotReachable;

        if (isOnline)
        {
            // Online → show Sign-In panel
            signInPanel.SetActive(true);
            profilePanel.SetActive(false);
            mainMenuPanel.SetActive(false);
        }
        else
        {
            // Offline → skip Sign-In, go to Main Menu
            signInPanel.SetActive(false);
        profilePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        mainScreenNameText.text = "User";
        mainScreenIdText.text = "";
        PlayerPrefs.SetString("offlineEntry", "true"); 
        SettingsAuthUI sui = FindObjectOfType<SettingsAuthUI>(true);
    if (sui != null) sui.RefreshUI();
        }

        loadingCircle.SetActive(false);


                }

            
            }
        
        });
        }
        void RefreshOnlineStatus()
    {
        if (!string.IsNullOrEmpty(userId))
        {
            db.Collection("users").Document(userId).SetAsync(
                new Dictionary<string, object> {
                    { "isOnline", true },
                    { "lastSeen", FieldValue.ServerTimestamp }
                },
                SetOptions.MergeAll
            );
        }
    }
    void Update()
    {
        // Handle Android/PC back button (Escape key)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
             if (signInHelpPanel.activeSelf)
    {
        signInHelpPanel.SetActive(false);
        return;
    }
    else if (signInTermsPanel.activeSelf)
    {
        signInTermsPanel.SetActive(false);
        return;
    }
            else if (signInPanel.activeSelf)
            {
                // Close Sign-In and go to Main Menu
                signInPanel.SetActive(false);
                profilePanel.SetActive(false);
                mainMenuPanel.SetActive(true);

                Debug.Log("System Back → Sign-In closed, Main Menu opened.");
            }
            else if (profilePanel.activeSelf)
            {
                // Close Profile and go back to Main Menu
                profilePanel.SetActive(false);
                mainMenuPanel.SetActive(true);

                Debug.Log("System Back → Profile closed, Main Menu opened.");
            }
            
          
        }
    }


    public void OnChooseAvatarClick()
    {
        chooseAvatarButton.GetComponentInChildren<TMP_Text>().color = Color.yellow;
        selectFrameButton.GetComponentInChildren<TMP_Text>().color = Color.white;

        avatarPanel.SetActive(true);
        framePanel.SetActive(false);
    }

    public void OnSelectFrameClick()
    {
        selectFrameButton.GetComponentInChildren<TMP_Text>().color = Color.yellow;
        chooseAvatarButton.GetComponentInChildren<TMP_Text>().color = Color.white;

        framePanel.SetActive(true);
        avatarPanel.SetActive(false);
    }
    public void OnFrameClick(int index)
    {
        selectedFrameId = "frame" + (index + 1);
        PlayerPrefs.SetString("frame", selectedFrameId);

        // Update top and main menu frame
        topFrameImage.sprite = frameOptions[index].sprite;
        mainScreenFrameImage.sprite = frameOptions[index].sprite;

        // Reset highlight
        for (int i = 0; i < frameOptions.Count; i++)
        {
            frameOptions[i].color = Color.white;
            frameOptions[i].transform.GetChild(0).gameObject.SetActive(false);
        }

        // Highlight selected frame
        Color c = frameOptions[index].color;
        c.a = 120f / 255f;
        frameOptions[index].color = c;
        frameOptions[index].transform.GetChild(0).gameObject.SetActive(true);
    }



        public void OnProfileButtonClick()
    {
        bool isOnline = Application.internetReachability != NetworkReachability.NotReachable;

        if (isOnline)
        {
            // If already properly signed in → open profile panel
            if (PlayerPrefs.HasKey("userId") && PlayerPrefs.GetString("offlineEntry") != "true")
            {
                profilePanel.SetActive(true);
                nameInput.text = mainScreenNameText.text;
                playerNameText.text = mainScreenNameText.text;
                userIdText.text = mainScreenIdText.text;
                int index = int.Parse(selectedAvatarId.Replace("avatar", "")) - 1;
                OnAvatarClick(index);
            }
            else
            {
                // Came in offline before → now online → show sign-in panel to complete login
                signInPanel.SetActive(true);
                profilePanel.SetActive(false);
                mainMenuPanel.SetActive(false);
            }
        }
        else
        {
            // Offline → do nothing
            Debug.Log("Offline: profile button disabled.");
        }

    }

        // 🔵 Google Login
        public void SignInWithGoogle()
        {
            loginType = "google"; 
        GoogleSignIn.Configuration = configuration;
        loadingCircle.SetActive(true);
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthFinished);
        }

        // 🟢 Guest Login
      public void PlayAsGuest()
{
    loginType = "guest";
    loadingCircle.SetActive(true);

    StartCoroutine(GuestLoginRoutine());
}

IEnumerator GuestLoginRoutine()
{
    // ✅ Proper Firebase logout
    if (auth != null && auth.CurrentUser != null)
    {
        auth.SignOut();
    }

    // ✅ Wait one frame so Firebase fully resets
    yield return null;

    // ✅ Reset auth reference
    auth = FirebaseAuth.DefaultInstance;

    // ✅ Start fresh anonymous login
    Login();
}
        public void OnEditClick()
    {
        nameInput.gameObject.SetActive(true);

        if (nameInput.image != null)
            nameInput.image.enabled = false;
    nameInput.characterLimit = 14;
        nameInput.text = playerNameText.text;
        nameInput.ActivateInputField();

        // Listen for end of editing
        nameInput.onEndEdit.AddListener(OnNameEntered);


    }
    private void OnNameEntered(string newName)
    {
    if (string.IsNullOrEmpty(newName))
        {
            Debug.Log("Enter name!");
            return;
        }
        if (newName.Length > 14)
        newName = newName.Substring(0, 14);

        playerNameText.text = newName;
        mainScreenNameText.text = newName;
        nameInput.gameObject.SetActive(false);
        nameInput.onEndEdit.RemoveListener(OnNameEntered);

        PlayerPrefs.SetString("playerName", newName);
        Debug.Log("Saved Locally: " + newName);

        // 🔥 Save/update Firestore for both Google and Guest
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "userId", userId },
            { "name", newName },
            { "loginType", loginType },
            { "avatar", selectedAvatarId },
        { "frame", selectedFrameId },
            { "countryIndex", PlayerPrefs.GetInt("selectedCountry", 0) }

        };
        db.Collection("users").Document(userId).SetAsync(data);
        Debug.Log("Saved to Firebase ✅");

    }



        void Login()
        {
            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            { if (!task.IsCompleted) return;
                if (Application.internetReachability == NetworkReachability.NotReachable)
    {
        signInPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        Debug.Log("Connection lost during Guest Sign-In → back to Main Menu (offline).");
        return;
    }

                
                

        var user = auth.CurrentUser;

        userId = GenerateShortId(user.UserId);

        PlayerPrefs.SetString("userId", userId);   
        PlayerPrefs.DeleteKey("offlineEntry");    // 🔥 ADD
        SettingsAuthUI sui = FindObjectOfType<SettingsAuthUI>(true);
    if (sui != null) sui.RefreshUI();
    PlayerPrefs.SetString("loginType", loginType);  // 🔥 ADD

        userIdText.text = "ID: " + userId;
        mainScreenIdText.text = "ID: " + userId;   
MainMenuController mmc = FindObjectOfType<MainMenuController>(true);
if (mmc != null)
{
    mmc.settingsPanel.SetActive(false);
    mmc.optionsPanel.SetActive(false);
}
    signInPanel.SetActive(false);
    profilePanel.SetActive(false);
    mainMenuPanel.SetActive(true);
    
    loadingCircle.SetActive(false); 

        Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "userId", userId },
                { "name", "Guest" },
                { "avatar", selectedAvatarId },
                {"frame", selectedFrameId},
                { "countryIndex", PlayerPrefs.GetInt("selectedCountry", 0) }
    // default until user edits
            };
            db.Collection("users").Document(userId).SetAsync(data);
            Debug.Log("Guest saved to Firebase ✅");
            PlayerPrefs.SetString("playerName", "Guest"); 
    db.Collection("users").Document(userId).SetAsync(
        new Dictionary<string, object> {
            { "isOnline", true },
            { "lastSeen", FieldValue.ServerTimestamp }
        
        },
        SetOptions.MergeAll
    );
    loginTypeText.text = "Guest"; 

            });
            
        }

        // 🔥 Convert Firebase UID → number
        string GenerateShortId(string firebaseId)
        {
            // Get a stable hash from Firebase UID
        int hash = firebaseId.GetHashCode();

        // Ensure positive numbera
        long positiveHash = Mathf.Abs(hash);

        // Pad with zeros if needed, then take 9 digits
        string numericPart = positiveHash.ToString().PadLeft(9, '0').Substring(0, 9);

        // Prepend '7' to make it 10 digits starting with 7
        string playerId = "7" + numericPart;

        return playerId;

        }

        // ✅ DONE BUTTON
        public void OnDone()
        {
            if (string.IsNullOrEmpty(nameInput.text))
        {
            Debug.Log("Enter name!");
            return;
        }

        string name = nameInput.text;
        playerNameText.text = name;
        mainScreenNameText.text = name; 

        // Hide input field
        nameInput.gameObject.SetActive(false);

        // Clear input field so it doesn’t show old text next time
        nameInput.text = "";

        PlayerPrefs.SetString("playerName", name);
        Debug.Log("Saved Locally: " + name);
        // 🔥 Save/update Firestore for both Google and Guest
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            { "userId", userId },
            { "name", name },
            { "loginType", loginType },
            { "avatar", selectedAvatarId }  ,
                { "frame", selectedFrameId } ,
                    { "countryIndex", PlayerPrefs.GetInt("selectedCountry", 0) }
            
        };
        db.Collection("users").Document(userId).SetAsync(data);
        Debug.Log("Saved to Firebase ✅");
        profilePanel.SetActive(false);

        }



    public void OnAvatarClick(int index)
    {
        selectedAvatarId = "avatar" + (index + 1);
    PlayerPrefs.SetString("avatar", selectedAvatarId);

        // Update top avatar image
        topAvatarImage.sprite = avatarOptions[index].sprite;
        mainScreenAvatarImage.sprite = avatarOptions[index].sprite; 

        // Reset all avatars
        for (int i = 0; i < avatarOptions.Count; i++)
        {
            // Reset color
            avatarOptions[i].color = Color.white;

            // Hide tick mark child (assuming tick is child[0])
            avatarOptions[i].transform.GetChild(0).gameObject.SetActive(false);
        }


        // Highlight selected avatar
        Color c = avatarOptions[index].color;
        c.a = 120f / 255f; // opacity A=120
        avatarOptions[index].color = c;

        // Show tick mark overlay
        avatarOptions[index].transform.GetChild(0).gameObject.SetActive(true);

    }


        void OnGoogleAuthFinished(System.Threading.Tasks.Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted || task.IsCanceled) return;

        GoogleSignInUser googleUser = task.Result;
        Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);
    if (Application.internetReachability == NetworkReachability.NotReachable)
    {
        signInPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        Debug.Log("Connection lost during Google Sign-In → back to Main Menu (offline).");
        return;
    }

        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
        {
            if (!authTask.IsCompleted) return;

            var user = auth.CurrentUser;

            // ✅ FIX 1 — Short ID
            userId = GenerateShortId(user.UserId);

            // ✅ FIX 2 — Save to PlayerPrefs
            PlayerPrefs.SetString("userId", userId);
            PlayerPrefs.DeleteKey("offlineEntry");
            SettingsAuthUI sui = FindObjectOfType<SettingsAuthUI>(true);
    if (sui != null) sui.RefreshUI();
            PlayerPrefs.SetString("loginType", "google");

            userIdText.text = "ID: " + userId;
            mainScreenIdText.text = "ID: " + userId;
        MainMenuController mmc = FindObjectOfType<MainMenuController>(true);
if (mmc != null)
{
    mmc.settingsPanel.SetActive(false);
    mmc.optionsPanel.SetActive(false);
}
        signInPanel.SetActive(false);
    profilePanel.SetActive(false);
    mainMenuPanel.SetActive(true);
    loadingCircle.SetActive(false);


    db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(snapshotTask =>
    {
        if (snapshotTask.Result.Exists)
        {
            // 🔥 Use the previously saved name
            string savedName = snapshotTask.Result.GetValue<string>("name");
            playerNameText.text = savedName;
            mainScreenNameText.text = savedName;
            nameInput.text = savedName;
            PlayerPrefs.SetString("playerName", savedName);

            loginTypeText.text = snapshotTask.Result.ContainsField("email")
                ? snapshotTask.Result.GetValue<string>("email")
                : "Google";

            if (snapshotTask.Result.ContainsField("avatar"))
            {
                string savedAvatar = snapshotTask.Result.GetValue<string>("avatar");
                selectedAvatarId = savedAvatar;
                int index = int.Parse(savedAvatar.Replace("avatar", "")) - 1;
                topAvatarImage.sprite = avatarOptions[index].sprite;
                OnAvatarClick(index);
            }

            if (snapshotTask.Result.ContainsField("frame"))
            {
                string savedFrame = snapshotTask.Result.GetValue<string>("frame");
                selectedFrameId = savedFrame;
                int index = int.Parse(savedFrame.Replace("frame", "")) - 1;
                topFrameImage.sprite = frameOptions[index].sprite;
                mainScreenFrameImage.sprite = frameOptions[index].sprite;
                PlayerPrefs.SetString("frame", savedFrame);
            }

            if (snapshotTask.Result.ContainsField("countryIndex"))
            {
                int countryIndex = snapshotTask.Result.GetValue<int>("countryIndex");
                countrySelector.LoadCountryFromFirebase(countryIndex);
            }

            Debug.Log("Loaded existing Google user from Firebase ✅");
        db.Collection("users").Document(userId).SetAsync(
        new Dictionary<string, object> {
            { "isOnline", true },
            { "lastSeen", FieldValue.ServerTimestamp }

        },
        SetOptions.MergeAll
    );
        }
        else  // ✅ correctly attached to Exists check now
        {
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "userId", userId },
                { "name", user.DisplayName ?? "GoogleUser" },
                { "loginType", "google" },
                { "email", user.Email ?? "" },
                { "avatar", selectedAvatarId },
                { "frame", selectedFrameId },
                { "countryIndex", PlayerPrefs.GetInt("selectedCountry", 0) }
            };
            db.Collection("users").Document(userId).SetAsync(data);

            playerNameText.text = user.DisplayName ?? "GoogleUser";
            loginTypeText.text = user.Email ?? "Google";

            Debug.Log("Created new Google user in Firebase ✅");
    db.Collection("users").Document(userId).SetAsync(
        new Dictionary<string, object> {
            { "isOnline", true },
                { "lastSeen", FieldValue.ServerTimestamp }

        },
        SetOptions.MergeAll
    );
        }

        signInPanel.SetActive(false);
        profilePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        loadingCircle.SetActive(false);
    });
        });
    }
    void LoadUserFromFirebase(string uid)
    {
        db.Collection("users").Document(uid).GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                string savedName = task.Result.GetValue<string>("name");
                playerNameText.text = savedName;
                nameInput.text = savedName;
                mainScreenNameText.text = savedName;
                PlayerPrefs.SetString("playerName", savedName);
            if (task.Result.ContainsField("loginType"))
    {
        loginType = task.Result.GetValue<string>("loginType");
        PlayerPrefs.SetString("loginType", loginType); // ✅ save locally
    }

    if (task.Result.ContainsField("email"))
        loginTypeText.text = task.Result.GetValue<string>("email");
    else
        loginTypeText.text = loginType == "google" ? "Google" : "Guest";
                    if (task.Result.ContainsField("avatar"))
    {
        string savedAvatar = task.Result.GetValue<string>("avatar");
        selectedAvatarId = savedAvatar;

        int index = int.Parse(savedAvatar.Replace("avatar", "")) - 1;
        topAvatarImage.sprite = avatarOptions[index].sprite;
    mainScreenAvatarImage.sprite = avatarOptions[index].sprite;
    for (int i = 0; i < avatarOptions.Count; i++)
    {
        avatarOptions[i].color = Color.white;
        avatarOptions[i].transform.GetChild(0).gameObject.SetActive(false);
    }
    Color c = avatarOptions[index].color;
    c.a = 120f / 255f;
    avatarOptions[index].color = c;
    avatarOptions[index].transform.GetChild(0).gameObject.SetActive(true);


        
        // ✅ re‑apply tick + alpha
        PlayerPrefs.SetString("avatar", savedAvatar);

    
    }
    if (task.Result.ContainsField("frame"))
    {
        string savedFrame = task.Result.GetValue<string>("frame");
        selectedFrameId = savedFrame;

        int index = int.Parse(savedFrame.Replace("frame", "")) - 1;
        topFrameImage.sprite = frameOptions[index].sprite;
        mainScreenFrameImage.sprite = frameOptions[index].sprite;

        // Save locally
        PlayerPrefs.SetString("frame", savedFrame);
    }
    if (task.Result.ContainsField("countryIndex"))
    {
        int countryIndex = task.Result.GetValue<int>("countryIndex");
        countrySelector.LoadCountryFromFirebase(countryIndex);
    }

        userIdText.text = "ID: " + uid;
                mainScreenIdText.text = "ID: " + uid;
            db.Collection("users").Document(userId).SetAsync(
        new Dictionary<string, object> {
            { "isOnline", true },
            { "lastSeen", FieldValue.ServerTimestamp }
            
        },
        SetOptions.MergeAll
    );
            }
    signInPanel.SetActive(false);
    profilePanel.SetActive(false);
    mainMenuPanel.SetActive(true);
    loadingCircle.SetActive(false); 

        });
        
    }
    void OnApplicationQuit()
    {
        if (!string.IsNullOrEmpty(userId))
        {
            db.Collection("users").Document(userId).SetAsync(
                new Dictionary<string, object> {
                    { "isOnline", false },
                    { "lastSeen", FieldValue.ServerTimestamp }
                
                },
                SetOptions.MergeAll
            );
        }
    }

    void OnApplicationPause(bool paused)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            if (paused)
            {
                // 🔴 Going background → OFFLINE + save time
                db.Collection("users").Document(userId).SetAsync(
                    new Dictionary<string, object> {
                        { "isOnline", false },
                        { "lastSeen", FieldValue.ServerTimestamp } // 🔥 ADD THIS
                
                    },
                    SetOptions.MergeAll
                );
            }
            else
            {
                // 🟢 Coming back → ONLINE + update time
                db.Collection("users").Document(userId).SetAsync(
                    new Dictionary<string, object> {
                        { "isOnline", true },
                        { "lastSeen", FieldValue.ServerTimestamp }
                        
                    // 🔥 IMPORTANT
                    },
                    SetOptions.MergeAll
                );
            }
        }
    }
    void OnApplicationFocus(bool hasFocus)
    {
        if (string.IsNullOrEmpty(userId) || db == null) return;
        if (!hasFocus)
        {
            db.Collection("users").Document(userId).SetAsync(
                new Dictionary<string, object> {
                    { "isOnline", false },
                    { "currentScene", "" },
                    { "lastSeen", FieldValue.ServerTimestamp }
                }, SetOptions.MergeAll
            );
        }
        else
        {
            db.Collection("users").Document(userId).SetAsync(
                new Dictionary<string, object> {
                    { "isOnline", true },
                    { "currentScene", SceneManager.GetActiveScene().name },
                    { "lastSeen", FieldValue.ServerTimestamp }
                }, SetOptions.MergeAll
            );
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.IsNullOrEmpty(userId) && db != null)
        {
            db.Collection("users").Document(userId).SetAsync(
                new Dictionary<string, object> {
                    { "isOnline", true },
                    { "currentScene", scene.name },
                    { "lastSeen", FieldValue.ServerTimestamp }
                }, SetOptions.MergeAll
            );
        }
    }
    public void OnLogOutConfirmed()
    {
        // ✅ Sign out from Firebase Auth
        if (auth != null)
{
    auth.SignOut();
    auth = FirebaseAuth.DefaultInstance;
}

        // ✅ Sign out from Google SDK (clears account picker cache)
        if (GoogleSignIn.DefaultInstance != null)
{
    GoogleSignIn.DefaultInstance.SignOut();
}
StopAllCoroutines();



        // Clear ALL saved data
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("playerName");
        PlayerPrefs.DeleteKey("loginType");
        PlayerPrefs.DeleteKey("avatar");
        PlayerPrefs.DeleteKey("frame");
        PlayerPrefs.DeleteKey("selectedCountry");
        PlayerPrefs.DeleteKey("offlineEntry");
        PlayerPrefs.Save();

        // Reset runtime variables
        userId = "";
        loginType = "";
        selectedAvatarId = "avatar1";
        selectedFrameId = "frame1";

        // Close logout popup
        if (logOutPopup != null)
            logOutPopup.SetActive(false);

        // Close Settings and Options panels
        MainMenuController mmc = FindObjectOfType<MainMenuController>(true);
        if (mmc != null)
        {
            mmc.settingsPanel.SetActive(false);
            mmc.optionsPanel.SetActive(false);
        }

        // Refresh Settings UI
        SettingsAuthUI sui = FindObjectOfType<SettingsAuthUI>(true);
        if (sui != null) sui.RefreshUI();
StartCoroutine(ReloadSceneAfterLogout());
        // Go to Sign In panel
  
    }
    public void OnLogOutButtonClicked()
    {
        if (logOutPopup != null)
            logOutPopup.SetActive(true);
    }

    public void OnLogOutNoClicked()
    {
        if (logOutPopup != null)
            logOutPopup.SetActive(false);
    }
    IEnumerator ReloadSceneAfterLogout()
{
    yield return new WaitForSeconds(0.5f);

    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
public void PlayOffline()
{
    signInPanel.SetActive(false);
    profilePanel.SetActive(false);
    mainMenuPanel.SetActive(true);

    mainScreenNameText.text = "User";
    mainScreenIdText.text = "";

    PlayerPrefs.SetString("offlineEntry", "true");

    SettingsAuthUI sui = FindObjectOfType<SettingsAuthUI>(true);
    if (sui != null)
        sui.RefreshUI();

    Debug.Log("Offline Mode Opened");
}
public void OpenSignInHelp()
{
    signInHelpPanel.SetActive(true);
}

public void CloseSignInHelp()
{
    signInHelpPanel.SetActive(false);
}

public void OpenSignInTerms()
{
    signInTermsPanel.SetActive(true);
}

public void CloseSignInTerms()
{
    signInTermsPanel.SetActive(false);
}
    }
