using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sound : MonoBehaviour
{
public static sound Instance;

    [Header("UI")]
    public Image soundIcon;
    public Sprite soundOnSprite;   // green button
    public Sprite soundOffSprite;  // red button

    [Header("Panel")]
    public GameObject soundPanel;

    private bool isSoundOn = true;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateSoundState();
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        UpdateSoundState();
    }

    void UpdateSoundState()
    {
        // ✅ SAFE AUDIO MUTE (NO GAME FREEZE)
        AudioListener.volume = isSoundOn ? 1f : 0f;

        if (soundIcon != null)
        {
            soundIcon.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }

    // 🔴 CLOSE PANEL
    public void ClosePanel()
    {
        if (soundPanel != null)
            soundPanel.SetActive(false);
    }

    public bool IsSoundOn()
    {
        return isSoundOn;
    }
}
