using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class turnmanager1 : MonoBehaviour
{
     public static turnmanager1 Instance;

    public enum Turn
    {
        Player1,
        Player2,
        Player3
    }

    public Turn currentTurn = Turn.Player1;

    public GameObject player1TurnText;
    public GameObject player2TurnText;
    public GameObject player3TurnText;

    void Awake()
    {
        Instance = this;
        UpdateTurnUI();
    }

    // 🔒 TURN CHECK
    public bool CanDrag(Transform card)
    {
        cardbeaviour1 cb = card.GetComponent<cardbeaviour1>();
        if (cb == null) return false;

        return cb.cardOwner == currentTurn;
    }

    // 🔁 TURN ROTATION
    public void SwitchTurn()
    {
        if (currentTurn == Turn.Player1)
            currentTurn = Turn.Player2;
        else if (currentTurn == Turn.Player2)
            currentTurn = Turn.Player3;
        else
            currentTurn = Turn.Player1;

        UpdateTurnUI();
    }

    void UpdateTurnUI()
    {
        if (player1TurnText != null)
            player1TurnText.SetActive(currentTurn == Turn.Player1);

        if (player2TurnText != null)
            player2TurnText.SetActive(currentTurn == Turn.Player2);

        if (player3TurnText != null)
            player3TurnText.SetActive(currentTurn == Turn.Player3);
    }
    public void SetTurn(Turn newTurn)
{
    currentTurn = newTurn;
    UpdateTurnUI();
}
}
