using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotController3 : MonoBehaviour
{
      [Header("Shuffle Buttons")]
    public Button player2ShuffleButton;
    public Button player3ShuffleButton;

    [Header("Bot Settings")]
    public float shuffleChance = 0.35f;

    private turnmanager1.Turn lastTurn;
    private bool shuffleCheckedThisTurn = false;

    [Header("Mode")]
    public bool bot2Enabled = true;   // toggle: true = Player2 is bot
    public bool bot3Enabled = true;   // toggle: true = Player3 is bot

    [Header("Identity")]
    public turnmanager1.Turn bot2Turn = turnmanager1.Turn.Player2;
    public turnmanager1.Turn bot3Turn = turnmanager1.Turn.Player3;
    public Transform bot2Stack;
    public Transform bot3Stack;
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

    void Start()
    {
        if (centerStack == null && roundmanager1.Instance != null)
            centerStack = roundmanager1.Instance.centerStack;

        ForceProtectBotCards();
    }

    void Update()
    {
        if (NewBehaviourScript1.IsDistributing) return;
        if (roundmanager1.Instance == null) return;
        if (turnmanager1.Instance == null) return;

        // Detect turn change
        if (turnmanager1.Instance.currentTurn != lastTurn)
        {
            lastTurn = turnmanager1.Instance.currentTurn;
            shuffleCheckedThisTurn = false;
        }

        // Bot2 move
        if (bot2Enabled && !bot2Processing &&
            roundmanager1.Instance.currentState == roundmanager1.RoundState.Playing &&
            turnmanager1.Instance.currentTurn == bot2Turn)
        {
           StartCoroutine(PerformBotMove(bot2Stack, player2ShuffleButton, bot2Turn, true));
        }

        // Bot3 move
        if (bot3Enabled && !bot3Processing&&
            roundmanager1.Instance.currentState == roundmanager1.RoundState.Playing &&
            turnmanager1.Instance.currentTurn == bot3Turn)
        {
          StartCoroutine(PerformBotMove(bot3Stack, player3ShuffleButton, bot3Turn, false));
        }
    }

    void ProtectBotCards(Transform stack, turnmanager1.Turn owner)
    {
        if (stack == null) return;

        foreach (Transform t in stack)
        {
            cardbeaviour1 cb = t.GetComponent<cardbeaviour1 >();
            if (cb != null)
            {
                cb.cardOwner = owner;
            }
        }
    }

    IEnumerator PerformBotMove(Transform botStack, Button shuffleButton, turnmanager1.Turn botTurn, bool isBot2)
    {
      if (isBot2)
    bot2Processing = true;
else
    bot3Processing = true;

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
              if (isBot2)
        bot2Processing = false;
    else
        bot3Processing = false;
            yield break;
        }

        Transform top = botStack.GetChild(botStack.childCount - 1);
        if (top == null)
        {
              if (isBot2)
        bot2Processing = false;
    else
        bot3Processing = false;
            yield break;
        }

       cardbeaviour1  cb = top.GetComponent<cardbeaviour1 >();
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

            roundmanager1.Instance.OnCardPlaced(cb);

            yield return null;

            bool isMatch = false;
            if (centerStack.childCount >= 2)
            {
                int last = centerStack.GetChild(centerStack.childCount - 1).GetComponent<cardealer1>().cardValue;
                int prev = centerStack.GetChild(centerStack.childCount - 2).GetComponent<cardealer1>().cardValue;
                if (last == prev) isMatch = true;
            }

            
        }

        ProtectBotCards(botStack, botTurn);

        yield return new WaitForSeconds(extraDelayAfterPlace);
       if (isBot2)
    bot2Processing = false;
else
    bot3Processing = false;
    }

    public void ForceProtectBotCards()
    {
        ProtectBotCards(bot2Stack, bot2Turn);
        ProtectBotCards(bot3Stack, bot3Turn);
    }

}
