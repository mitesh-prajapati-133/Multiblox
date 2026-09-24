using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class joinscene : MonoBehaviour
{
     public void OpenJoinScene()
    {
        SceneManager.LoadScene("join lan");
    }
}
