using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
public class PhotonLobbyManager : MonoBehaviourPunCallbacks
{
     public static PhotonLobbyManager Instance;

    [Header("Room Code")]
    public TMP_Text roomCodeText;

    [Header("Panels")]
    public GameObject mainStartPanel;
    public GameObject timeDonePanel;

    [Header("Player Slots")]
    public GameObject[] playerSlots;       // size 4
    public TMP_Text[] playerNameTexts;     // size 4
    public TMP_Text[] readyTexts;          // size 4

    [Header("Buttons")]
    public Button startButton;
    public TMP_Text startButtonText;
    public Button leftButton;
    public Button rightButton;
public Button exitButton;
    [Header("Countdown")]
    public TMP_Text countdownText;

    [Header("Time Settings")]
    public TMP_Text timeText;
    [Header("Country Flags")]
public Sprite[] countryFlags; // assign same 30 flags in same order as CountrySelector
public Image[] playerFlagImages; 
public Image[] playerAvatarImages; // size 4, one per player slot
public Sprite[] avatarSprites;
    private int[] timeOptions = { 3, 4, 5, 6, 7 };
    private int currentTimeIndex = 2; // default 5
    Coroutine countdownRoutine;
    // Photon keys
    const string READY_KEY = "Ready";
    const string TIME_KEY = "LobbyTime";

    bool isReady = false;
    bool isCountingDown = false;
const string COUNTDOWN_KEY = "Countdown";
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
         PhotonNetwork.AutomaticallySyncScene = true;
    if (roomCodeText != null)
        roomCodeText.text = PhotonNetwork.CurrentRoom.Name;

    if (timeDonePanel != null)
        timeDonePanel.SetActive(false);
    if (mainStartPanel != null)
        mainStartPanel.SetActive(true);

    foreach (var slot in playerSlots)
        slot.SetActive(false);

    UpdateCountdownText(-1);

    // ✅ Delay properties until fully ready
    StartCoroutine(SetPropertiesWhenReady());
    }
    IEnumerator SetPropertiesWhenReady()
{
    // Wait until fully joined
    float t = 0;
    while (PhotonNetwork.NetworkClientState != ClientState.Joined && t < 10f)
    {
        t += Time.deltaTime;
        yield return null;
    }

    Hashtable myProps = new Hashtable();
    myProps["countryIndex"] = PlayerPrefs.GetInt("selectedCountry", 0);
    myProps["avatarIndex"] = int.Parse(PlayerPrefs.GetString("avatar", "avatar1").Replace("avatar", "")) - 1;
    myProps["playerName"] = PlayerPrefs.GetString("playerName", "Player");
    PhotonNetwork.LocalPlayer.SetCustomProperties(myProps);

    // ✅ Update UI after properties set
    yield return new WaitForSeconds(0.5f);
    UpdateLobbyUI();
}

    // ─────────────────────────────────────
    // LOBBY UI
    // ─────────────────────────────────────
    public void UpdateLobbyUI()
    {
        var players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < players.Length)
            {
                playerSlots[i].SetActive(true);

                if (playerNameTexts[i] != null)
                {
                string pName = players[i].CustomProperties.ContainsKey("playerName")
    ? (string)players[i].CustomProperties["playerName"]
    : (players[i].NickName != "" ? players[i].NickName : "Player " + (i + 1));
playerNameTexts[i].text = pName;
                }
                        // Show country flag
if (playerFlagImages != null && playerFlagImages.Length > i && playerFlagImages[i] != null)
{
    int cIndex = 0;
    if (players[i].CustomProperties.ContainsKey("countryIndex"))
        cIndex = (int)players[i].CustomProperties["countryIndex"];

    if (countryFlags != null && cIndex < countryFlags.Length)
        playerFlagImages[i].sprite = countryFlags[cIndex];
        
}
  if (playerAvatarImages != null && playerAvatarImages.Length > i && playerAvatarImages[i] != null)
{
    int aIndex = 0;
    if (players[i].CustomProperties.ContainsKey("avatarIndex"))
        aIndex = (int)players[i].CustomProperties["avatarIndex"];

    if (avatarSprites != null && aIndex < avatarSprites.Length)
        playerAvatarImages[i].sprite = avatarSprites[aIndex];
}

                bool ready = false;
                if (players[i].CustomProperties.ContainsKey(READY_KEY))
                    ready = (bool)players[i].CustomProperties[READY_KEY];

                if (readyTexts[i] != null)
                {
                    readyTexts[i].text = ready ? "READY" : "NOT READY";
                    readyTexts[i].color = ready ? Color.green : Color.red;
                }
            }
            else
            {
                playerSlots[i].SetActive(false);
            }
        }

        // Update start button text
        if (startButtonText != null)
            startButtonText.text = isReady ? "NOT READY" : "START";

        CheckAllReady();
    }

    // ─────────────────────────────────────
    // READY BUTTON
    // ─────────────────────────────────────
    public void ClickReady()
    {
     if (PhotonNetwork.CurrentRoom == null) return; // safety check
    
    isReady = !isReady;

    if (startButtonText != null)
        startButtonText.text = isReady ? "NOT READY" : "START";

    Hashtable props = new Hashtable();
    props[READY_KEY] = isReady;
    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    // ─────────────────────────────────────
    // CHECK ALL READY
    // ─────────────────────────────────────
    void CheckAllReady()
    {
        if (PhotonNetwork.CurrentRoom == null) return;
    if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

    foreach (var player in PhotonNetwork.PlayerList)
    {
        if (!player.CustomProperties.ContainsKey(READY_KEY) ||
            !(bool)player.CustomProperties[READY_KEY])
        {
            return; // someone not ready
        }
    }

    // Everyone ready → lock button and start countdown
    if (!isCountingDown && PhotonNetwork.IsMasterClient)
    {
        if (startButton != null)
            startButton.interactable = false;
                if (exitButton != null)
            exitButton.interactable = false;

        countdownRoutine = StartCoroutine(StartCountdown());
    }

    }

    IEnumerator StartCountdown()
    {
      isCountingDown = true;

    for (int i = 10; i >= 0; i--)
    {
        Hashtable props = new Hashtable();
        props[COUNTDOWN_KEY] = i;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        yield return new WaitForSeconds(1f);
    }

    isCountingDown = false;   // ADD THIS

   if (PhotonNetwork.IsMasterClient)
{
    GameMode.IsOnline = true;
    PhotonNetwork.LoadLevel("room_play");
}
    }

   

    void UpdateCountdownText(int value)
    {
        if (countdownText == null) return;

        if (value < 0)
        {
            countdownText.text = "WAIT FOR PLAYERS TO JOIN!!";
            countdownText.color = Color.white;
            if (startButton != null)
                startButton.interactable = true;
        }
        else
        {
            countdownText.text = "Game starts in: " + value +"seconds";
            countdownText.color = value <= 4 ? Color.red : Color.white;
        }
    }

    // ─────────────────────────────────────
    // TIME SETTINGS — same as your old lobby
    // ─────────────────────────────────────
    public void OpenTimeSettings()
    {
        mainStartPanel.SetActive(false);
        timeDonePanel.SetActive(true);

        // Get current time from room properties
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(TIME_KEY))
        {
            int currentTime = (int)PhotonNetwork.CurrentRoom
                .CustomProperties[TIME_KEY];

            for (int i = 0; i < timeOptions.Length; i++)
            {
                if (timeOptions[i] == currentTime)
                {
                    currentTimeIndex = i;
                    break;
                }
            }
        }

        UpdateTimeText();

        // Only host can change time
        bool isHost = PhotonNetwork.IsMasterClient;
        if (leftButton != null) leftButton.interactable = isHost;
        if (rightButton != null) rightButton.interactable = isHost;
    }

    public void IncreaseTime()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        currentTimeIndex = (currentTimeIndex + 1) % timeOptions.Length;
        UpdateTimeText();
    }

    public void DecreaseTime()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        currentTimeIndex--;
        if (currentTimeIndex < 0)
            currentTimeIndex = timeOptions.Length - 1;
        UpdateTimeText();
    }

    void UpdateTimeText()
    {
        if (timeText != null)
            timeText.text = timeOptions[currentTimeIndex] + ":00";
    }

    public void ConfirmTime()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Sync time to all players via room properties
            Hashtable props = new Hashtable();
            props[TIME_KEY] = timeOptions[currentTimeIndex];
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        timeDonePanel.SetActive(false);
        mainStartPanel.SetActive(true);
    }

    // ─────────────────────────────────────
    // EXIT
    // ─────────────────────────────────────
    public void ExitLobby()
    {
        PhotonNetwork.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager
            .LoadScene("Main menu");
    }

    // ─────────────────────────────────────
    // PHOTON CALLBACKS
    // ─────────────────────────────────────
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateLobbyUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
         // If host left → joiner auto-exits (disband room)
    if (otherPlayer.IsMasterClient)
    {
        ExitLobby();
        return;
    }

    isCountingDown = false;
    if (countdownRoutine != null)
        StopCoroutine(countdownRoutine);

    if (exitButton != null)
        exitButton.interactable = true;
    if (startButton != null)
        startButton.interactable = true;

    UpdateLobbyUI();
    UpdateCountdownText(-1);
    }

    public override void OnPlayerPropertiesUpdate(
        Player targetPlayer, Hashtable changedProps)
    {
        UpdateLobbyUI();
         CheckAllReady(); 
    }

  
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
{
    if (propertiesThatChanged.ContainsKey(TIME_KEY))
    {
        int time = (int)propertiesThatChanged[TIME_KEY];
        if (timeText != null)
            timeText.text = time + ":00";
    }

    if (propertiesThatChanged.ContainsKey(COUNTDOWN_KEY))
    {
         int value = (int)propertiesThatChanged[COUNTDOWN_KEY];

    UpdateCountdownText(value);

    if (startButton != null)
        startButton.interactable = false; 
        
        if (exitButton != null)
            exitButton.interactable = false;
    }
    
    if (propertiesThatChanged.ContainsKey("ChatLog"))
{
    if (LobbyChatManager.Instance != null)
        LobbyChatManager.Instance.RefreshChatDisplay(
            (string)propertiesThatChanged["ChatLog"]);
}
}

}
