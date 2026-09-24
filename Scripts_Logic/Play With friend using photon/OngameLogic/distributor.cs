using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;


public class distributor : MonoBehaviourPun

{
   public static distributor Instance;
 
    public RectTransform player1Stack;
    public RectTransform player2Stack;
 
    public float startDelay = 2f;
    public float moveSpeed = 1200f;
    public float scaleSpeed = 12f;
    public float cardOffset = 40f;
    public static bool IsDistributing = false;
 
    public Vector3 centerScale = Vector3.one;
    public Vector3 handScale = new Vector3(0.6f, 0.6f, 1f);
 
    [Header("Audio")]
    public AudioSource distributionAudio;
 
    List<RectTransform> cards = new List<RectTransform>();
 
    // ── Store original indices BEFORE any reparenting ──
    Dictionary<RectTransform, int> cardOriginalIndex = new Dictionary<RectTransform, int>();
 
    int currentIndex = 0;
    float timer = 0f;
    bool startDeal = false;
    bool audioStarted = false;
 
    RectTransform activeCard;
    Vector3 targetPos;
    Vector3 targetScale;
    RectTransform targetStack;
 
    void Start()
    {
        Instance = this;
        IsDistributing = true;
        turndown.Instance.enabled = false;
     GameNetworkManager.ResetReady();
        cards.Clear();
        cardOriginalIndex.Clear();
 
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform card = transform.GetChild(i) as RectTransform;
            card.localScale = centerScale;
            cards.Add(card);
            cardOriginalIndex[card] = i; // ✅ Store original index now, before any shuffle/reparent
        }
 
        if (PhotonNetwork.IsMasterClient)
        {
            Shuffle(cards);
 
            List<int> order = new List<int>();
            foreach (var c in cards)
                order.Add(cardOriginalIndex[c]); // ✅ Use stored original index
 
            GameNetworkManager.Instance.SendShuffle(order);
        }
        else
        {
            // Wait for network shuffle
        }
    }
 
    public void ApplyNetworkShuffle(int[] order)
    {
        // Build a lookup: originalIndex -> RectTransform
        Dictionary<int, RectTransform> byOriginal = new Dictionary<int, RectTransform>();
        foreach (var kvp in cardOriginalIndex)
            byOriginal[kvp.Value] = kvp.Key;
 
        List<RectTransform> newList = new List<RectTransform>();
        foreach (int i in order)
        {
            if (byOriginal.ContainsKey(i))
                newList.Add(byOriginal[i]);
        }
 
        cards = newList;
    }
 
    void Update()
    {
        timer += Time.deltaTime;
 
        if (!startDeal)
        {
            if (timer >= startDelay)
            {
                startDeal = true;
 
                if (distributionAudio != null && !audioStarted)
                {
                    distributionAudio.Play();
                    audioStarted = true;
                }
 
                PrepareNext();
            }
            return;
        }
 
        if (activeCard == null) return;
 
        activeCard.position = Vector3.MoveTowards(
            activeCard.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
 
        activeCard.localScale = Vector3.Lerp(
            activeCard.localScale,
            targetScale,
            Time.deltaTime * scaleSpeed
        );
 
        if (Vector3.Distance(activeCard.position, targetPos) < 0.5f)
        {
            FinishCard();
            PrepareNext();
        }
    }
 
    void PrepareNext()
    {
        if (currentIndex >= cards.Count)
        {
            activeCard = null;
            cards.Clear();
            enabled = false;

            IsDistributing = false;
 
            if (distributionAudio != null && distributionAudio.isPlaying)
                distributionAudio.Stop();
     GameNetworkManager.Instance.SendReady();

        // ✅ NEW: only enable turndown when BOTH sides are ready
        StartCoroutine(WaitBothReadyThenEnableTurn());
            return;
        }
 
        activeCard = cards[currentIndex];
        targetStack = currentIndex % 2 == 0 ? player1Stack : player2Stack;
 
        int index = targetStack.childCount;
        float direction = targetStack == player1Stack ? 1f : -1f;
 
        targetPos = targetStack.TransformPoint(
            new Vector2(direction * index * cardOffset, 0)
        );
 
        targetScale = handScale;
        activeCard.SetParent(targetStack.parent, true);
        currentIndex++;
    }
 
    void FinishCard()
    {
        activeCard.SetParent(targetStack, false);
 
        float direction = targetStack == player1Stack ? 1f : -1f;
        activeCard.anchoredPosition =
            new Vector2(direction * (targetStack.childCount - 1) * cardOffset, 0);
 
        behaviour cb = activeCard.GetComponent<behaviour>();
        cb.cardOwner =
            targetStack == player1Stack
                ? turndown.Turn.Player1
                : turndown.Turn.Player2;
 
        activeCard.localScale = handScale;
 
        // ✅ Use the pre-stored original index — never IndexOf after reparenting
        cb.cardIndex = cardOriginalIndex.ContainsKey(activeCard)
            ? cardOriginalIndex[activeCard]
            : -1;
    }
 IEnumerator WaitBothReadyThenEnableTurn()
{
    float timeout = 5f;
    float elapsed = 0f;

    while (!GameNetworkManager.bothSidesReady)
    {
        elapsed += Time.deltaTime;

        // ✅ If opponent disconnected — dont wait forever, unlock after timeout
        if (elapsed >= timeout)
        {
            GameNetworkManager.remoteAnimationDone = true; // force unlock
            break;
        }
        yield return null;
    }

    turndown.Instance.enabled = true;
}
    protected virtual void Shuffle(List<RectTransform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
