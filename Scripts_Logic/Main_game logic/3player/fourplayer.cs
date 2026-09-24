using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  
using TMPro;

public class fourplayer : MonoBehaviour
{
    // Start is called before the first frame update
     public TMP_InputField player1Input;
    public TMP_InputField player2Input;
    public TMP_InputField player3Input;
  public TMP_InputField player4Input;
    public void OnPlayButtonClicked()
    {
        string p1;
        string p2;
        string p3;
  string p4;
        // Player 1
        if (player1Input.text == "")
            p1 = "Player 1";
        else
            p1 = player1Input.text;

        // Player 2
        if (player2Input.text == "")
            p2 = "Player 2";
        else
            p2 = player2Input.text;

        // Player 3
        if (player3Input.text == "")
            p3 = "Player 3";
        else
            p3 = player3Input.text;

 if (player4Input.text == "")
            p4 = "Player 4";
        else
            p4 = player4Input.text;

        // Save names
        PlayerPrefs.SetString("P1_NAME", p1);
        PlayerPrefs.SetString("P2_NAME", p2);
        PlayerPrefs.SetString("P3_NAME", p3);
        PlayerPrefs.SetString("P4_NAME", p4);
        PlayerPrefs.Save();

        // Load War Player scene
        SceneManager.LoadScene("Play 2"); // exact scene name
    }
   
}
