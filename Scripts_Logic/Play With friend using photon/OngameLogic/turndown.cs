using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class turndown : MonoBehaviourPun

{
   public static turndown Instance;

    public enum Turn
    {
        Player1,
        Player2
    }

    public Turn currentTurn = Turn.Player1;

    public Transform player1Stack;
    public Transform player2Stack;

    public GameObject player1TurnText;
    public GameObject player2TurnText;

    void Awake()
    {
        Instance = this;
        UpdateTurnUI();
    }

    // ─── WHO AM I? ───────────────────────────────────────────────
    public static Turn MyTurn()
    {
        return PhotonNetwork.IsMasterClient ? Turn.Player1 : Turn.Player2;
    }

    // ─── CAN THIS CARD BE DRAGGED? ───────────────────────────────
    public bool CanDrag(Transform card)
    {
        behaviour cb = card.GetComponent<behaviour>();
        if (cb == null) return false;
        return cb.cardOwner == currentTurn && cb.cardOwner == MyTurn();
    }

    // ─── NORMAL TURN SWITCH ───────────────────────────────────────
    public void SwitchTurn()
    {
        Turn next = currentTurn == Turn.Player1 ? Turn.Player2 : Turn.Player1;
        ApplyTurn(next);

        // Send via reliable event instead of RPC ← CHANGED
        if (PhotonNetwork.IsConnected)
            GameNetworkManager.Instance.SendTurn(next);
    }

    // ─── FORCE TURN (after collection) ───────────────────────────
    public void SetTurn(Turn newTurn)
    {
        ApplyTurn(newTurn);

        // Send via reliable event instead of RPC ← CHANGED
        if (PhotonNetwork.IsConnected)
            GameNetworkManager.Instance.SendTurn(newTurn);
    }

    // ─── CALLED ON RECEIVING CLIENT via TURN_EVENT ← NEW ─────────
    public void ApplyTurnNetwork(Turn t)
    {
        ApplyTurn(t);
        // No SendTurn() here — we are the receiver, not the sender
    }

    // ─── KEPT for safety (existing RPC calls won't crash) ────────
    [PunRPC]
    void RPC_SetTurn(int turnIndex)
    {
        // Now handled by TURN_EVENT — this is a no-op fallback
        ApplyTurn((Turn)turnIndex);
    }

    void ApplyTurn(Turn t)
    {
        currentTurn = t;
        UpdateTurnUI();
    }

    void UpdateTurnUI()
    {
        player1TurnText.SetActive(currentTurn == Turn.Player1);
        player2TurnText.SetActive(currentTurn == Turn.Player2);
    }
}
