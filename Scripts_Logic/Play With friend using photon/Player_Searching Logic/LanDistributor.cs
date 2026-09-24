using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanDistributor :  NewBehaviourScript
{
  protected override void Shuffle(List<RectTransform> list)
    {
        // Same seed = same shuffle on both phones ✅
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
