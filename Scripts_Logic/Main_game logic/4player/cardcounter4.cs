using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class cardcounter4 : MonoBehaviour
{
  public RectTransform playerStack;
    public TextMeshProUGUI countText;

    public void Update()
    {
        countText.text = "Total Cards : " + playerStack.childCount;
    }
}
