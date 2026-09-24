using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class redealmanager1 : MonoBehaviour
{
 public static redealmanager1 Instance;

    [Header("Stacks")]
    public RectTransform centerStack;
    public RectTransform player1Stack;
    public RectTransform player2Stack;
    public RectTransform player3Stack;

    [Header("Animation")]
    public float moveSpeed = 1200f;
    public float scaleSpeed = 12f;
    public float cardOffset = 6f;
    public Vector3 handScale = new Vector3(0.6f, 0.6f, 1f);
    public float redealDelay = 0.5f;
    public AudioSource redealAudioSource;
public AudioClip redealClip;

    // ================= INTERNAL =================
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

    float GetX(RectTransform stack, int index)
    {
        return stack == player1Stack
            ? index * cardOffset
            : -index * cardOffset;
    }

    public void ForceRedealFromCenter()
    {
        if (!isRedealing)
            StartCoroutine(StartRedeal());
    }

    IEnumerator StartRedeal()
    {
        isRedealing = true;
        turnmanager1.Instance.enabled = false;
     
 if (redealAudioSource != null && !redealAudioSource.isPlaying)
{
    redealAudioSource.clip = redealClip;
    redealAudioSource.loop = true;
    redealAudioSource.Play();
}
        yield return new WaitForSeconds(redealDelay);

        cards.Clear();

        while (centerStack.childCount > 0)
        {
            RectTransform card = centerStack.GetChild(0) as RectTransform;
            cards.Add(card);
            card.SetParent(centerStack.parent, true);
        }

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

        cardbeaviour1 cb = activeCard.GetComponent<cardbeaviour1>();
        Image img = activeCard.GetComponent<Image>();

        activeCard.sizeDelta = cb.GetOriginalSize();
        img.sprite = roundmanager1.Instance.backCard;

        int mod = currentIndex % 3;
        targetStack = mod == 0 ? player1Stack :
                      mod == 1 ? player2Stack : player3Stack;

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

        cardbeaviour1 cb = activeCard.GetComponent<cardbeaviour1>();
        cb.cardOwner =
            targetStack == player1Stack ? turnmanager1.Turn.Player1 :
            targetStack == player2Stack ? turnmanager1.Turn.Player2 :
            turnmanager1.Turn.Player3;

        int index = targetStack.childCount - 1;
        activeCard.anchoredPosition = new Vector2(GetX(targetStack, index), 0);
        activeCard.localScale = handScale;

        activeCard = null;
    }

    void EndRedeal()
    {
        
          if (redealAudioSource != null && redealAudioSource.isPlaying)
        redealAudioSource.Stop();
        isRedealing = false;

        roundmanager1.Instance.ArrangeHand(player1Stack);
        roundmanager1.Instance.ArrangeHand(player2Stack);
        roundmanager1.Instance.ArrangeHand(player3Stack);
        roundmanager1.Instance.pendingFinalResponse3P = false;

        // 🔑 PASS TURN AFTER REDEAL (NOT SAME PLAYER)
        turnmanager1.Instance.SetTurn(
            roundmanager1.Instance.GetNextAliveTurn(
                turnmanager1.Instance.currentTurn
            )
        );

        roundmanager1.Instance.currentState = roundmanager1.RoundState.Playing;
        turnmanager1.Instance.enabled = true;
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
