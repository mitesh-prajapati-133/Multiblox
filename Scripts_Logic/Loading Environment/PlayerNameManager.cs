using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class PlayerNameManager : MonoBehaviour
{
    public InputField player1Input;
    public InputField player2Input;
    public InputField player3Input;
    public InputField player4Input;

    public void OnPlayButton()
    {
        // Save names (use default if empty)
        PlayerPrefs.SetString("P1", player1Input.text == "" ? "Player 1" : player1Input.text);
        PlayerPrefs.SetString("P2", player2Input.text == "" ? "Player 2" : player2Input.text);
        PlayerPrefs.SetString("P3", player3Input.text == "" ? "Player 3" : player3Input.text);
        PlayerPrefs.SetString("P4", player4Input.text == "" ? "Player 4" : player4Input.text);

        PlayerPrefs.Save();

        // Load next scene
        SceneManager.LoadScene("Play 2");
    }
}
