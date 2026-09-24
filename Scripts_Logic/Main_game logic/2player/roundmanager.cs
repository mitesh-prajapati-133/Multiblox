using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class roundmanager : MonoBehaviour
{ 

    public static roundmanager Instance;

    public enum RoundState
    {
        Playing,
        Collecting,
        Redealing
    }

    public RoundState currentState = RoundState.Playing;

    public RectTransform centerStack;
    public RectTransform player1Stack;
    public RectTransform player2Stack;
    public bool player1HasStuck = false;
public bool player2HasStuck = false;


    [Header("Hand Layout")]
    public float tightSpacing = 28f;
    public Vector3 handScale = new Vector3(0.6f, 0.6f, 1f);

    [Header("Animation (DO NOT CHANGE)")]
    public float moveSpeed = 350f;
    public float delayBetweenCards = 0.15f;

    [Header("Delays")]
    public float warStartDelay = 1f;
    public float emptyPlayerDelay = 0.7f;

    public Sprite backCard;
    public AudioSource distributionAudio;

    // ================= INTERNAL =================
    private List<cardbehavior> centerCards = new List<cardbehavior>();
    private bool emptyCheckRunning = false;

    // FINAL RESPONSE
    private bool pendingFinalResponse = false;
    private turnmanager.Turn emptyPlayer;
     public void PlayDistribution()
{
    if (distributionAudio != null && !distributionAudio.isPlaying)
        distributionAudio.Play();
}

public void StopDistribution()
{
    if (distributionAudio != null && distributionAudio.isPlaying)
        distributionAudio.Stop();
}

    void Awake()
    {
        Instance = this;
    }

    // ================= POSITION RULE =================
    float GetX(RectTransform stack, int index)
    {
        return stack == player1Stack
            ? index * tightSpacing     // Player 1 → RIGHT
            : -index * tightSpacing;   // Player 2 → LEFT
    }

    // =====================================================
    public void ResetCenter()
    {
        centerCards.Clear();
            player1HasStuck = false;
    player2HasStuck = false;

    }

    // =====================================================
    public void OnCardPlaced(cardbehavior card)
    {
        
        if (currentState != RoundState.Playing)
            return;

        centerCards.Add(card);
        if (card.cardOwner == turnmanager.Turn.Player1)
    player1HasStuck = true;
else
    player2HasStuck = true;


        // 🥇 MATCH RULE
        if (centerCards.Count >= 2)
        {
            int last = centerCards[^1].GetComponent<carddealer>().cardValue;
            int prev = centerCards[^2].GetComponent<carddealer>().cardValue;

            if (last == prev)
            {
                PlayDistribution(); // 🔊 START SOUND
                RectTransform winnerStack =
                    card.cardOwner == turnmanager.Turn.Player1
                        ? player1Stack
                        : player2Stack;

                pendingFinalResponse = false;
                currentState = RoundState.Collecting;

                StartCoroutine(DelayedCollect(winnerStack));            
                return;
            }
        }

        bool p1Empty = player1Stack.childCount == 0;
        bool p2Empty = player2Stack.childCount == 0;

        // 🥈 FINAL RESPONSE
        if (!pendingFinalResponse && (p1Empty ^ p2Empty))
        {
            pendingFinalResponse = true;
            emptyPlayer = p1Empty
                ? turnmanager.Turn.Player1
                : turnmanager.Turn.Player2;
            return;
        }

        if (pendingFinalResponse)
        {
            pendingFinalResponse = false;
            StartCoroutine(DelayedRedealAfterFinal());
            return;
        }

        CheckBothPlayersEmpty();
        CheckEmptyPlayerRule();
    }

    // =====================================================
    IEnumerator DelayedRedealAfterFinal()
    {
        yield return new WaitForSeconds(emptyPlayerDelay);

        currentState = RoundState.Redealing;
        ResetCenter();
        PlayDistribution();
        redealamanager.Instance.ForceRedealFromCenter();
    }

    // =====================================================
    void CheckEmptyPlayerRule()
    {
        if (pendingFinalResponse || emptyCheckRunning)
            return;

        bool p1Empty = player1Stack.childCount == 0;
        bool p2Empty = player2Stack.childCount == 0;

        if (p1Empty ^ p2Empty)
        {
            emptyCheckRunning = true;
            StartCoroutine(HandleEmptyPlayer());
        }
    }

    // =====================================================
    IEnumerator HandleEmptyPlayer()
    {
        yield return new WaitForSeconds(emptyPlayerDelay);

        if (currentState != RoundState.Playing)
        {
            emptyCheckRunning = false;
            yield break;
        }

        int centerCount = centerStack.childCount;
        turnmanager.Instance.enabled = false;

        if (centerCount >= 2)
        {
            currentState = RoundState.Redealing;
            ResetCenter();
            PlayDistribution();
            redealamanager.Instance.ForceRedealFromCenter();
        }
       

        turnmanager.Instance.enabled = true;
        emptyCheckRunning = false;
    }

    // =====================================================
    IEnumerator DelayedCollect(RectTransform winnerStack)
    {
        yield return new WaitForSeconds(warStartDelay);
        yield return StartCoroutine(CollectCenterPile(winnerStack));
        StopDistribution(); 
        currentState = RoundState.Playing;
    }

    // =====================================================
    IEnumerator CollectCenterPile(RectTransform winnerStack)
    {
        turnmanager.Instance.enabled = false;

        List<cardbehavior> pile = new List<cardbehavior>(centerCards);
        ResetCenter();
        pile.Reverse();

        foreach (cardbehavior card in pile)
        {
            RectTransform rect = card.GetComponent<RectTransform>();
            Image img = card.GetComponent<Image>();

            Vector2 startPos = rect.anchoredPosition;
            Vector3 startScale = Vector3.one * 0.85f;

            rect.sizeDelta = card.GetOriginalSize();
            rect.localRotation = Quaternion.identity;
            rect.SetParent(winnerStack, true);
            img.sprite = backCard;

            card.cardOwner =
                winnerStack == player1Stack
                    ? turnmanager.Turn.Player1
                    : turnmanager.Turn.Player2;

            int index = winnerStack.childCount - 1;
            Vector2 targetPos = new Vector2(GetX(winnerStack, index), 0);

            float totalDist = Vector2.Distance(startPos, targetPos);

            while (Vector2.Distance(rect.anchoredPosition, targetPos) > 1f)
            {
                rect.anchoredPosition = Vector2.MoveTowards(
                    rect.anchoredPosition,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

                float t = totalDist == 0 ? 1f :
                    1f - (Vector2.Distance(rect.anchoredPosition, targetPos) / totalDist);

                rect.localScale = Vector3.Lerp(
                    startScale,
                    handScale,
                    Mathf.SmoothStep(0f, 1f, t)
                );

                yield return null;
            }

            rect.anchoredPosition = targetPos;
            rect.localScale = handScale;
            yield return new WaitForSeconds(delayBetweenCards);
        }

        ArrangeHand(winnerStack);

        turnmanager.Instance.SetTurn(
            winnerStack == player1Stack
                ? turnmanager.Turn.Player1
                : turnmanager.Turn.Player2
        );


        turnmanager.Instance.enabled = true;
    }

    // =====================================================
    public void ArrangeHand(RectTransform hand)
    {
        for (int i = 0; i < hand.childCount; i++)
        {
            RectTransform card = hand.GetChild(i) as RectTransform;
            card.localScale = handScale;
            card.anchoredPosition = new Vector2(GetX(hand, i), 0);
        }
    }

    // =====================================================
    void CheckBothPlayersEmpty()
    {
        if (pendingFinalResponse || currentState != RoundState.Playing)
            return;

        if (player1Stack.childCount == 0 &&
            player2Stack.childCount == 0 &&
            centerStack.childCount > 0)
        {
            currentState = RoundState.Redealing;
            ResetCenter();
            PlayDistribution();
            redealamanager.Instance.ForceRedealFromCenter();
        }
    }

}

