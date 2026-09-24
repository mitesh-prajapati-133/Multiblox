using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class behaviour : MonoBehaviourPun

{
  public RectTransform centerStack;
    public Sprite frontCard;
 
    public float previewDistance = 110f;
    public float snapDistance = 70f;
 
    [HideInInspector]
    public int cardIndex = -1;
 
    public Vector2 tableCardSize;
 
    [HideInInspector]
    public turndown.Turn cardOwner;
 
    [Header("Audio")]
    public AudioSource centerAudioSource;
    public AudioClip stickClip;
 public static int currentDraggingCardIndex = -1;
    RectTransform rect;
    Image img;
    Vector2 startPos;
    Vector2 originalSize;
 
    bool dragging = false;
    bool isPreviewing = false;
 
    static behaviour activeCard = null;
    static bool turnProcessing = false;
 
    protected virtual void OnDragUpdate(Vector2 anchoredPos) { }
    protected virtual void OnCardSnapped() { }
 
    void Awake()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        originalSize = rect.sizeDelta;
    }
 
    public Vector2 GetOriginalSize() => originalSize;
 
    void Update()
    {
        if (turnProcessing && !turndown.Instance.CanDrag(transform))
            turnProcessing = false;
 
        HandleInput();
 
        if (dragging)
            HandleDistanceLogic();
    }
 
    void HandleInput()
    {
        if (!pausemanager.GameInputEnabled) return;
 
        if (Input.GetMouseButtonDown(0))
            TryStartDrag(Input.mousePosition);
 
        if (Input.GetMouseButton(0) && dragging)
            Drag(Input.mousePosition);
 
        if (Input.GetMouseButtonUp(0) && dragging)
            Release();
 
        if (Input.touchCount == 0) return;
 
        Touch t = Input.GetTouch(0);
 
        if (t.phase == TouchPhase.Began)
            TryStartDrag(t.position);
 
        if (t.phase == TouchPhase.Moved && dragging)
            Drag(t.position);
 
        if (t.phase == TouchPhase.Ended && dragging)
            Release();
    }
 
    protected virtual void TryStartDrag(Vector2 screenPos)
    {
        // ✅ BLOCK: Bot check
        BotController bot = FindObjectOfType<BotController>();
        if (bot != null && cardOwner == turndown.Turn.Player2)
            return;
 
        if (turnProcessing) return;
 
        if (round.Instance.currentState != round.RoundState.Playing)
            return;
 
        if (distributor.IsDistributing)
            return;
 
        if (!turndown.Instance.enabled)
            return;
            if (!GameNetworkManager.bothSidesReady)
    return;
 
        if (activeCard != null) return;
        if (!IsTopCard()) return;
 
        // ✅ CORE FIX: This card must belong to the current turn AND to this local device
        // MasterClient owns Player1 cards, Joiner owns Player2 cards
        turndown.Turn myTurn = turndown.MyTurn();
        if (cardOwner != myTurn) return;            // not my card
        if (cardOwner != turndown.Instance.currentTurn) return; // not my turn
 
        if (!RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, null))
            return;
 
        activeCard = this;
        dragging = true;
        isPreviewing = false;
 
        startPos = rect.anchoredPosition;
        transform.SetAsLastSibling();
        currentDraggingCardIndex = cardIndex;

// 🔥 SEND TO HOST
GameNetworkManager.Instance.SendDraggingCard(cardIndex);
    }
 
    void Drag(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect.parent as RectTransform,
            screenPos,
            null,
            out Vector2 localPos);
 
        rect.anchoredPosition = localPos;
        OnDragUpdate(rect.anchoredPosition);
    }
 
    void HandleDistanceLogic()
    {
        float distance = Vector2.Distance(rect.position, centerStack.position);
 
        if (distance < previewDistance && !isPreviewing)
        {
            isPreviewing = true;
            img.sprite = frontCard;
            rect.sizeDelta = tableCardSize;
 
            // Last card auto-stick
            Transform stack = transform.parent;
            if (stack != null && stack.childCount == 1)
            {
                dragging = false;
                activeCard = null;
 
                RectTransform center = round.Instance.centerStack;
 
                rect.SetParent(center, false);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = tableCardSize;
                rect.localScale = Vector3.one;
                img.sprite = frontCard;
 
                if (centerAudioSource && stickClip)
                    centerAudioSource.PlayOneShot(stickClip);
 
                round.Instance.OnCardPlaced(this);
                OnCardSnapped();
 
                // ✅ Send to other client
    GameNetworkManager.Instance.SendCard(cardIndex);
 
                if (round.Instance.currentState == round.RoundState.Playing)
                    turndown.Instance.SwitchTurn(); // ✅ This now syncs via RPC
 
                isPreviewing = true;
                return;
            }
        }
    }
 
    void Release()
    {
        dragging = false;
        activeCard = null;
 
        float distance = Vector2.Distance(rect.position, centerStack.position);
 
        if (isPreviewing || distance < snapDistance)
        {
            rect.SetParent(centerStack, false);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = tableCardSize;
            rect.localScale = Vector3.one;
            img.sprite = frontCard;
 
            if (centerAudioSource && stickClip)
                centerAudioSource.PlayOneShot(stickClip);
 
            bool isMatch = false;
 
            if (centerStack.childCount >= 2)
            {
                int last = centerStack.GetChild(centerStack.childCount - 1)
                    .GetComponent<carddealer>().cardValue;
 
                int prev = centerStack.GetChild(centerStack.childCount - 2)
                    .GetComponent<carddealer>().cardValue;
 
                if (last == prev)
                    isMatch = true;
            }
 
            round.Instance.OnCardPlaced(this);
 
            // ✅ Send card to other client
 GameNetworkManager.Instance.SendCard(cardIndex);
 
            OnCardSnapped();
 
            if (!isMatch)
            {
                if (round.Instance.currentState == round.RoundState.Playing)
                    turndown.Instance.SwitchTurn(); // ✅ Synced via RPC in turndown
            }
        }
        else
        {
            rect.anchoredPosition = startPos;
            rect.sizeDelta = originalSize;
            isPreviewing = false;
        }
        currentDraggingCardIndex = -1;
GameNetworkManager.Instance.SendDraggingCard(-1);
    }
 
    bool IsTopCard()
    {
        Transform stack = transform.parent;
        if (stack == null) return false;
        return transform == stack.GetChild(stack.childCount - 1);
    }
 
    public static void ForceReleaseActiveCard()
    {
        if (activeCard == null) return;
 
        behaviour card = activeCard;
        card.dragging = false;
        activeCard = null;
 
        if (card.rect.parent == card.centerStack) return;
 
        float distance = Vector2.Distance(card.rect.position, card.centerStack.position);
 
        if (card.isPreviewing || distance < card.snapDistance)
        {
            card.rect.SetParent(card.centerStack, false);
            card.rect.anchoredPosition = Vector2.zero;
            card.rect.sizeDelta = card.tableCardSize;
            card.rect.localScale = Vector3.one;
            card.img.sprite = card.frontCard;
 
            if (card.centerAudioSource && card.stickClip)
                card.centerAudioSource.PlayOneShot(card.stickClip);
 
            round.Instance.OnCardPlaced(card);
        }
        else
        {
            card.rect.anchoredPosition = card.startPos;
            card.rect.sizeDelta = card.originalSize;
            card.isPreviewing = false;
        }
    }
 
    public static bool HasPlayerStuck(turndown.Turn turn)
    {
        if (turn == turndown.Turn.Player1)
            return round.Instance.player1HasStuck;
        else
            return round.Instance.player2HasStuck;
    }
 
    public static bool ActiveCardExists() => activeCard != null;
    public static behaviour GetActiveCard() => activeCard;
 
    public void StopDrag()
    {
        dragging = false;
        activeCard = null;
        currentDraggingCardIndex = -1;
GameNetworkManager.Instance.SendDraggingCard(-1);
    }
 
    // ✅ Called on the RECEIVING client — no turn switch, no SendCard
    public void ForceNetworkPlace()
    {
         if (activeCard == this)
    {
        dragging = false;
        activeCard = null;
    }

        RectTransform r = GetComponent<RectTransform>();
        Image i = GetComponent<Image>();
 
        r.SetParent(centerStack, false);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = tableCardSize;
        r.localScale = Vector3.one;
        i.sprite = frontCard;
 
        if (centerAudioSource && stickClip)
            centerAudioSource.PlayOneShot(stickClip);
 
        round.Instance.OnCardPlaced(this);
        // ✅ NO SwitchTurn here — turndown RPC handles it from the sender side
    }
}
