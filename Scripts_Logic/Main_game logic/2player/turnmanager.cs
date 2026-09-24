using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class turnmanager : MonoBehaviour
{
       public static turnmanager Instance;

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

   public bool CanDrag(Transform card)
{
    cardbehavior cb = card.GetComponent<cardbehavior>();
    if (cb == null) return false;

    return cb.cardOwner == currentTurn;
}


    // 🔥 NORMAL TURN SWITCH (used only when NO match)
    public void SwitchTurn()
    {
        currentTurn =
            currentTurn == Turn.Player1 ? Turn.Player2 : Turn.Player1;

        UpdateTurnUI();
    }

    // 🔥 FORCE TURN TO WINNER (USED AFTER CENTER COLLECTION)
    public void SetTurn(Turn newTurn)
    {
        currentTurn = newTurn;
        UpdateTurnUI();
    }

    void UpdateTurnUI()
    {
        player1TurnText.SetActive(currentTurn == Turn.Player1);
        player2TurnText.SetActive(currentTurn == Turn.Player2);
    }
    
    
}
