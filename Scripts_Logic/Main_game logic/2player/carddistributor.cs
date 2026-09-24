using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
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
         IsDistributing = true; 
        turnmanager.Instance.enabled = false;

        cards.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform card = transform.GetChild(i) as RectTransform;
            card.localScale = centerScale;
            cards.Add(card);
        }

        Shuffle(cards);
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
            turnmanager.Instance.enabled = true;
            IsDistributing = false;

            if (distributionAudio != null && distributionAudio.isPlaying)
                distributionAudio.Stop();

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

        cardbehavior cb = activeCard.GetComponent<cardbehavior>();
        cb.cardOwner =
            targetStack == player1Stack
                ? turnmanager.Turn.Player1
                : turnmanager.Turn.Player2;

        activeCard.localScale = handScale;
           cb.cardIndex = cards.IndexOf(activeCard);
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
