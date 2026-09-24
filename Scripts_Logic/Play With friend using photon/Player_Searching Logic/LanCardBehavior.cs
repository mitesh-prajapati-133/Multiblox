using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LanCardBehavior : cardbehavior
{
     protected override void TryStartDrag(Vector2 screenPos)
    {
        // Host = Player1 only, Joiner = Player2 only
        if (LanManager.IsHost && cardOwner == turnmanager.Turn.Player2)
            return;

        if (!LanManager.IsHost && cardOwner == turnmanager.Turn.Player1)
            return;

        base.TryStartDrag(screenPos);
    }

    // ── SYNC DRAG POSITION ──
    protected override void OnDragUpdate(Vector2 anchoredPos)
    {

    }

    // ── SYNC CARD PLACED ──
    protected override void OnCardSnapped()
    {
         if (LanManager.Instance != null)
    {
        int value = GetComponent<carddealer>().cardValue;

        LanManager.Instance.SendCardPlaced(cardIndex, (int)cardOwner, value);
             LanManager.Instance.SendStuckFlag((int)cardOwner);
    }

    StartCoroutine(SendTurnAfterDelay());
   
    }

    // ── REMOTE FORCE SNAP (called on other phone) ──
    public void ForceSnap()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Image img = GetComponent<Image>();
        RectTransform center = roundmanager.Instance.centerStack;
           if (rect.parent == center)
        return;
   Debug.Log("ForceSnap: " + gameObject.name + 
              " owner=" + cardOwner + 
              " parent=" + rect.parent.name);
        rect.SetParent(center, false);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = tableCardSize;
        rect.localScale = Vector3.one;
        img.sprite = frontCard;
        

turnmanager.Instance.SetTurn(cardOwner); 
    roundmanager.Instance.OnCardPlaced(this);
  

    }
    IEnumerator SendTurnAfterDelay()
{
    yield return new WaitForEndOfFrame(); // wait 1 frame

    int turn = (int)turnmanager.Instance.currentTurn;
    if (LanManager.Instance != null)
        LanManager.Instance.SendTurnSwitch(turn);
}

}
