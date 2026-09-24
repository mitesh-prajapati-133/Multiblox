using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class cardbehavior : MonoBehaviour
{
       public RectTransform centerStack;
    public Sprite frontCard;

    public float previewDistance = 110f;
    public float snapDistance = 70f;
[HideInInspector]
public int cardIndex = -1; 
    public Vector2 tableCardSize;

    [HideInInspector]
    public turnmanager.Turn cardOwner;

    [Header("Audio")]
    public AudioSource centerAudioSource;
    public AudioClip stickClip;

    RectTransform rect;
    Image img;
    Vector2 startPos;
    Vector2 originalSize;

    bool dragging = false;
    bool isPreviewing = false;

    static cardbehavior activeCard = null;

    // 🔒 prevents same player from dragging again in same frame
    static bool turnProcessing = false;
    protected virtual void OnDragUpdate(Vector2 anchoredPos) { }
protected virtual void OnCardSnapped() { }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        originalSize = rect.sizeDelta;
    }

    public Vector2 GetOriginalSize()
    {
        return originalSize;
    }

    void Update()
    {
        // unlock automatically when turn really changes
        if (turnProcessing && !turnmanager.Instance.CanDrag(transform))
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
        BotController bot = FindObjectOfType<BotController>();
        if (bot != null && cardOwner == turnmanager.Turn.Player2)
            return;

        if (turnProcessing) return;

        if (roundmanager.Instance.currentState != roundmanager.RoundState.Playing)
            return;

        if (NewBehaviourScript.IsDistributing)
            return;

        if (!turnmanager.Instance.enabled)
            return;

        if (activeCard != null) return;
        if (!IsTopCard()) return;
        if (!turnmanager.Instance.CanDrag(transform)) return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, null))
            return;

        activeCard = this;
        dragging = true;
        isPreviewing = false;

        startPos = rect.anchoredPosition;
        transform.SetAsLastSibling();
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

        // ✅ NEW: If this is the last card in the player's stack, auto-stick immediately
        // If this is the last card in the player's stack, auto-stick immediately (no animation)
Transform stack = transform.parent;
if (stack != null && stack.childCount == 1)
{
  
    // prevent duplicate execution if you have a guard flag
    // stop dragging and clear active card
    dragging = false;
    activeCard = null;

    // use roundmanager center stack to place card
    RectTransform center = roundmanager.Instance.centerStack;

    rect.SetParent(center, false);
    rect.anchoredPosition = Vector2.zero;
    rect.sizeDelta = tableCardSize;
    rect.localScale = Vector3.one;
    img.sprite = frontCard;

    if (centerAudioSource && stickClip)
        centerAudioSource.PlayOneShot(stickClip);
   

    // notify round manager and pass the turn
    roundmanager.Instance.OnCardPlaced(this);
    OnCardSnapped();
if (roundmanager.Instance.currentState == roundmanager.RoundState.Playing)
{
        turnmanager.Instance.SwitchTurn();
}

    // mark preview/auto-stuck state if you use a guard (optional)
    isPreviewing = true;
    return; // exit so Release() logic doesn't run for this card
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

   
            

        // 🔥 CHECK MATCH BEFORE SWITCHING TURN
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

  roundmanager.Instance.OnCardPlaced(this); // LOCAL FIRST ✅
        OnCardSnapped();  
 
       if (!isMatch)
{
    if (roundmanager.Instance.currentState == roundmanager.RoundState.Playing)
    {
        // ✅ LAN: SyncVar OnTurnChanged handles turn switch
        // Solo: switch locally as before
   
            turnmanager.Instance.SwitchTurn();
    }
}
        
          
    }
    else
    {
        rect.anchoredPosition = startPos;
        rect.sizeDelta = originalSize;
        isPreviewing = false;
    }
    
    }

    bool IsTopCard()
    {
        Transform stack = transform.parent;
        if (stack == null) return false;
        return transform == stack.GetChild(stack.childCount - 1);
    }
    public static void ForceReleaseActiveCard()
{
        if (activeCard == null)
        return;

    cardbehavior card = activeCard;
    card.dragging = false;
    activeCard = null;

    // ✅ NEW: If card is already in centerStack, do nothing
    if (card.rect.parent == card.centerStack)
    {
        return;
    }

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

        roundmanager.Instance.OnCardPlaced(card);
    }
    else
    {
        card.rect.anchoredPosition = card.startPos;
        card.rect.sizeDelta = card.originalSize;
        card.isPreviewing = false;
    }

}
public static bool HasPlayerStuck(turnmanager.Turn turn)
{
    // Example placeholder logic:
    // Return true if the player has already stuck their card
    // You must replace this with your actual game condition
    if (turn == turnmanager.Turn.Player1)
        return roundmanager.Instance.player1HasStuck;
    else
        return roundmanager.Instance.player2HasStuck;
}
public static bool ActiveCardExists()
{
    return activeCard != null;
}

public static cardbehavior GetActiveCard()
{
    return activeCard;
}
public void StopDrag()
{
    dragging = false;
    activeCard = null;
}


}
