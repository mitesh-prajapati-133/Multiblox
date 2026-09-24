using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class playerUpdateUi : MonoBehaviour
{
    public TMP_Text player1NameText;
    public TMP_Text player2NameText;
    public TMP_Text player3NameText;

    void Start()
    {
        player1NameText.text = PlayerPrefs.GetString("P1_NAME", "Player 1");
        player2NameText.text = PlayerPrefs.GetString("P2_NAME", "Player 2");
        player3NameText.text = PlayerPrefs.GetString("P3_NAME", "Player 3");
    }
}
