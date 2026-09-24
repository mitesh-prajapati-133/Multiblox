using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class RoomManager : MonoBehaviourPunCallbacks
{
   public static RoomManager Instance;
    bool wantsToHost = false;
bool joiningViaInvite = false;
    void Awake()
    {
        if (Instance != null && Instance != this)
    {
        Destroy(gameObject); // ✅ Destroys the NEW duplicate
        return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
    PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.KeepAliveInBackground = 60000f; 
               PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 100000;
    }

    // ── HOST ──
    public void HostGame()
    {  
        joiningViaInvite = false;
          if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby)
        CreateRoom(); // already in lobby → create directly
    else if (PhotonNetwork.IsConnected)
        CreateRoom();
    else
    {
        wantsToHost = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    }

    void CreateRoom()
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string roomCode = "";
        for (int i = 0; i < 6; i++)
            roomCode += chars[Random.Range(0, chars.Length)];

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        options.IsVisible = true;
        options.IsOpen = true;
        options.PlayerTtl = 0;    // ✅ keep player slot 20 seconds
options.EmptyRoomTtl = 0;

        PhotonNetwork.JoinOrCreateRoom(roomCode, options, TypedLobby.Default);
    }

    // ── JOIN ──
    public void JoinGame(string roomCode)
    {
     if (!PhotonNetwork.IsConnected) return;
    joiningViaInvite = true;
    
    // ✅ If in lobby already, join directly
    if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby ||
        PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
    {
        PhotonNetwork.JoinRoom(roomCode.Trim());
    }
    else
    {
        // Store roomCode and join after lobby
        StartCoroutine(JoinAfterLobby(roomCode));
    }
    }

    // ── CALLBACKS ──
    public override void OnConnectedToMaster()
    {
         if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer
        && !string.IsNullOrEmpty(PhotonNetwork.CurrentRoom?.Name ?? ""))
        return;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Connected to Photon ✅");

        // Auto create room if host clicked before connected
        if (wantsToHost)
        {
            wantsToHost = false;
            CreateRoom();
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {

    PhotonNetwork.NickName = PlayerPrefs.GetString("playerName", "Player");

    if (joiningViaInvite)
    {
        Debug.Log("✅ Joined via invite, host will sync scene");
        joiningViaInvite = false;
    }
    else
    {
        PhotonNetwork.LoadLevel("host_join");
    }
        
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join failed: " + message);
        if (JoinSceneManager.Instance != null)
            JoinSceneManager.Instance.ShowError("Invalid Code! Try Again.");
    }
IEnumerator JoinAfterLobby(string roomCode)
{
    float t = 0;
    while (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby && t < 10f)
    {
        t += Time.deltaTime;
        yield return null;
    }
    PhotonNetwork.JoinRoom(roomCode.Trim());
}
}
