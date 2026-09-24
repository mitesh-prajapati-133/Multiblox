using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;  

public class winner1 : MonoBehaviour
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
    public RectTransform centerStack;

public TMP_Text player1NameText;
public TMP_Text player2NameText;
public TMP_Text player3NameText;

    [Header("Live In-Game Card Count UI")]
    public TMP_Text player1CountText;
    public TMP_Text player2CountText;
    public TMP_Text player3CountText;

    [Header("Timer")]
    public timer gameTimer;

    [Header("Winner Images")]
    public GameObject player1WinnerImage;
    public GameObject player2WinnerImage;
    public GameObject player3WinnerImage;
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

        if (roundmanager1.Instance.currentState != roundmanager1.RoundState.Playing)
            return;

        CheckInstantWin();
        CheckTimeOver();
    }

    // =====================================================
    void UpdateLiveCardTexts()
    {
        if (player1CountText)
          player1CountText.text = player1NameText.text + " : " + player1Stack.childCount;

        if (player2CountText)
          player2CountText.text = player2NameText.text + " : " + player2Stack.childCount;

        if (player3CountText)
            player3CountText.text = player3NameText.text + " : " + player3Stack.childCount;
    }

    // =====================================================
    void CheckInstantWin()
    {
        if (centerStack.childCount > 0) return;

        int p1 = player1Stack.childCount;
        int p2 = player2Stack.childCount;
        int p3 = player3Stack.childCount;

        int alive = 0;
        if (p1 > 0) alive++;
        if (p2 > 0) alive++;
        if (p3 > 0) alive++;

        if (alive != 1) return;
if (p1 > 0) EndWin(player1NameText.text, 1);
else if (p2 > 0) EndWin(player2NameText.text, 2);
else EndWin(player3NameText.text, 3);
    }

    // =====================================================
    void CheckTimeOver()
    {
        if (gameTimer.time > 0) return;
        if (centerStack.childCount > 0) return;

        int p1 = player1Stack.childCount;
        int p2 = player2Stack.childCount;
        int p3 = player3Stack.childCount;

        if (p1 == p2 && p2 == p3)
        {
            EndDraw();
            return;
        }

if (p1 > p2 && p1 > p3)
    EndWin(player1NameText.text, 1);
else if (p2 > p1 && p2 > p3)
    EndWin(player2NameText.text, 2);
else
    EndWin(player3NameText.text, 3);
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
}

        // 🔊 Stop all other sounds first
        StopAllSoundsExceptWinner();

        // 🔊 Play winner audio
        if (winAudio && !winAudio.isPlaying)
            winAudio.Play();

     player1WinnerImage.SetActive(playerIndex == 1);
player2WinnerImage.SetActive(playerIndex == 2);
player3WinnerImage.SetActive(playerIndex == 3);

        showingWinnerImage = true;
        winnerTimer = 0f;
    }

    // =====================================================
    void EndDraw()
    {
        if (gameEnded) return;
        gameEnded = true;
        isDraw = true;

        winnerCards = player1Stack.childCount;

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

        resultPanel.SetActive(true);

        if (isDraw)
        {
            winText.text = "Draw";
            byText.text = "with " + winnerCards + " cards";
            subText.text = "Well played! All players are equal.";
        }
        else
        {
            winText.text = winnerName + " Wins!!";
            byText.text = "with " + winnerCards + " cards";
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
