using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotController4 : MonoBehaviour
{
        [Header("Shuffle Buttons")]
    public Button player2ShuffleButton;
    public Button player3ShuffleButton;
    public Button player4ShuffleButton;

    [Header("Bot Settings")]
    public float shuffleChance = 0.35f;

    private turnmanager4.Turn lastTurn;
    private bool shuffleCheckedThisTurn = false;

    [Header("Mode")]
    public bool bot2Enabled = true;   // toggle: true = Player2 is bot
    public bool bot3Enabled = true;
    public bool bot4Enabled = true;   // toggle: true = Player3 is bot

    [Header("Identity")]
    public turnmanager4.Turn bot2Turn = turnmanager4.Turn.Player2;
    public turnmanager4.Turn bot3Turn = turnmanager4.Turn.Player3;
    public turnmanager4.Turn bot4Turn = turnmanager4.Turn.Player4;
    public Transform bot2Stack;
    public Transform bot3Stack;
    public Transform bot4Stack;
    public RectTransform centerStack;

    [Header("Timing")]
    public float thinkDelayMin = 0.6f;
    public float thinkDelayMax = 1.2f;
    public float moveSpeed = 1200f;
    public float extraDelayAfterPlace = 0.25f;

    [Header("Audio")]
    public AudioSource botAudioSource;
    public AudioClip placeClip;

    bool bot2Processing = false;
bool bot3Processing = false;
bool bot4Processing = false;

    void Start()
    {
        if (centerStack == null && roundmanager4.Instance != null)
            centerStack = roundmanager4.Instance.centerStack;

        ForceProtectBotCards();
    }

    void Update()
    {
        if (carddistributor4.IsDistributing) return;
        if (roundmanager4.Instance == null) return;
        if (turnmanager4.Instance == null) return;

        // Detect turn change
        if (turnmanager4.Instance.currentTurn != lastTurn)
        {
            lastTurn = turnmanager4.Instance.currentTurn;
            shuffleCheckedThisTurn = false;
        }

        // Bot2 move
        if (bot2Enabled && !bot2Processing &&
            roundmanager4.Instance.currentState == roundmanager4.RoundState.Playing &&
            turnmanager4.Instance.currentTurn == bot2Turn)
        {
           StartCoroutine(PerformBotMove(bot2Stack, player2ShuffleButton, bot2Turn, 2));
        }

        // Bot3 move
        if (bot3Enabled && !bot3Processing&&
            roundmanager4.Instance.currentState == roundmanager4.RoundState.Playing &&
            turnmanager4.Instance.currentTurn == bot3Turn)
        {
          StartCoroutine(PerformBotMove(bot3Stack, player3ShuffleButton, bot3Turn, 3));
        }
        // Bot4 move
if (bot4Enabled && !bot4Processing &&
    roundmanager4.Instance.currentState == roundmanager4.RoundState.Playing &&
    turnmanager4.Instance.currentTurn == bot4Turn)
{
    StartCoroutine(PerformBotMove(bot4Stack, player4ShuffleButton, bot4Turn, 4));
}
    }

    void ProtectBotCards(Transform stack, turnmanager4.Turn owner)
    {
        if (stack == null) return;

        foreach (Transform t in stack)
        {
            cardbehaviour4 cb = t.GetComponent<cardbehaviour4  >();
            if (cb != null)
            {
                cb.cardOwner = owner;
            }
        }
    }

    IEnumerator PerformBotMove(Transform botStack, Button shuffleButton, turnmanager4.Turn botTurn,int botNumber)
    {
if (botNumber == 2)
    bot2Processing = true;
else if (botNumber == 3)
    bot3Processing = true;
else if (botNumber == 4)
    bot4Processing = true;

        if (!shuffleCheckedThisTurn)
        {
            shuffleCheckedThisTurn = true;

            int count = botStack.childCount;

            if (shuffleButton != null && count > 1)
            {
                if (count < 5 || Random.value < shuffleChance)
                {
                    shuffleButton.onClick.Invoke();
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        float think = Random.Range(thinkDelayMin, thinkDelayMax);
        yield return new WaitForSeconds(think);

        if (botStack == null || botStack.childCount == 0)
        {
            if (botNumber == 2)
    bot2Processing = false;
else if (botNumber == 3)
    bot3Processing = false;
else if (botNumber == 4)
    bot4Processing = false;

yield break;
        }

        Transform top = botStack.GetChild(botStack.childCount - 1);
        if (top == null)
        {
             if (botNumber == 2)
    bot2Processing = false;
else if (botNumber == 3)
    bot3Processing = false;
else if (botNumber == 4)
    bot4Processing = false;
    yield break;
        }

      cardbehaviour4   cb = top.GetComponent<cardbehaviour4 >();
        RectTransform rect = top as RectTransform;
        Image img = top.GetComponent<Image>();

        if (cb != null) cb.cardOwner = botTurn;

        Sprite frontSprite = cb != null ? cb.frontCard : null;
        Vector2 tableSize = cb != null ? cb.tableCardSize : Vector2.zero;
        float snapDistance = cb != null ? cb.snapDistance : 70f;
        AudioSource centerAudio = cb != null ? cb.centerAudioSource : null;
        AudioClip stickClip = cb != null ? cb.stickClip : null;

        if (rect != null && centerStack != null)
        {
            rect.SetParent(centerStack.parent, true);
            while (Vector3.Distance(rect.position, centerStack.position) > snapDistance)
            {
                rect.position = Vector3.MoveTowards(rect.position, centerStack.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            rect.SetParent(centerStack, false);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = (tableSize != Vector2.zero) ? tableSize : rect.sizeDelta;
            rect.localScale = Vector3.one;

            if (img != null && frontSprite != null) img.sprite = frontSprite;

            if (centerAudio != null && stickClip != null) centerAudio.PlayOneShot(stickClip);
            else if (botAudioSource != null && placeClip != null) botAudioSource.PlayOneShot(placeClip);

            roundmanager4.Instance.OnCardPlaced(cb);

            yield return null;

            bool isMatch = false;
            if (centerStack.childCount >= 2)
            {
                int last = centerStack.GetChild(centerStack.childCount - 1).GetComponent<carddealer4>().cardValue;
                int prev = centerStack.GetChild(centerStack.childCount - 2).GetComponent<carddealer4>().cardValue;
                if (last == prev) isMatch = true;
            }
        }
            
        

        ProtectBotCards(botStack, botTurn);

        yield return new WaitForSeconds(extraDelayAfterPlace);
        if (botNumber == 2)
    bot2Processing = false;
else if (botNumber == 3)
    bot3Processing = false;
else if (botNumber == 4)
    bot4Processing = false;
}
    public void ForceProtectBotCards()
    {
        ProtectBotCards(bot2Stack, bot2Turn);
        ProtectBotCards(bot3Stack, bot3Turn);
        ProtectBotCards(bot4Stack, bot4Turn);
    }

}
