using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class cardcounter : MonoBehaviour
{
    public RectTransform playerStack;
    public TextMeshProUGUI countText;

    void Update()
    {
        // COUNT ONLY CARDS (tag = "Card") ✅
        int count = 0;
        foreach (Transform child in playerStack)
            if (child.CompareTag("Card"))
             count++;
        countText.text = "Total Cards : " + count;
    }
}
