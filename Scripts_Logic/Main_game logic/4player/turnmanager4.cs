using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class turnmanager4 : MonoBehaviour
{
       public static turnmanager4 Instance;

    public enum Turn
    {
        Player1,
        Player2,
        Player3,
        Player4
    }

    public Turn currentTurn = Turn.Player1;

    public GameObject player1TurnText;
    public GameObject player2TurnText;
    public GameObject player3TurnText;
    public GameObject player4TurnText;

    void Awake()
    {
        Instance = this;
        UpdateTurnUI();
    }

    // 🔒 TURN CHECK
    public bool CanDrag(Transform card)
    {
        cardbehaviour4 cb = card.GetComponent<cardbehaviour4>();
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
        else if (currentTurn == Turn.Player3)
            currentTurn = Turn.Player4;
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

        if (player4TurnText != null)
            player4TurnText.SetActive(currentTurn == Turn.Player4);
    }

    public void SetTurn(Turn newTurn)
    {
        currentTurn = newTurn;
        UpdateTurnUI();
    }

}
