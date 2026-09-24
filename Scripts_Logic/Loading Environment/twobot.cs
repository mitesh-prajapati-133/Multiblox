using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class twobot : MonoBehaviour
{
    // Start is called before the first frame update
  public void OnPlayButtonClicked()
  {
    SceneManager.LoadScene("Bot(Player 2)");
  }
}
