using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
public class redeal : MonoBehaviourPun
{
    public static redeal Instance;

    public RectTransform centerStack;
    public RectTransform player1Stack;
    public RectTransform player2Stack;

    public float moveSpeed = 1200f;
    public float scaleSpeed = 12f;
    public float cardOffset = 6f;
    public Vector3 handScale = new Vector3(0.6f, 0.6f, 1f);
    public float redealDelay = 0.5f;

    List<RectTransform> cards = new List<RectTransform>();
    RectTransform activeCard;
    RectTransform targetStack;

    Vector3 targetWorldPos;
    Vector3 targetScale;

    int currentIndex = 0;
    bool isRedealing = false;
    bool shuffleReceived = false;
    bool joinerReady = false;
    int[] pendingShuffleOrder = null;

    Dictionary<RectTransform, int> cardOriginalIndex = new Dictionary<RectTransform, int>();

    static int redealGeneration = 0;
    const int INDEX_OFFSET = 1000;

    void Awake() { Instance = this; }

    float GetX(RectTransform stack, int index)
    {
        return stack == player1Stack
            ? index * cardOffset
            : -index * cardOffset;
    }

    public void TryRedeal()
    {
        if (isRedealing) return;
        if (round.Instance.currentState != round.RoundState.Playing) return;

        if (player1Stack.childCount == 0 &&
            player2Stack.childCount == 0 &&
            centerStack.childCount > 0)
        {
            StartCoroutine(StartRedeal());
        }
    }

    public void ForceRedealFromCenter()
    {
        if (!isRedealing)
            StartCoroutine(StartRedeal());
    }

    IEnumerator StartRedeal()
    {
        shuffleReceived = false;
        joinerReady = false;
        pendingShuffleOrder = null;
        isRedealing = true;
        turndown.Instance.enabled = false;
    GameNetworkManager.ResetReady();
        round.Instance.currentState = round.RoundState.Redealing;
        round.Instance.ResetCenter();

        // ✅ FIX 1: Only MasterClient waits redealDelay here
        // Joiner skips this and takes the delay AFTER shuffle received
        // so both clients start animation at the same real time
        if (PhotonNetwork.IsMasterClient)
            yield return new WaitForSeconds(redealDelay);

        cards.Clear();
        cardOriginalIndex.Clear();

        redealGeneration++;
        int baseIndex = redealGeneration * INDEX_OFFSET;

        int idx = 0;
        while (centerStack.childCount > 0)
        {
            RectTransform card = centerStack.GetChild(0) as RectTransform;
            cards.Add(card);
            cardOriginalIndex[card] = baseIndex + idx;
            idx++;
            card.SetParent(centerStack.parent, true);
        }

        GameNetworkManager.currentShuffleTarget = GameNetworkManager.ShuffleTarget.Redeal;

        if (PhotonNetwork.IsMasterClient)
        {
            Shuffle(cards);

            List<int> order = new List<int>();
            foreach (var c in cards)
                order.Add(cardOriginalIndex[c]);

            // ✅ FIX 2: Send shuffle first, then wait small buffer
            // so joiner receives shuffle before host moves first card
            GameNetworkManager.Instance.SendShuffle(order);

            yield return new WaitForSeconds(0.1f);

            currentIndex = 0;
            PrepareNext();
        }
        else
        {
            // Joiner: mark ready THEN check if shuffle already arrived early
            joinerReady = true;

            if (pendingShuffleOrder != null)
            {
                ApplyShuffleInternal(pendingShuffleOrder);
                pendingShuffleOrder = null;
            }

            StartCoroutine(WaitForShuffleThenStart());
        }
    }

    IEnumerator WaitForShuffleThenStart()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (!shuffleReceived)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogWarning("Redeal shuffle timeout — using fallback order");
                shuffleReceived = true;
                break;
            }
            yield return null;
        }

        // ✅ FIX 3: After shuffle received, joiner waits redealDelay
        // This matches the total time host waited before starting animation
        // so both clients begin PrepareNext() at the same moment
        yield return new WaitForSeconds(redealDelay);

        currentIndex = 0;
        PrepareNext();
    }

    void ApplyShuffleInternal(int[] order)
    {
        Dictionary<int, RectTransform> byOriginal = new Dictionary<int, RectTransform>();
        foreach (var kvp in cardOriginalIndex)
            byOriginal[kvp.Value] = kvp.Key;

        List<RectTransform> newList = new List<RectTransform>();
        foreach (int i in order)
        {
            if (byOriginal.ContainsKey(i))
                newList.Add(byOriginal[i]);
        }

        if (newList.Count > 0)
            cards = newList;

        shuffleReceived = true;
    }

    public void ApplyNetworkShuffle(int[] order)
    {
        if (!joinerReady)
        {
            pendingShuffleOrder = order;
            return;
        }

        ApplyShuffleInternal(order);
    }

    void Update()
    {
        if (!isRedealing || activeCard == null) return;

        activeCard.position = Vector3.MoveTowards(
            activeCard.position,
            targetWorldPos,
            moveSpeed * Time.deltaTime
        );

        activeCard.localScale = Vector3.Lerp(
            activeCard.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );

        if (Vector3.Distance(activeCard.position, targetWorldPos) < 0.5f)
        {
            FinishCard();
            PrepareNext();
        }
    }

    void PrepareNext()
    {
        if (currentIndex >= cards.Count)
        {
            EndRedeal();
            return;
        }

        activeCard = cards[currentIndex];

        behaviour cb = activeCard.GetComponent<behaviour>();
        Image img = activeCard.GetComponent<Image>();

        activeCard.sizeDelta = cb.GetOriginalSize();
        img.sprite = round.Instance.backCard;

        targetStack = currentIndex % 2 == 0 ? player1Stack : player2Stack;

        int index = targetStack.childCount;
        Vector2 localPos = new Vector2(GetX(targetStack, index), 0);

        targetWorldPos = targetStack.TransformPoint(localPos);
        targetScale = handScale;

        activeCard.SetParent(targetStack.parent, true);
        currentIndex++;
    }

    void FinishCard()
    {
        activeCard.SetParent(targetStack, false);

        behaviour cb = activeCard.GetComponent<behaviour>();
        cb.cardOwner =
            targetStack == player1Stack
                ? turndown.Turn.Player1
                : turndown.Turn.Player2;

        cb.cardIndex = cardOriginalIndex.ContainsKey(activeCard)
            ? cardOriginalIndex[activeCard]
            : -1;

        int index = targetStack.childCount - 1;
        activeCard.anchoredPosition = new Vector2(GetX(targetStack, index), 0);
        activeCard.localScale = handScale;

        activeCard = null;
    }

    void EndRedeal()
    {
        isRedealing = false;

        round.Instance.ArrangeHand(player1Stack);
        round.Instance.ArrangeHand(player2Stack);

        round.Instance.currentState = round.RoundState.Playing;

       

        round.Instance.StopDistribution();

        GameNetworkManager.currentShuffleTarget = GameNetworkManager.ShuffleTarget.Distributor;
           GameNetworkManager.Instance.SendReady();

    // ✅ NEW: only enable turndown when BOTH sides finished redeal
    StartCoroutine(WaitBothReadyThenEnableTurn());
    }
    IEnumerator WaitBothReadyThenEnableTurn()
{
   float timeout = 5f;
    float elapsed = 0f;

    while (!GameNetworkManager.bothSidesReady)
    {
        elapsed += Time.deltaTime;

        // ✅ If opponent disconnected — force unlock after timeout
        if (elapsed >= timeout)
        {
            GameNetworkManager.remoteAnimationDone = true; // force unlock
            break;
        }
        yield return null;
    }

    turndown.Instance.enabled = true;
}

    void Shuffle(List<RectTransform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
