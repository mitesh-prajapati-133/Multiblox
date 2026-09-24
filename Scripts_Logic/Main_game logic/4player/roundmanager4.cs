using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class roundmanager4 : MonoBehaviour
{
     public static roundmanager4 Instance;

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
    public RectTransform player4Stack;
    public GameObject player1OutText;
public GameObject player2OutText;
public GameObject player3OutText;
public GameObject player4OutText;

    [Header("Hand Layout (DO NOT CHANGE)")]
    public float tightSpacing = 28f;
    public Vector3 handScale = new Vector3(0.6f, 0.6f, 1f);

    [Header("Animation (DO NOT CHANGE)")]
    public float moveSpeed = 350f;
    public float delayBetweenCards = 0.15f;
    public float collectDelay = 0.8f;
public bool pendingFinalResponse4P = false;
    public Sprite backCard;
    public AudioSource collectAudio;

    private List<cardbehaviour4> centerCards = new List<cardbehaviour4>();

    void Awake()
    {
        Instance = this;
    }

    // =====================================================
    float GetX(RectTransform stack, int index)
    {
    float direction =
        (stack == player1Stack || stack == player2Stack) ? 1f : -1f;

    return direction * index * tightSpacing;
    }

    RectTransform GetStackByOwner(turnmanager4.Turn owner)
    {
        if (owner == turnmanager4.Turn.Player1) return player1Stack;
        if (owner == turnmanager4.Turn.Player2) return player2Stack;
        if (owner == turnmanager4.Turn.Player3) return player3Stack;
        return player4Stack;
    }

    // =====================================================
    // CARD PLACED
    // =====================================================
    public void OnCardPlaced(cardbehaviour4 card)
    {
          if (currentState != RoundState.Playing)
        return;

    centerCards.Add(card);

    // Less than 2 cards → normal rotation
    if (centerCards.Count < 2)
    {

    UpdateOutUI(); 
     turnmanager4.Instance.SetTurn(
        GetNextAliveTurn(turnmanager4.Instance.currentTurn)
    );
        return;
    }

    int last = centerCards[^1].GetComponent<carddealer4>().cardValue;
    int prev = centerCards[^2].GetComponent<carddealer4>().cardValue;

    // ================= SAME → COLLECT
    if (last == prev)
    {
        RectTransform winnerStack = GetStackByOwner(card.cardOwner);
        currentState = RoundState.Collecting;

        if (collectAudio && !collectAudio.isPlaying)
            collectAudio.Play();

        StartCoroutine(CollectCenterPile(winnerStack));
        return;
    }

    // ================= REDEAL CHECK (4‑player rule)
    if (CheckRedealCondition())
        return;

    // Normal turn rotation

    UpdateOutUI(); 
turnmanager4.Instance.SetTurn(
    GetNextAliveTurn(turnmanager4.Instance.currentTurn)
);

    }

    // =====================================================
    // COLLECT — ❗ WINNER ALWAYS GETS TURN ❗
    // =====================================================
    IEnumerator CollectCenterPile(RectTransform winnerStack)
    {
        turnmanager4.Instance.enabled = false;
        yield return new WaitForSeconds(collectDelay);

        List<cardbehaviour4> pile = new List<cardbehaviour4>(centerCards);
        centerCards.Clear();
        pile.Reverse();

        foreach (cardbehaviour4 card in pile)
        {
            RectTransform rect = card.GetComponent<RectTransform>();
            Image img = card.GetComponent<Image>();

            rect.sizeDelta = card.GetOriginalSize();
            rect.localRotation = Quaternion.identity;
            rect.SetParent(winnerStack, true);
            img.sprite = backCard;

            card.cardOwner =
                winnerStack == player1Stack ? turnmanager4.Turn.Player1 :
                winnerStack == player2Stack ? turnmanager4.Turn.Player2 :
                winnerStack == player3Stack ? turnmanager4.Turn.Player3 :
                turnmanager4.Turn.Player4;

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

        // 🔥 Winner plays again
        turnmanager4.Instance.SetTurn(
            winnerStack == player1Stack ? turnmanager4.Turn.Player1 :
            winnerStack == player2Stack ? turnmanager4.Turn.Player2 :
            winnerStack == player3Stack ? turnmanager4.Turn.Player3 :
            turnmanager4.Turn.Player4
        );

        if (collectAudio && collectAudio.isPlaying)
            collectAudio.Stop();
            ClearOutUI();

        turnmanager4.Instance.enabled = true;
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
    bool CheckRedealCondition()
{
    if (currentState != RoundState.Playing)
        return false;

    bool p1Empty = player1Stack.childCount == 0;
    bool p2Empty = player2Stack.childCount == 0;
    bool p3Empty = player3Stack.childCount == 0;
    bool p4Empty = player4Stack.childCount == 0;

    int emptyCount = 0;
    if (p1Empty) emptyCount++;
    if (p2Empty) emptyCount++;
    if (p3Empty) emptyCount++;
    if (p4Empty) emptyCount++;
 if (emptyCount == 3)
    {
        // FIRST TIME → allow final stick
        if (!pendingFinalResponse4P)
        {
            pendingFinalResponse4P = true;
                turnmanager4.Instance.SetTurn(
        GetNextAliveTurn(turnmanager4.Instance.currentTurn)
    );
            return true; // stop normal switch
        }

        // SECOND TIME → now redeal
        pendingFinalResponse4P = false;
        currentState = RoundState.Redealing;
        centerCards.Clear();
        redealmanager4.Instance.ForceRedealFromCenter();
        return true;
    }

    // ALL EMPTY
    if (emptyCount == 4)
    {
        currentState = RoundState.Redealing;
        centerCards.Clear();
        redealmanager4.Instance.ForceRedealFromCenter();
        return true;
    }

    return false;
 
}
public turnmanager4.Turn GetNextAliveTurn(turnmanager4.Turn current)
{
    for (int i = 0; i < 4; i++)
    {
        current =
            current == turnmanager4.Turn.Player1 ? turnmanager4.Turn.Player2 :
            current == turnmanager4.Turn.Player2 ? turnmanager4.Turn.Player3 :
            current == turnmanager4.Turn.Player3 ? turnmanager4.Turn.Player4 :
                                                   turnmanager4.Turn.Player1;

        if (current == turnmanager4.Turn.Player1 && player1Stack.childCount > 0)
            return current;

        if (current == turnmanager4.Turn.Player2 && player2Stack.childCount > 0)
            return current;

        if (current == turnmanager4.Turn.Player3 && player3Stack.childCount > 0)
            return current;

        if (current == turnmanager4.Turn.Player4 && player4Stack.childCount > 0)
            return current;
    }

    return current; // fallback (should never happen)
}
public void UpdateOutUI()
{
    if (player1OutText)
        player1OutText.SetActive(player1Stack.childCount == 0);

    if (player2OutText)
        player2OutText.SetActive(player2Stack.childCount == 0);

    if (player3OutText)
        player3OutText.SetActive(player3Stack.childCount == 0);

    if (player4OutText)
        player4OutText.SetActive(player4Stack.childCount == 0);
}

 public void ClearOutUI()
{
    if (player1OutText) player1OutText.SetActive(false);
    if (player2OutText) player2OutText.SetActive(false);
    if (player3OutText) player3OutText.SetActive(false);
    if (player4OutText) player4OutText.SetActive(false);
}

}
