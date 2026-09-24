using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GameNetworkManager : MonoBehaviourPunCallbacks
{
  public static GameNetworkManager Instance;

    const byte SHUFFLE_EVENT = 1;
    const byte CARD_EVENT = 2;
    const byte TURN_EVENT = 3;
    const byte READY_EVENT = 4;
public static int remoteDraggingCardIndex = -1;
    public enum ShuffleTarget { Distributor, Redeal }
    public static ShuffleTarget currentShuffleTarget = ShuffleTarget.Distributor;
const byte DRAG_EVENT = 20;
    public static bool localAnimationDone = false;
    public static bool remoteAnimationDone = false;
    public static bool bothSidesReady => localAnimationDone && remoteAnimationDone;

    private int pendingOfflineCard = -1;
    private turndown.Turn pendingOfflineTurn;
    private bool hasPendingCard = false;
    private string lastRoomName = "";

   
   

    void Awake() { Instance = this; }

    void Start()
    {
        // ✅ Keep connection alive 60s in background
        PhotonNetwork.KeepAliveInBackground = 60000f;
           PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 100000;
    }

    void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    // =====================================================
    // ✅ OPPONENT WENT OFFLINE
    // fires on the ONLINE player's screen
    // =====================================================
    

    // =====================================================
    // ✅ OPPONENT CAME BACK ONLINE
    // fires on the ONLINE player's screen
    // =====================================================
  
public void SendDraggingCard(int index)
{
    PhotonNetwork.RaiseEvent(
        DRAG_EVENT,
        index,
        new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
        SendOptions.SendReliable
    );
}
    // =====================================================
    // ✅ 20 SECOND TIMEOUT — just disable, no scene change
    // =====================================================
   

    // =====================================================
    // ✅ THIS CLIENT RECONNECTED
    // fires on the OFFLINE player's screen after rejoin
    // =====================================================
    

  
    // =====================================================
    // ✅ THIS CLIENT DISCONNECTED
    // fires on the OFFLINE player's own screen
    // =====================================================
    public override void OnDisconnected(DisconnectCause cause)
    {
        // ✅ Disable dragging on own screen immediately
        if (turndown.Instance != null)
            turndown.Instance.enabled = false;
 Debug.Log("Disconnected - handled by DisconnectHandler");
        // ✅ Try to reconnect if was in a room
    
    }

   
    // =====================================================
    // EXISTING — ALL UNCHANGED
    // =====================================================
    public void SendReady()
    {
        localAnimationDone = true;

        PhotonNetwork.RaiseEvent(
            READY_EVENT,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable
        );
    }

    public static void ResetReady()
    {
        localAnimationDone = false;
        remoteAnimationDone = false;
    }

    public void SendShuffle(List<int> order)
    {
        PhotonNetwork.RaiseEvent(
            SHUFFLE_EVENT,
            order.ToArray(),
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendReliable
        );
    }

    public void SendCard(int index)
    {
        pendingOfflineCard = index;
    hasPendingCard = true;

    if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        return; 

  
    bool sent = PhotonNetwork.RaiseEvent(
        CARD_EVENT,
        index,
        new RaiseEventOptions { Receivers = ReceiverGroup.Others },
        SendOptions.SendReliable
    );


    if (sent)
        hasPendingCard = false;
    }

    public void SendTurn(turndown.Turn nextTurn)
    {
     pendingOfflineTurn = nextTurn;

    if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
        return; 

    PhotonNetwork.RaiseEvent(
        TURN_EVENT,
        (int)nextTurn,
        new RaiseEventOptions { Receivers = ReceiverGroup.Others },
        SendOptions.SendReliable
    );
    }

    void OnEvent(EventData ev)
    {
        if (ev.Code == SHUFFLE_EVENT)
        {
            int[] order = (int[])ev.CustomData;

            if (currentShuffleTarget == ShuffleTarget.Distributor)
            {
                if (distributor.Instance != null)
                    distributor.Instance.ApplyNetworkShuffle(order);
            }
            else
            {
                if (redeal.Instance != null)
                    redeal.Instance.ApplyNetworkShuffle(order);
            }
        }

        if (ev.Code == CARD_EVENT)
        {
            int index = (int)ev.CustomData;
            behaviour[] cards = FindObjectsOfType<behaviour>();
            foreach (var c in cards)
            {
                if (c.cardIndex == index)
                {
                    c.ForceNetworkPlace();
                    break;
                }
            }
        }

        if (ev.Code == TURN_EVENT)
        {
            int turnIndex = (int)ev.CustomData;
            turndown.Instance.ApplyTurnNetwork((turndown.Turn)turnIndex);
        }

        if (ev.Code == READY_EVENT)
        {
            remoteAnimationDone = true;
        }
        if (ev.Code == DRAG_EVENT)
{
    remoteDraggingCardIndex = (int)ev.CustomData;
}
    }
    
}
