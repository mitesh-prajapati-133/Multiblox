using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class JoinSceneManager : MonoBehaviour
{
     public static JoinSceneManager Instance;

    public TMP_InputField codeInputField;
    public TMP_Text errorText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false);

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public void ClickJoin()
    {
        string code = codeInputField.text.Trim();

        if (code.Length < 6)
        {
            ShowError("Enter 6 digit code!");
            return;
        }

        ShowError("");

        if (RoomManager.Instance != null)
            RoomManager.Instance.JoinGame(code);
        else
            PhotonNetwork.JoinRoom(code);
    }

    public void ShowError(string msg)
    {
        if (errorText == null) return;
        errorText.gameObject.SetActive(!string.IsNullOrEmpty(msg));
        errorText.text = msg;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("lan menu");
    }
}
