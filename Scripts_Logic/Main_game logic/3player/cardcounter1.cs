using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class cardcounter1 : MonoBehaviour
{
    public RectTransform playerStack;
    public TextMeshProUGUI countText;

    public void Update()
    {
        countText.text = "Total Cards : " + playerStack.childCount;
    }
}
