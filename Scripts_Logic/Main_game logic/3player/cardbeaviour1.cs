using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cardbeaviour1 : MonoBehaviour
{

    public RectTransform centerStack;
    public Sprite frontCard;

    public float previewDistance = 110f;
    public float snapDistance = 70f;

    public Vector2 tableCardSize;

    [HideInInspector]
    public turnmanager1.Turn cardOwner;

    RectTransform rect;
    Image img;
    Vector2 startPos;
    Vector2 originalSize;

    bool dragging = false;
    bool isPreviewing = false;

    static cardbeaviour1 activeCard = null;
    public AudioSource centerAudioSource;   // AudioSource on CenterStack
public AudioClip stickClip;       

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        originalSize = rect.sizeDelta; // ✅ stored once
    }

    public Vector2 GetOriginalSize()
    {
        return originalSize;
    }

    void Update()
    {
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

    void TryStartDrag(Vector2 screenPos)
    {  
        BotController3 botController = FindObjectOfType<BotController3>();

if (botController != null)
{
    if ((botController.bot2Enabled && cardOwner == botController.bot2Turn) ||
        (botController.bot3Enabled && cardOwner == botController.bot3Turn))
    {
        return; // Block drag for bot-controlled players
    }
}
        if (roundmanager1.Instance.currentState != roundmanager1.RoundState.Playing)
        return;

    // 🔒 BLOCK DRAG WHILE DISTRIBUTING
    if (NewBehaviourScript1.IsDistributing)
        return;

    // 🔒 BLOCK DRAG IF TURN SYSTEM DISABLED
    if (!turnmanager1.Instance.enabled)
        return;

    if (activeCard != null) return;
    if (!IsTopCard()) return;
    if (!turnmanager1.Instance.CanDrag(transform)) return;

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
    }

    void HandleDistanceLogic()
    {
        float distance = Vector2.Distance(rect.position, centerStack.position);

        if (distance < previewDistance && !isPreviewing)
        {
            isPreviewing = true;
            img.sprite = frontCard;
            rect.sizeDelta = tableCardSize;
               Transform stack = transform.parent;
        if (stack != null && stack.childCount == 1)
        {
            // stop dragging and clear active card
            dragging = false;
            activeCard = null;

            rect.SetParent(centerStack, false);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = tableCardSize;
            rect.localScale = Vector3.one;
            img.sprite = frontCard;

            if (centerAudioSource && stickClip)
                centerAudioSource.PlayOneShot(stickClip);

            // let round manager handle turn (same as normal stick)
            roundmanager1.Instance.OnCardPlaced(this);

            return; // 🚫 IMPORTANT: prevents Release() logic
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
           if (centerAudioSource != null && stickClip != null)
    {
        centerAudioSource.PlayOneShot(stickClip);
    }

        // ✅ ROUND MANAGER DECIDES TURN
        roundmanager1.Instance.OnCardPlaced(this);
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
    public static bool ForceReleaseActiveCard()
{
    if (activeCard == null)
        return false;

    cardbeaviour1 card = activeCard;
    activeCard = null;
    card.dragging = false;

    float distance = Vector2.Distance(card.rect.position, card.centerStack.position);

    bool placed = (card.isPreviewing || distance < card.snapDistance);

    if (placed)
    {
        card.rect.SetParent(card.centerStack, false);
        card.rect.anchoredPosition = Vector2.zero;
        card.rect.sizeDelta = card.tableCardSize;
        card.rect.localScale = Vector3.one;
        card.img.sprite = card.frontCard;

        if (card.centerAudioSource && card.stickClip)
            card.centerAudioSource.PlayOneShot(card.stickClip);

        roundmanager1.Instance.OnCardPlaced(card);
    }
    else
    {
        card.rect.anchoredPosition = card.startPos;
        card.rect.sizeDelta = card.originalSize;
        card.isPreviewing = false;
    }

    return placed; 
}
public static bool ActiveCardExists()
{
    return activeCard != null;
}

public static cardbeaviour1 GetActiveCard()
{
    return activeCard;
}

public void StopDrag()
{
    dragging = false;
    activeCard = null;
}
}
