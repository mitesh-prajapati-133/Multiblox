using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowPlayerNames : MonoBehaviour
{
    // Start is called before the first frame update
      public TMP_Text player1Text;
    public TMP_Text player2Text;
    public TMP_Text player3Text;
    public TMP_Text player4Text;

    void Start()
    {
        player1Text.text = PlayerPrefs.GetString("P1", "Player 1");
        player2Text.text = PlayerPrefs.GetString("P2", "Player 2");
        player3Text.text = PlayerPrefs.GetString("P3", "Player 3");
        player4Text.text = PlayerPrefs.GetString("P4", "Player 4");
    }
}
