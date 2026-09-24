using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BotController : MonoBehaviour
{
    public Button player2ShuffleButton;
public float shuffleChance = 0.35f;

private turnmanager.Turn lastTurn;
private bool shuffleCheckedThisTurn = false;
     [Header("Mode")]
    public bool botEnabled = true;   // toggle: true = Player2 is bot, false = Player2 is human

    [Header("Identity")]
    public turnmanager.Turn botTurn = turnmanager.Turn.Player2;
    public Transform botStack;
    public RectTransform centerStack;

    [Header("Timing")]
    public float thinkDelayMin = 0.6f;
    public float thinkDelayMax = 1.2f;
    public float moveSpeed = 1200f;
    public float extraDelayAfterPlace = 0.25f;

    [Header("Audio")]
    public AudioSource botAudioSource;
    public AudioClip placeClip;

    bool isProcessing = false;

    void Start()
    {
        if (!botEnabled) return; // do nothing if human vs human mode
        if (centerStack == null && roundmanager.Instance != null)
            centerStack = roundmanager.Instance.centerStack;

        ProtectBotCards();
        
    }

    void Update()
    {
        if (!botEnabled) return;
    if (NewBehaviourScript.IsDistributing) return;
    if (roundmanager.Instance == null) return;
    if (turnmanager.Instance == null) return;

    // Detect turn change
    if (turnmanager.Instance.currentTurn != lastTurn)
    {
        lastTurn = turnmanager.Instance.currentTurn;
        shuffleCheckedThisTurn = false;
    }

    if (!isProcessing &&
        roundmanager.Instance.currentState == roundmanager.RoundState.Playing &&
        turnmanager.Instance.currentTurn == botTurn)
    {
        StartCoroutine(PerformBotMove());
    }
    }

    void ProtectBotCards()
    {
          if (botStack == null) return;

    foreach (Transform t in botStack)
    {
        cardbehavior cb = t.GetComponent<cardbehavior>();
        if (cb != null)
        {
            cb.cardOwner = botTurn;
        
        }
    }
    }

 

    IEnumerator PerformBotMove()
    {
        
        isProcessing = true;
          if (!shuffleCheckedThisTurn)
    {
        shuffleCheckedThisTurn = true;

        int count = botStack.childCount;

        if (player2ShuffleButton != null && count > 1)
        {
            if (count < 5 || Random.value < shuffleChance)
            {
                player2ShuffleButton.onClick.Invoke();
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
        float think = Random.Range(thinkDelayMin, thinkDelayMax);
        yield return new WaitForSeconds(think);

        if (botStack == null || botStack.childCount == 0)
        {
            
            isProcessing = false;
            yield break;
        }

        Transform top = botStack.GetChild(botStack.childCount - 1);
        if (top == null)
        {
            
            isProcessing = false;
            yield break;
        }

        cardbehavior cb = top.GetComponent<cardbehavior>();
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

            roundmanager.Instance.OnCardPlaced(cb);

            yield return null;
            

            bool isMatch = false;
            if (centerStack.childCount >= 2)
            {
                int last = centerStack.GetChild(centerStack.childCount - 1).GetComponent<carddealer>().cardValue;
                int prev = centerStack.GetChild(centerStack.childCount - 2).GetComponent<carddealer>().cardValue;
                if (last == prev) isMatch = true;
            }

            if (!isMatch && roundmanager.Instance.currentState == roundmanager.RoundState.Playing)
                turnmanager.Instance.SwitchTurn();
        }

        ProtectBotCards();
        

        yield return new WaitForSeconds(extraDelayAfterPlace);
        isProcessing = false;
    }

  

    public void ForceProtectBotCards()
    {
        ProtectBotCards();
        
    }

}
