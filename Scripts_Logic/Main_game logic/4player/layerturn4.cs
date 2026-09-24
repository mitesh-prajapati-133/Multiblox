using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class layerturn4 : MonoBehaviour
{
       [Header("Timer Settings")]
    public float turnTime = 10f;

    [Header("Player Layers")]
    public Image player1Layer;
    public Image player2Layer;
    public Image player3Layer;
    public Image player4Layer;

    float timer;
    bool timerRunning;
    public AudioSource countdownAudio;
    public AudioClip countdownClip;

    bool soundStarted = false;
    turnmanager4.Turn activeTurn;

    void Start()
    {
        ResetAllLayers();
        timerRunning = false;   // 🔒 timer OFF at start
    }

    void Update()
    {
        if (turnmanager4.Instance == null)
            return;

        // 🚫 DO NOTHING during distribution
        if (carddistributor4.IsDistributing ||
            roundmanager4.Instance.currentState != roundmanager4.RoundState.Playing)
        {
            timerRunning = false;
            ResetAllLayers();
            return;
        }

        // ✅ Start timer ONLY AFTER distribution finishes
        if (!timerRunning)
        {
            StartTurn(turnmanager4.Instance.currentTurn);
        }

        // 🔄 Turn changed externally
        if (turnmanager4.Instance.currentTurn != activeTurn)
        {
            StopCountdownSound();
            StartTurn(turnmanager4.Instance.currentTurn);
        }

        timer -= Time.deltaTime;

        // 🔔 Play warning sound at last 4 seconds
        if (timer <= 4f && timer > 0f && !soundStarted)
        {
            soundStarted = true;
            if (countdownAudio && countdownClip)
            {
                countdownAudio.clip = countdownClip;
                countdownAudio.Play();
            }
        }

        // ⏹ Stop sound if card is placed during warning
       

        // ⏱ TIME OVER → PASS TURN
        if (timer <= 0f)
        {
            if (!timerRunning) return;
            StopCountdownSound();
            timerRunning = false;

            Image layer = GetLayer(activeTurn);
            layer.fillAmount = 0f;
            layer.gameObject.SetActive(false);
            bool placedThisTimeout = false;

// 🔹 Check alive players
bool p1Alive = roundmanager4.Instance.player1Stack.childCount > 0;
bool p2Alive = roundmanager4.Instance.player2Stack.childCount > 0;
bool p3Alive = roundmanager4.Instance.player3Stack.childCount > 0;
bool p4Alive = roundmanager4.Instance.player4Stack.childCount > 0;

int aliveCount = 0;
if (p1Alive) aliveCount++;
if (p2Alive) aliveCount++;
if (p3Alive) aliveCount++;
if (p4Alive) aliveCount++;

// 🔥 SAME CONCEPT AS 3-PLAYER
if (aliveCount == 1)
{
         AutoStickTopCardImmediate(activeTurn);
    placedThisTimeout = true;
}
else
{
    // normal timeout behavior
    if (cardbehaviour4.ActiveCardExists())
    {
        
         placedThisTimeout = cardbehaviour4.ForceStickActiveCardImmediate();
    }
    else
    {
        placedThisTimeout = cardbehaviour4.ForceReleaseActiveCard();
    }
}

// 🔄 PASS TURN only if nothing was placed
if (!placedThisTimeout)
{
    turnmanager4.Instance.SwitchTurn();
}

            // 🔄 PASS TURN
       return;
        }

        // ⏳ Update timer bar (clockwise rotation)
        Image activeLayer = GetLayer(activeTurn);
        activeLayer.fillAmount = Mathf.Clamp01(timer / turnTime);
    }

    void StartTurn(turnmanager4.Turn turn)
    {
        // 🔥 SKIP DEAD PLAYERS
        if (!IsPlayerAlive(turn))
        {
            StopCountdownSound();
            timerRunning = false;
            turnmanager4.Instance.SwitchTurn();
            return;
        }

        ResetAllLayers();

        activeTurn = turn;
        timer = turnTime;
        timerRunning = true;
        soundStarted = false;

        Image layer = GetLayer(turn);
        layer.gameObject.SetActive(true);
        layer.fillAmount = 1f;
    }

    void ResetAllLayers()
    {
        player1Layer.gameObject.SetActive(false);
        player2Layer.gameObject.SetActive(false);
        player3Layer.gameObject.SetActive(false);
        player4Layer.gameObject.SetActive(false);

        player1Layer.fillAmount = 1f;
        player2Layer.fillAmount = 1f;
        player3Layer.fillAmount = 1f;
        player4Layer.fillAmount = 1f;
    }

    Image GetLayer(turnmanager4.Turn turn)
    {
        switch (turn)
        {
            case turnmanager4.Turn.Player1: return player1Layer;
            case turnmanager4.Turn.Player2: return player2Layer;
            case turnmanager4.Turn.Player3: return player3Layer;
            case turnmanager4.Turn.Player4: return player4Layer;
            default: return player1Layer;
        }
    }

    void StopCountdownSound()
    {
        if (countdownAudio && countdownAudio.isPlaying)
            countdownAudio.Stop();

        soundStarted = false;
    }

    bool IsPlayerAlive(turnmanager4.Turn turn)
    {
        switch (turn)
        {
            case turnmanager4.Turn.Player1:
                return roundmanager4.Instance.player1Stack.childCount > 0;
            case turnmanager4.Turn.Player2:
                return roundmanager4.Instance.player2Stack.childCount > 0;
            case turnmanager4.Turn.Player3:
                return roundmanager4.Instance.player3Stack.childCount > 0;
            case turnmanager4.Turn.Player4:
                return roundmanager4.Instance.player4Stack.childCount > 0;
            default:
                return false;
        }
    }

    // 🔹 Auto-stick logic (commented until needed)
    /*
    void AutoStickTopCardImmediate(turnmanager4.Turn turn) { ... }
    */
void AutoStickTopCardImmediate(turnmanager4.Turn turn)
{
    RectTransform stack = null;

    switch (turn)
    {
        case turnmanager4.Turn.Player1:
            stack = roundmanager4.Instance.player1Stack;
            break;
        case turnmanager4.Turn.Player2:
            stack = roundmanager4.Instance.player2Stack;
            break;
        case turnmanager4.Turn.Player3:
            stack = roundmanager4.Instance.player3Stack;
            break;
        case turnmanager4.Turn.Player4:
            stack = roundmanager4.Instance.player4Stack;
            break;
    }

    if (stack == null || stack.childCount == 0)
        return;

    Transform topCard = stack.GetChild(stack.childCount - 1);
    cardbehaviour4 cb = topCard.GetComponent<cardbehaviour4>();
    if (cb == null) return;

    RectTransform rect = cb.GetComponent<RectTransform>();

    rect.SetParent(cb.centerStack, false);
    rect.anchoredPosition = Vector2.zero;
    rect.sizeDelta = cb.tableCardSize;
    rect.localScale = Vector3.one;
    rect.GetComponent<Image>().sprite = cb.frontCard;

    if (cb.centerAudioSource && cb.stickClip)
        cb.centerAudioSource.PlayOneShot(cb.stickClip);

    cb.cardOwner = turn;
    roundmanager4.Instance.OnCardPlaced(cb);
}

}
