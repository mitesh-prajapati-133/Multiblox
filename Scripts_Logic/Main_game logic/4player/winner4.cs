using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;  

public class winner4 : MonoBehaviour
{
  [Header("Result Panel")]
    public GameObject resultPanel;
    public TMP_Text winText;
    public TMP_Text byText;
    public TMP_Text subText;

    [Header("Stacks")]
    public RectTransform player1Stack;
    public RectTransform player2Stack;
    public RectTransform player3Stack;
    public RectTransform player4Stack;
    public RectTransform centerStack;

    [Header("Live In-Game Card Count UI")]
    public TMP_Text player1CountText;
    public TMP_Text player2CountText;
    public TMP_Text player3CountText;
    public TMP_Text player4CountText;
    [Header("Player Name Texts (Table UI)")]
public TMP_Text player1NameText;
public TMP_Text player2NameText;
public TMP_Text player3NameText;
public TMP_Text player4NameText;

    [Header("Timer")]
    public timer gameTimer;

    [Header("Winner Images")]
    public GameObject player1WinnerImage;
    public GameObject player2WinnerImage;
    public GameObject player3WinnerImage;
    public GameObject player4WinnerImage;
    public float winnerImageDuration = 5f;

    [Header("Audio")]
    public AudioSource winAudio; // assign in Inspector

    // ================= INTERNAL =================
    bool gameEnded = false;
    bool showingWinnerImage = false;
    bool isDraw = false;

    float winnerTimer = 0f;
    string winnerName = "";
    int winnerCards = 0;

    // =====================================================
    void Start()
    {
        resultPanel.SetActive(false);

        player1WinnerImage.SetActive(false);
        player2WinnerImage.SetActive(false);
        player3WinnerImage.SetActive(false);
        player4WinnerImage.SetActive(false);

        UpdateLiveCardTexts(); // initial update
    }

    // =====================================================
    void Update()
    {
        // ✅ ALWAYS UPDATE IN-GAME TEXT
        UpdateLiveCardTexts();

        if (gameEnded)
        {
            if (showingWinnerImage)
            {
                winnerTimer += Time.deltaTime;
                if (winnerTimer >= winnerImageDuration)
                {
                    showingWinnerImage = false;
                    ShowResultPanel();
                }
            }
            return;
        }

        if (roundmanager4.Instance.currentState != roundmanager4.RoundState.Playing)
            return;

        CheckInstantWin();
        CheckTimeOver();
    }

    // =====================================================
    void UpdateLiveCardTexts()
    {
        if (player1CountText)
      player1CountText.text = player1NameText.text + " : " + player1Stack.childCount + " cards";
        if (player2CountText)
       player2CountText.text = player2NameText.text + " : " + player2Stack.childCount + " cards";

        if (player3CountText)
player3CountText.text = player3NameText.text + " : " + player3Stack.childCount + " cards";
        if (player4CountText)
            player4CountText.text = player4NameText.text + " : " + player4Stack.childCount + " cards";
    }

    // =====================================================
    void CheckInstantWin()
    {
        if (centerStack.childCount > 0) return;

        int p1 = player1Stack.childCount;
        int p2 = player2Stack.childCount;
        int p3 = player3Stack.childCount;
        int p4 = player4Stack.childCount;

        int alive = 0;
        if (p1 > 0) alive++;
        if (p2 > 0) alive++;
        if (p3 > 0) alive++;
        if (p4 > 0) alive++;

        if (alive != 1) return;

if (p1 > 0) EndWin(player1NameText.text, 1);
else if (p2 > 0) EndWin(player2NameText.text, 2);
else if (p3 > 0) EndWin(player3NameText.text, 3);
else EndWin(player4NameText.text, 4);
    }

    // =====================================================
    void CheckTimeOver()
    {
        if (gameTimer.time > 0) return;
        if (centerStack.childCount > 0) return;

        int p1 = player1Stack.childCount;
        int p2 = player2Stack.childCount;
        int p3 = player3Stack.childCount;
        int p4 = player4Stack.childCount;

        // Draw check
        if (p1 == p2 && p2 == p3 && p3 == p4)
        {
            EndDraw();
            return;
        }

        // Find max
        int maxCards = Mathf.Max(p1, Mathf.Max(p2, Mathf.Max(p3, p4)));

      if (maxCards == p1) EndWin(player1NameText.text, 1);
else if (maxCards == p2) EndWin(player2NameText.text, 2);
else if (maxCards == p3) EndWin(player3NameText.text, 3);
else EndWin(player4NameText.text, 4);
    }

    // =====================================================
    void EndWin(string name, int playerIndex)
    {
        if (gameEnded) return;
    gameEnded = true;
    isDraw = false;

    winnerName = name;

    switch (playerIndex)
    {
        case 1: winnerCards = player1Stack.childCount; break;
        case 2: winnerCards = player2Stack.childCount; break;
        case 3: winnerCards = player3Stack.childCount; break;
        case 4: winnerCards = player4Stack.childCount; break;
    }

    StopAllSoundsExceptWinner();

    if (winAudio && !winAudio.isPlaying)
        winAudio.Play();

    // ✅ Winner image (in-game screen)
    player1WinnerImage.SetActive(playerIndex == 1);
    player2WinnerImage.SetActive(playerIndex == 2);
    player3WinnerImage.SetActive(playerIndex == 3);
    player4WinnerImage.SetActive(playerIndex == 4);

    // ✅ Winner panel text
    winText.text = winnerName + " Wins!!";
    byText.text = "with <color=green>" + winnerCards + "</color> cards";
    subText.text = "Amazing job! You defeated your opponents!";

    showingWinnerImage = true;
    winnerTimer = 0f;
    }

    // =====================================================
    void EndDraw()
    {
        if (gameEnded) return;
        gameEnded = true;
        isDraw = true;

        winnerCards = player1Stack.childCount; // all equal

        // 🔊 Stop all other sounds first
        StopAllSoundsExceptWinner();

        // 🔊 Play winner audio
        if (winAudio && !winAudio.isPlaying)
            winAudio.Play();

        Invoke(nameof(ShowResultPanel), 0.5f);
    }

    // =====================================================
    void ShowResultPanel()
    {
        player1WinnerImage.SetActive(false);
        player2WinnerImage.SetActive(false);
        player3WinnerImage.SetActive(false);
        player4WinnerImage.SetActive(false);

        resultPanel.SetActive(true);

        if (isDraw)
        {
            winText.text = "Draw";
            byText.text = "with <color=green>" + winnerCards + "</color> cards";
            subText.text = "Well played! All players are equal.";
        }
        else
        {
            winText.text = winnerName + " Wins!!";
            byText.text = "with <color=green>" + winnerCards + "</color> cards";
            subText.text = "Amazing job! You defeated your opponents!";
        }
    }

    // =====================================================
    public void RetryGame()
    {
        StopWinSound();
          Scene currentScene = SceneManager.GetActiveScene();

    SceneManager.LoadScene(currentScene.name);
    }

    public void GoHome()
    {
        StopWinSound();
        SceneManager.LoadScene("Main menu");
    }

    void StopWinSound()
    {
        if (winAudio && winAudio.isPlaying)
            winAudio.Stop();
    }

    void StopAllSoundsExceptWinner()
    {
        // Stop all AudioSources in the scene except winAudio
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (var audio in allAudio)
        {
            if (audio != null && audio != winAudio && audio.isPlaying)
                audio.Stop();
        }
    }

}
