using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shuffle : MonoBehaviour
{
     public RectTransform playerStack;

    public float shuffleTime = 0.6f;
    public float fanDistance = 100f;
    public float fanAngle = 30f;

    public bool invertDirection;

    [Header("Audio")]
    public AudioSource shuffleAudio;   // 👈 NEW

    List<RectTransform> cards = new List<RectTransform>();
    List<Vector2> startPos = new List<Vector2>();
    List<Vector2> targetPos = new List<Vector2>();

    float timer;
    bool isShuffling;

    void Update()
    {
        if (!isShuffling) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / shuffleTime);
        t = t * t * (3f - 2f * t);

        float dir = invertDirection ? -1f : 1f;

        for (int i = 0; i < cards.Count; i++)
        {
            float angleStep = fanAngle / (cards.Count - 1);
            float angle = -fanAngle / 2 + angleStep * i;

            Vector2 pos = Vector2.Lerp(startPos[i], targetPos[i], t);

            pos.x += Mathf.Sin(angle * Mathf.Deg2Rad) * fanDistance * Mathf.Sin(t * Mathf.PI);
            pos.y += Mathf.Cos(angle * Mathf.Deg2Rad) * fanDistance * Mathf.Sin(t * Mathf.PI) * dir;

            cards[i].anchoredPosition = pos;
            cards[i].localRotation =
                Quaternion.Euler(0, 0, angle * Mathf.Sin(t * Mathf.PI));
        }

        if (t >= 1f)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].anchoredPosition = targetPos[i];
                cards[i].localRotation = Quaternion.identity;
            }
            isShuffling = false;
        }
    }

    public void Shuffle()
    {
        if (isShuffling) return;

        // 🔊 PLAY SHUFFLE SOUND
        if (shuffleAudio != null)
            shuffleAudio.PlayOneShot(shuffleAudio.clip);

        cards.Clear();
        startPos.Clear();
        targetPos.Clear();

        foreach (Transform child in playerStack)
            cards.Add(child.GetComponent<RectTransform>());

        foreach (RectTransform c in cards)
            startPos.Add(c.anchoredPosition);

        for (int i = 0; i < cards.Count; i++)
        {
            int r = Random.Range(i, cards.Count);
            RectTransform temp = cards[i];
            cards[i] = cards[r];
            cards[r] = temp;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            targetPos.Add(startPos[i]);
            cards[i].SetSiblingIndex(i);
        }

        timer = 0f;
        isShuffling = true;
    }
}
