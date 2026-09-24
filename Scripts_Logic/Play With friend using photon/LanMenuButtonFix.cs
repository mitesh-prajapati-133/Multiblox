using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LanMenuButtonFix : MonoBehaviour
{
      public Button hostButton;
    public Button joinButton;

    void Start()
    {
        hostButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();

        hostButton.onClick.AddListener(() =>
        {
            RoomManager.Instance.HostGame();
        });

        joinButton.onClick.AddListener(() =>
        {
            FindObjectOfType<LoadJoinScene>().OpenJoinScene();
        });
    }
}
