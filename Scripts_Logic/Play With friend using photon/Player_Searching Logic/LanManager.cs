using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class LanManager :  MonoBehaviourPunCallbacks
{
       public static LanManager Instance;

    // Is this player the host (Player1) or joiner (Player2)?
    public static bool IsHost => PhotonNetwork.IsMasterClient;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Host generates seed and sends to all
        if (IsHost)
        {
            int seed = Random.Range(0, 999999);
            photonView.RPC("RpcSetSeed", RpcTarget.All, seed);
        }
    }

    // ── SEED SYNC ──
    [PunRPC]
    public void RpcSetSeed(int seed)
    {
        Random.InitState(seed);
        Debug.Log("Seed set: " + seed);
    }

    // ── CARD PLACED SYNC ──
    public void SendCardPlaced(int cardIndex,int owner,int value)
    {
          photonView.RPC("RpcCardPlaced", RpcTarget.Others, cardIndex, owner, value);
    }

    [PunRPC]
    void RpcCardPlaced(int cardIndex,int owner, int value)
    {
 cardbehavior[] cards = FindObjectsOfType<cardbehavior>();

    foreach (var card in cards)
    {
        if (card.cardIndex == cardIndex)
        {
            LanCardBehavior cb = card.GetComponent<LanCardBehavior>();

            // sync owner
            cb.cardOwner = (turnmanager.Turn)owner;

            // sync card value so correct sprite is used
            carddealer dealer = card.GetComponent<carddealer>();
            dealer.cardValue = value;

            // snap the same card
            cb.ForceSnap();
            break;
        }
    }

    }

    // ── TURN SYNC ──
    public void SendTurnSwitch(int playerTurn)
    {
        photonView.RPC("RpcSwitchTurn", RpcTarget.Others, playerTurn);
    }

    [PunRPC]
    void RpcSwitchTurn(int playerTurn)
    {
        turnmanager.Turn turn = (turnmanager.Turn)playerTurn;
        turnmanager.Instance.SetTurn(turn);
    }

    // ── DRAG POSITION SYNC ──

 public void SendStuckFlag(int player)
{
    photonView.RPC("RpcSetStuckFlag", RpcTarget.Others, player);
}

[PunRPC]
void RpcSetStuckFlag(int player)
{
    if (player == 0)
        roundmanager.Instance.player1HasStuck = true;
    else
        roundmanager.Instance.player2HasStuck = true;
}
public void SendRedealSeed(int seed)
{
    photonView.RPC("RpcRedealSeed", RpcTarget.All, seed);
}

[PunRPC]
void RpcRedealSeed(int seed)
{
    Random.InitState(seed);
}
}
