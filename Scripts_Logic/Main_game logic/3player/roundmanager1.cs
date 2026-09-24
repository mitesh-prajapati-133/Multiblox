using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class roundmanager1 : MonoBehaviour
{ 
  public static roundmanager1 Instance;

    public enum RoundState
    {
        Playing,
        Collecting,
        Redealing
    }

    public RoundState currentState = RoundState.Playing;

    [Header("Stacks")]
    public RectTransform centerStack;
    public RectTransform player1Stack;
    public RectTransform player2Stack;
    public RectTransform player3Stack;

    [Header("Hand Layout (DO NOT CHANGE)")]
    public float tightSpacing = 28f;
    public Vector3 handScale = new Vector3(0.6f, 0.6f, 1f);

    [Header("Animation (DO NOT CHANGE)")]
    public float moveSpeed = 350f;
    public float delayBetweenCards = 0.15f;
    public float collectDelay = 0.8f;
    // ================= FINAL RESPONSE (3 PLAYER) =================
public bool pendingFinalResponse3P = false;
turnmanager1.Turn soleAlivePlayer;
[Header("OUT UI")]
public GameObject player1OutText;
public GameObject player2OutText;
public GameObject player3OutText;

    public Sprite backCard;
    public AudioSource collectAudio;

    private List<cardbeaviour1> centerCards = new List<cardbeaviour1>();

    void Awake()
    {
        Instance = this;
    }

    // =====================================================
    float GetX(RectTransform stack, int index)
    {
        return stack == player1Stack ? index * tightSpacing : -index * tightSpacing;
    }

    RectTransform GetStackByOwner(turnmanager1.Turn owner)
    {
        if (owner == turnmanager1.Turn.Player1) return player1Stack;
        if (owner == turnmanager1.Turn.Player2) return player2Stack;
        return player3Stack;
    }

    // =====================================================
    // 🔑 SKIP ONLY IF CURRENT PLAYER IS EMPTY (NORMAL TURN)
    // =====================================================
    turnmanager1.Turn GetNextTurnSkippingEmpty(turnmanager1.Turn current)
    {
        for (int i = 0; i < 3; i++)
        {
            current =
                current == turnmanager1.Turn.Player1 ? turnmanager1.Turn.Player2 :
                current == turnmanager1.Turn.Player2 ? turnmanager1.Turn.Player3 :
                turnmanager1.Turn.Player1;

            if (current == turnmanager1.Turn.Player1 && player1Stack.childCount > 0)
                return current;

            if (current == turnmanager1.Turn.Player2 && player2Stack.childCount > 0)
                return current;

            if (current == turnmanager1.Turn.Player3 && player3Stack.childCount > 0)
                return current;
        }

        return current; // fallback
    }

    // =====================================================
    // CARD PLACED
    // =====================================================
    public void OnCardPlaced(cardbeaviour1 card)
    {
        if (currentState != RoundState.Playing)
            return;

        centerCards.Add(card);

        // Less than 2 cards → normal rotation with skip
        if (centerCards.Count < 2)
        {
            UpdateOutUI();
          
            turnmanager1.Instance.SetTurn(
                GetNextTurnSkippingEmpty(turnmanager1.Instance.currentTurn)
            );
            return;
        }

        int last = centerCards[^1].GetComponent<cardealer1>().cardValue;
        int prev = centerCards[^2].GetComponent<cardealer1>().cardValue;

        // ================= SAME → COLLECT (OLD LOGIC 🔒)
        if (last == prev)
        {
              pendingFinalResponse3P = false; 
            RectTransform winnerStack = GetStackByOwner(card.cardOwner);
            currentState = RoundState.Collecting;

            if (collectAudio && !collectAudio.isPlaying)
                collectAudio.Play();

            StartCoroutine(CollectCenterPile(winnerStack));
            return;
        }
if (HandleTwoPlayersEmptyFinalResponse())
    return; 
        // ================= REDEAL =================
        if (CheckAllPlayersEmpty())
            return;
        UpdateOutUI();
        // Normal turn → skip empty players only here
        turnmanager1.Instance.SetTurn(
            GetNextTurnSkippingEmpty(turnmanager1.Instance.currentTurn)
        );
    }

    // =====================================================
    bool CheckAllPlayersEmpty()
    {
        if (currentState != RoundState.Playing)
            return false;

        if (player1Stack.childCount == 0 &&
            player2Stack.childCount == 0 &&
            player3Stack.childCount == 0 &&
            centerStack.childCount > 0)
        {
            currentState = RoundState.Redealing;
            ClearOutUI();
            centerCards.Clear();
            redealmanager1.Instance.ForceRedealFromCenter();
            return true;
        }

        return false;
    }
    bool HandleTwoPlayersEmptyFinalResponse()
{
    if (currentState != RoundState.Playing)
        return false;

    bool p1Empty = player1Stack.childCount == 0;
    bool p2Empty = player2Stack.childCount == 0;
    bool p3Empty = player3Stack.childCount == 0;

    int emptyCount = 0;
    if (p1Empty) emptyCount++;
    if (p2Empty) emptyCount++;
    if (p3Empty) emptyCount++;

    // Only care about EXACTLY TWO EMPTY
    if (emptyCount != 2)
        return false;

    // Identify the only alive player
    if (!p1Empty) soleAlivePlayer = turnmanager1.Turn.Player1;
    else if (!p2Empty) soleAlivePlayer = turnmanager1.Turn.Player2;
    else soleAlivePlayer = turnmanager1.Turn.Player3;

    // FIRST TIME → allow final response
    if (!pendingFinalResponse3P)
    {
        pendingFinalResponse3P = true;

        // force turn to the alive player (do NOT skip, do NOT redeal)
        turnmanager1.Instance.SetTurn(soleAlivePlayer);
        return true;
    }

    // SECOND TIME → response used → redeal
    pendingFinalResponse3P = false;
    currentState = RoundState.Redealing;
    ClearOutUI();
    centerCards.Clear();
    redealmanager1.Instance.ForceRedealFromCenter();
    return true;
} 
void UpdateOutUI()
{
    if (player1OutText)
        player1OutText.SetActive(player1Stack.childCount == 0);

    if (player2OutText)
        player2OutText.SetActive(player2Stack.childCount == 0);

    if (player3OutText)
        player3OutText.SetActive(player3Stack.childCount == 0);
}

void ClearOutUI()
{
    if (player1OutText) player1OutText.SetActive(false);
    if (player2OutText) player2OutText.SetActive(false);
    if (player3OutText) player3OutText.SetActive(false);
}

    // =====================================================
    // COLLECT — ❗ WINNER ALWAYS GETS TURN ❗
    // =====================================================
    IEnumerator CollectCenterPile(RectTransform winnerStack)
    {
        turnmanager1.Instance.enabled = false;
        yield return new WaitForSeconds(collectDelay);

        List<cardbeaviour1> pile = new List<cardbeaviour1>(centerCards);
        centerCards.Clear();
        pile.Reverse();

        foreach (cardbeaviour1 card in pile)
        {
            RectTransform rect = card.GetComponent<RectTransform>();
            Image img = card.GetComponent<Image>();

            rect.sizeDelta = card.GetOriginalSize();
            rect.localRotation = Quaternion.identity;
            rect.SetParent(winnerStack, true);
            img.sprite = backCard;

            card.cardOwner =
                winnerStack == player1Stack ? turnmanager1.Turn.Player1 :
                winnerStack == player2Stack ? turnmanager1.Turn.Player2 :
                turnmanager1.Turn.Player3;

            int index = winnerStack.childCount - 1;
            Vector2 targetPos = new Vector2(GetX(winnerStack, index), 0);

            while (Vector2.Distance(rect.anchoredPosition, targetPos) > 1f)
            {
                rect.anchoredPosition = Vector2.MoveTowards(
                    rect.anchoredPosition,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

                rect.localScale = Vector3.Lerp(
                    rect.localScale,
                    handScale,
                    Time.deltaTime * 8f
                );

                yield return null;
            }

            rect.anchoredPosition = targetPos;
            rect.localScale = handScale;
            yield return new WaitForSeconds(delayBetweenCards);
        }

        ArrangeHand(winnerStack);

        // 🔥 OLD RULE RESTORED — WINNER PLAYS AGAIN
        turnmanager1.Instance.SetTurn(
            winnerStack == player1Stack ? turnmanager1.Turn.Player1 :
            winnerStack == player2Stack ? turnmanager1.Turn.Player2 :
            turnmanager1.Turn.Player3
        );
        if (collectAudio && collectAudio.isPlaying)
    collectAudio.Stop();
        ClearOutUI();   
        turnmanager1.Instance.enabled = true;
        currentState = RoundState.Playing;
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
    public turnmanager1.Turn GetNextAliveTurn(turnmanager1.Turn current)
{
    // STRICT round-robin skip only if player has NO cards
    for (int i = 0; i < 3; i++)
    {
        current =
            current == turnmanager1.Turn.Player1 ? turnmanager1.Turn.Player2 :
            current == turnmanager1.Turn.Player2 ? turnmanager1.Turn.Player3 :
            turnmanager1.Turn.Player1;

        if (current == turnmanager1.Turn.Player1 && player1Stack.childCount > 0)
            return current;

        if (current == turnmanager1.Turn.Player2 && player2Stack.childCount > 0)
            return current;

        if (current == turnmanager1.Turn.Player3 && player3Stack.childCount > 0)
            return current;
    }

    return current; // fallback
}
}

