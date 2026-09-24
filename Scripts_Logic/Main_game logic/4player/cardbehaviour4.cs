using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cardbehaviour4 : MonoBehaviour
{
       public RectTransform centerStack;
    public Sprite frontCard;

    public float previewDistance = 110f;
    public float snapDistance = 70f;

    public Vector2 tableCardSize;

    // Ownership (commented until turnmanager exists)
    [HideInInspector]
    public turnmanager4.Turn cardOwner;

    RectTransform rect;
    Image img;
    Vector2 startPos;
    Vector2 originalSize;

    bool dragging = false;
    bool isPreviewing = false;

    static cardbehaviour4 activeCard = null;

    [Header("Audio")]
    public AudioSource centerAudioSource;
    public AudioClip stickClip;

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
        HandleInput();
        if (dragging)
            HandleDistanceLogic();
    }

    void HandleInput()
    {
        // if (!pausemanager.GameInputEnabled) return;  // ❌ Commented until pausemanager exists
        // 🚫 Block all input if game is paused
    if (!pausemanager4.GameInputEnabled)
        return;

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
        BotController4 bot = FindObjectOfType<BotController4>();

if (bot != null)
{
    if ((bot.bot2Enabled && cardOwner == turnmanager4.Turn.Player2) ||
        (bot.bot3Enabled && cardOwner == turnmanager4.Turn.Player3) ||
        (bot.bot4Enabled && cardOwner == turnmanager4.Turn.Player4))
    {
        return;
    }
}
          if (carddistributor4.IsDistributing) return;
    if (!turnmanager4.Instance.enabled) return;

    if (activeCard != null) return;
    if (!IsTopCard()) return;
    if (!turnmanager4.Instance.CanDrag(transform)) return;

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
                dragging = false;
                activeCard = null;

                rect.SetParent(centerStack, false);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = tableCardSize;
                rect.localScale = Vector3.one;
                img.sprite = frontCard;

                if (centerAudioSource && stickClip)
                    centerAudioSource.PlayOneShot(stickClip);
                    

                roundmanager4.Instance.OnCardPlaced(this); 
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

        if (centerAudioSource != null && stickClip != null)
            centerAudioSource.PlayOneShot(stickClip);

        // ✅ Switch turn after placing

        roundmanager4.Instance.OnCardPlaced(this); // ❌ Commented until roundmanager exists
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

    // Static helpers
    public static bool ForceReleaseActiveCard()
    {
        if (activeCard == null)
            return false;

        cardbehaviour4 card = activeCard;
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

            roundmanager4.Instance.OnCardPlaced(card); 
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

    public static cardbehaviour4 GetActiveCard()
    {
        return activeCard;
    }

    public void StopDrag()
    {
        dragging = false;
        activeCard = null;
    }
public static bool ForceStickActiveCardImmediate()
{
    if (activeCard == null)
        return false;

    cardbehaviour4 card = activeCard;
    activeCard = null;
    card.dragging = false;

    // 🔥 FORCE STICK — IGNORE DISTANCE / PREVIEW
    card.rect.SetParent(card.centerStack, false);
    card.rect.anchoredPosition = Vector2.zero;
    card.rect.sizeDelta = card.tableCardSize;
    card.rect.localScale = Vector3.one;
    card.img.sprite = card.frontCard;

    if (card.centerAudioSource && card.stickClip)
        card.centerAudioSource.PlayOneShot(card.stickClip);

    roundmanager4.Instance.OnCardPlaced(card);
    return true;
}
}
