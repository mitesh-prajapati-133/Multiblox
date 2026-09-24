using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class screenfade : MonoBehaviour
{
       public Image fadeImage;
    public float fadeDuration = 1.5f;

    float timer = 0f;
    bool fading = true;

    void Start()
    {
        Color c = fadeImage.color;
        c.a = 1f;              // fully black
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!fading) return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / fadeDuration);
        t = Mathf.SmoothStep(0f, 1f, t);   // 🔥 smooth cinematic fade

        Color c = fadeImage.color;
        c.a = Mathf.Lerp(1f, 0f, t);
        fadeImage.color = c;

        if (timer >= fadeDuration)
        {
            fading = false;
            fadeImage.gameObject.SetActive(false);
        }
    }
}
