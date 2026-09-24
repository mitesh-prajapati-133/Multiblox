using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class layerturn : MonoBehaviour
{
        [Header("Timer Settings")]
    public float turnTime = 10f;

    [Header("Player Layers")]
    public Image player1Layer;
    public Image player2Layer;
    public Image player3Layer;

    float timer;
    bool timerRunning;
    public AudioSource countdownAudio;
    public AudioClip countdownClip;

    bool soundStarted = false;
    turnmanager1.Turn activeTurn;

    void Start()
    {
        ResetAllLayers();
        timerRunning = false;   // 🔒 timer OFF at start
    }

    void Update()
    {
        if (turnmanager1.Instance == null)
            return;

        // 🚫 DO NOTHING during distribution
        if (NewBehaviourScript1.IsDistributing ||
            roundmanager1.Instance.currentState != roundmanager1.RoundState.Playing)
        {
            timerRunning = false;
            ResetAllLayers();
            return;
        }

        // ✅ Start timer ONLY AFTER distribution finishes
        if (!timerRunning)
        {
            StartTurn(turnmanager1.Instance.currentTurn);
        }

        // 🔄 Turn changed externally (collect logic etc.)
        if (turnmanager1.Instance.currentTurn != activeTurn)
        {
            StopCountdownSound(); 
            StartTurn(turnmanager1.Instance.currentTurn);
        }

        timer -= Time.deltaTime;
        if (timer <= 4f && timer > 0f && !soundStarted)
        {
            soundStarted = true;
            if (countdownAudio && countdownClip)
            {
                countdownAudio.clip = countdownClip;
                countdownAudio.Play();
            }
        }

        Image layer = GetLayer(activeTurn);
        layer.fillAmount = Mathf.Clamp01(timer / turnTime);

        // ⏱ TIME OVER → PASS TURN
        if (timer <= 0f)
        {
            if (!timerRunning) return;
            StopCountdownSound();
    timerRunning = false;

    layer.fillAmount = 0f;
    layer.gameObject.SetActive(false);

    bool placedThisTimeout = false;
    
    

    // 🔹 Check alive players
    bool p1Alive = roundmanager1.Instance.player1Stack.childCount > 0;
    bool p2Alive = roundmanager1.Instance.player2Stack.childCount > 0;
    bool p3Alive = roundmanager1.Instance.player3Stack.childCount > 0;

    int aliveCount = 0;
    if (p1Alive) aliveCount++;
    if (p2Alive) aliveCount++;
    if (p3Alive) aliveCount++;

    // 🔥 SPECIAL CASE (INSPIRED FROM 2 PLAYER):
    // Only ONE player remains alive → auto-stick on timeout
    if (aliveCount == 1)
    {
        AutoStickTopCardImmediate(activeTurn);
        placedThisTimeout = true;
    }
    else
    {
if (cardbeaviour1.ActiveCardExists())
    {
        cardbeaviour1.GetActiveCard().StopDrag(); // ensure drag stops
        AutoStickTopCardImmediate(activeTurn);
        placedThisTimeout = true;
    }
    else
    {
        // 🔹 Normal timeout behavior
        placedThisTimeout = cardbeaviour1.ForceReleaseActiveCard();
    }

    }

    // 🔄 PASS TURN only if no card was placed
    if (!placedThisTimeout)
    {
        turnmanager1.Instance.SwitchTurn();
    }
        }
    }

    void StartTurn(turnmanager1.Turn turn)
    {
        // 🔥 SKIP DEAD PLAYERS
        if (!IsPlayerAlive(turn))
        {
            StopCountdownSound();
            timerRunning = false;
            turnmanager1.Instance.SwitchTurn();
            return;
        }

        ResetAllLayers();

        activeTurn = turn;
        timer = turnTime;
        timerRunning = true;

        Image layer = GetLayer(turn);
        layer.gameObject.SetActive(true);
        layer.fillAmount = 1f;
    }

    void ResetAllLayers()
    {
        player1Layer.gameObject.SetActive(false);
        player2Layer.gameObject.SetActive(false);
        player3Layer.gameObject.SetActive(false);

        player1Layer.fillAmount = 1f;
        player2Layer.fillAmount = 1f;
        player3Layer.fillAmount = 1f;
    }

    Image GetLayer(turnmanager1.Turn turn)
    {
        switch (turn)
        {
            case turnmanager1.Turn.Player1: return player1Layer;
            case turnmanager1.Turn.Player2: return player2Layer;
            default: return player3Layer;
        }
    }

    void StopCountdownSound()
    {
        if (countdownAudio && countdownAudio.isPlaying)
            countdownAudio.Stop();

        soundStarted = false;
    }

    bool IsPlayerAlive(turnmanager1.Turn turn)
    {
        switch (turn)
        {
            case turnmanager1.Turn.Player1:
                return roundmanager1.Instance.player1Stack.childCount > 0;
            case turnmanager1.Turn.Player2:
                return roundmanager1.Instance.player2Stack.childCount > 0;
            case turnmanager1.Turn.Player3:
                return roundmanager1.Instance.player3Stack.childCount > 0;
            default:
                return false;
        }
    }
    void AutoStickTopCardImmediate(turnmanager1.Turn turn)
{
    RectTransform stack = null;

    switch (turn)
    {
        case turnmanager1.Turn.Player1:
            stack = roundmanager1.Instance.player1Stack;
            break;
        case turnmanager1.Turn.Player2:
            stack = roundmanager1.Instance.player2Stack;
            break;
        case turnmanager1.Turn.Player3:
            stack = roundmanager1.Instance.player3Stack;
            break;
    }

    if (stack == null || stack.childCount == 0)
        return;

    Transform topCard = stack.GetChild(stack.childCount - 1);
    cardbeaviour1 cb = topCard.GetComponent<cardbeaviour1>();
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
    roundmanager1.Instance.OnCardPlaced(cb);


}

}
