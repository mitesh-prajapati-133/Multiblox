using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript1 : MonoBehaviour
{
     public RectTransform player1Stack;
    public RectTransform player2Stack;
    public RectTransform player3Stack;

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
        IsDistributing = true; // 🔒 BLOCK DRAGGING
        cards.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform card = transform.GetChild(i) as RectTransform;
            card.localScale = centerScale;
            cards.Add(card);
        }

        // SHUFFLE (UNCHANGED)
        for (int i = 0; i < cards.Count; i++)
        {
            int r = Random.Range(i, cards.Count);
            (cards[i], cards[r]) = (cards[r], cards[i]);
        }
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

            return;
        }

        activeCard = cards[currentIndex];

        // 3-PLAYER ROUND ROBIN (UNCHANGED)
        int mod = currentIndex % 3;
        if (mod == 0) targetStack = player1Stack;
        else if (mod == 1) targetStack = player2Stack;
        else targetStack = player3Stack;

        int index = targetStack.childCount;

        float direction =
            targetStack == player1Stack ? 1f :
            targetStack == player2Stack ? -1f :
            -1f;

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

        float direction =
            targetStack == player1Stack ? 1f :
            targetStack == player2Stack ? -1f :
            -1f;

        activeCard.anchoredPosition =
            new Vector2(direction * (targetStack.childCount - 1) * cardOffset, 0);
               cardbeaviour1 cb = activeCard.GetComponent<cardbeaviour1>();

    if (targetStack == player1Stack)
        cb.cardOwner = turnmanager1.Turn.Player1;
    else if (targetStack == player2Stack)
        cb.cardOwner = turnmanager1.Turn.Player2;
    else
        cb.cardOwner = turnmanager1.Turn.Player3;

        activeCard.localScale = handScale;
    }
}
