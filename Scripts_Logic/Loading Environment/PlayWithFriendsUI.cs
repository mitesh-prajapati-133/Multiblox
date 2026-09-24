using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayWithFriendsUI : MonoBehaviour
{
     public GameObject playFriendsPanel;

    public void OpenPlayFriendsPanel()
    {
        if (playFriendsPanel != null)
            playFriendsPanel.SetActive(true);
    }

    public void ClosePlayFriendsPanel()
    {
        if (playFriendsPanel != null)
            playFriendsPanel.SetActive(false);
    }
}
