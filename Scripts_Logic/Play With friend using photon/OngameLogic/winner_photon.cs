using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
public class winner_photon : MonoBehaviour
{
    [Header("Result Panel")]
    public GameObject resultPanel;
    public TMP_Text winText;
    public TMP_Text byText;
    public TMP_Text subText;
bool isDisconnectWin = false;
    [Header("Stacks")]
    public RectTransform player1Stack;
    public RectTransform player2Stack;
    public RectTransform centerStack;

    [Header("Managers")]
    public timer_photon gameTimer;
    public round round;
    public turndown turndown;

    [Header("Audio")]
    public AudioSource winAudio;

    [Header("Side By-Card Texts (Drag from Hierarchy)")]
    public TMP_Text player1ByCardsText;
    public TMP_Text player2ByCardsText;

    [Header("Winner Images")]
    public GameObject player1WinnerImage;
    public GameObject player2WinnerImage;
    public float winnerImageDuration = 5f;
    [Header("Player Name Texts (Table UI)")]
public TMP_Text player1NameText;
public TMP_Text player2NameText;

    // ================= INTERNAL =================
    bool gameEnded = false;
    bool showingWinnerImage = false;
    float winnerTimer = 0f;

    string pendingWinnerName = "";
    int pendingByCards = 0;
    bool isDraw = false;

    CanvasGroup panelGroup;
    Vector3 startScale = new Vector3(0.85f, 0.85f, 1f);
    Vector3 endScale = Vector3.one;

    // =====================================================
    void Start()
    {
        resultPanel.SetActive(false);

        if (player1WinnerImage) player1WinnerImage.SetActive(false);
        if (player2WinnerImage) player2WinnerImage.SetActive(false);

        panelGroup = resultPanel.GetComponent<CanvasGroup>();
        if (panelGroup == null)
            panelGroup = resultPanel.AddComponent<CanvasGroup>();

        panelGroup.alpha = 0f;
        resultPanel.transform.localScale = startScale;
    }

    // =====================================================
    void Update()
    {
        if (showingWinnerImage)
        {
            winnerTimer += Time.deltaTime;
            if (winnerTimer >= winnerImageDuration)
            {
                showingWinnerImage = false;
                ShowResultPanel();
            }
            return;
        }

        if (gameEnded) return;

        if (round.currentState == round.RoundState.Playing)
        CheckAllCardsWin();
    
        CheckTimeOver();
    }

    // =====================================================
    void CheckAllCardsWin()
    {
        if (centerStack.childCount != 0)
            return;

        int p1 = player1Stack.childCount;
        int p2 = player2Stack.childCount;

        if (p1 > 0 && p2 == 0)
         EndWin(player1NameText.text, 1);
        else if (p2 > 0 && p1 == 0)
         EndWin(player2NameText.text, 2);
    }

    // =====================================================
    void CheckTimeOver()
    {
          if (gameTimer.currentRemaining > 0) return;

    // ✅ Wait for collection/redeal to finish
    if (round.currentState != round.RoundState.Playing) return;

    // ✅ Wait for redeal animation to finish
    if (redeal.Instance != null && !turndown.Instance.enabled) return;

    // ✅ Wait until centerStack is empty — same as local winner script
    if (centerStack.childCount > 0) return;

    int p1 = player1Stack.childCount;
    int p2 = player2Stack.childCount;

    if (p1 > p2)
        EndWin(player1NameText.text, 1);
    else if (p2 > p1)
        EndWin(player2NameText.text, 2);
    else
        EndDraw(p1);
    }

    // =====================================================
   void EndWin(string winnerName, int playerIndex)
    {
        if (gameEnded) return;
        gameEnded = true;
 turndown.Instance.enabled = false;
        int p1 = player1Stack.childCount;
        int p2 = player2Stack.childCount;
        pendingByCards = Mathf.Abs(p1 - p2);
        pendingWinnerName = winnerName;
        isDraw = false;

        // 🔊 Only play win audio
        if (winAudio && !winAudio.isPlaying)
            winAudio.Play();

        // Clear both side texts first
        if (player1ByCardsText) player1ByCardsText.text = "";
        if (player2ByCardsText) player2ByCardsText.text = "";

        // Update only winner side text
       if (playerIndex == 1)
{
    if (player1WinnerImage) player1WinnerImage.SetActive(true);
    if (player1ByCardsText)
        player1ByCardsText.text = "by " + pendingByCards + " cards";
}
else
{
    if (player2WinnerImage) player2WinnerImage.SetActive(true);
    if (player2ByCardsText)
        player2ByCardsText.text = "by " + pendingByCards + " cards";
}

        showingWinnerImage = true;
        winnerTimer = 0f;
    }

    // =====================================================
    void EndDraw(int sameCards)
    {
        if (gameEnded) return;

        gameEnded = true;
            turndown.Instance.enabled = false;
        isDraw = true;
        pendingByCards = sameCards;

        // 🔊 Only play win audio
        if (winAudio && !winAudio.isPlaying)
            winAudio.Play();

        // Do not show winner images
        if (player1WinnerImage) player1WinnerImage.SetActive(false);
        if (player2WinnerImage) player2WinnerImage.SetActive(false);

        // Do not show side by-card texts
        if (player1ByCardsText) player1ByCardsText.text = "";
        if (player2ByCardsText) player2ByCardsText.text = "";

        // Short delay then show result panel
        Invoke(nameof(ShowResultPanel), 0.5f);
    }

    // =====================================================
    void ShowResultPanel()
    {
        if (player1WinnerImage) player1WinnerImage.SetActive(false);
        if (player2WinnerImage) player2WinnerImage.SetActive(false);

        resultPanel.SetActive(true);
        panelGroup.alpha = 1f;
        resultPanel.transform.localScale = endScale;

        if (isDraw)
        {
            winText.text = "Draw";
            byText.text = "by same cards (" + pendingByCards + ")";
            subText.text = "Well played! Both players are equal.";
        }
        else
        {
            winText.text = pendingWinnerName + " Wins !!";
          if (isDisconnectWin)
{
    byText.text = "Opponent left the game";
}
else
{
    byText.text = "by " + pendingByCards + " cards";
}
          if (isDisconnectWin)
{
    subText.text = "Opponent disconnected. Victory awarded!";
}
else
{
    subText.text = "Amazing job! You defeated your opponent!";
}
        }
    }

    // =====================================================
    

    public void GoHome()
    {
        SceneManager.LoadScene("Main menu");
    }
public void ForceWin()
{
      if (gameEnded) return;

    gameEnded = true;

isDisconnectWin = true;
    if (turndown != null)
        turndown.enabled = false;

    pendingByCards = 0;
    isDraw = false;

    // 🔊 Play win sound
    if (winAudio && !winAudio.isPlaying)
        winAudio.Play();

    // Clear texts
    if (player1ByCardsText) player1ByCardsText.text = "";
    if (player2ByCardsText) player2ByCardsText.text = "";

    // ✅ Detect which player is YOU
    bool isPlayer1 = PhotonNetwork.IsMasterClient;

    if (isPlayer1)
    {
             pendingWinnerName = player1NameText.text;
        if (player1WinnerImage) player1WinnerImage.SetActive(true);
        if (player1ByCardsText)
            player1ByCardsText.text = "by disconnect";
    }
    else
    {
         pendingWinnerName = player2NameText.text;
        if (player2WinnerImage) player2WinnerImage.SetActive(true);
        if (player2ByCardsText)
            player2ByCardsText.text = "by disconnect";
    }

    // ✅ SAME FLOW AS NORMAL WIN
    showingWinnerImage = true;
    winnerTimer = 0f;
}
public bool IsGameEnded()
{
    return gameEnded;
}
}
