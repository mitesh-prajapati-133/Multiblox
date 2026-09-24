using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class redealamanager : MonoBehaviour
{
  public static redealamanager Instance;

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

    void Awake()
    {
        Instance = this;
    }

    // ================= POSITION RULE =================
    float GetX(RectTransform stack, int index)
    {
        return stack == player1Stack
            ? index * cardOffset     // Player 1 → RIGHT
            : -index * cardOffset;   // Player 2 → LEFT
    }

    public void TryRedeal()
    {
        if (isRedealing) return;
        if (roundmanager.Instance.currentState != roundmanager.RoundState.Playing)
            return;

        if (player1Stack.childCount == 0 &&
            player2Stack.childCount == 0 &&
            centerStack.childCount > 0)
        {
            StartCoroutine(StartRedeal());
        }
    }

    IEnumerator StartRedeal()
    {
        isRedealing = true;
        turnmanager.Instance.enabled = false;

        roundmanager.Instance.currentState = roundmanager.RoundState.Redealing;
        roundmanager.Instance.ResetCenter();

        yield return new WaitForSeconds(redealDelay);

        cards.Clear();

        while (centerStack.childCount > 0)
        {
            RectTransform card = centerStack.GetChild(0) as RectTransform;
            cards.Add(card);
            card.SetParent(centerStack.parent, true);
        }
   // SYNC SHUFFLE SEED ✅
  

        Shuffle(cards);
        currentIndex = 0;
        PrepareNext();
    }

    void Update()
    {
        if (!isRedealing || activeCard == null)
            return;

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

        cardbehavior cb = activeCard.GetComponent<cardbehavior>();
        Image img = activeCard.GetComponent<Image>();

        activeCard.sizeDelta = cb.GetOriginalSize();
        img.sprite = roundmanager.Instance.backCard;

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

        cardbehavior cb = activeCard.GetComponent<cardbehavior>();
        cb.cardOwner =
            targetStack == player1Stack
                ? turnmanager.Turn.Player1
                : turnmanager.Turn.Player2;

        int index = targetStack.childCount - 1;
        activeCard.anchoredPosition = new Vector2(GetX(targetStack, index), 0);
        activeCard.localScale = handScale;

        activeCard = null;
    }

    void EndRedeal()
    {
        isRedealing = false;

    // ✅ FORCE FINAL HAND ARRANGEMENT
    roundmanager.Instance.ArrangeHand(player1Stack);
    roundmanager.Instance.ArrangeHand(player2Stack);

    // Reset state back to Playing
    roundmanager.Instance.currentState = roundmanager.RoundState.Playing;



    turnmanager.Instance.enabled = true;

    // 🔊 stop sound
    roundmanager.Instance.StopDistribution();

    }

    void Shuffle(List<RectTransform> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public void ForceRedealFromCenter()
    {
        if (!isRedealing)
            StartCoroutine(StartRedeal());
    }
}
