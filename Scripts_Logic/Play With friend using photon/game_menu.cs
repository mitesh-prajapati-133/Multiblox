using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
 

public class game_menu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject optionsPanel;
    public GameObject exitPanel;

    [Header("Menu Button")]
    public Button menuButton;

    [Header("Sound Toggle")]
    public Button soundButton;
    public TMP_Text soundButtonText;   // shows "Sound: ON" or "Sound: OFF"
    // OR use two sprites / images if you prefer icon-based toggle

    [Header("Audio")]
    public AudioSource clickAudioSource;
    public AudioClip clickSound;

    // ── internal ──
    private bool soundEnabled = true;   // default ON (matches local menu behaviour)

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        if (menuPanel  != null) menuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (exitPanel  != null) exitPanel.SetActive(false);

        // ✅ Read initial state from sound.Instance
        soundEnabled = sound.Instance != null ? sound.Instance.IsSoundOn() : true;
        RefreshSoundLabel();
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        // Lock menu button while cards are being distributed / collected
        if (menuButton == null) return;

        bool busy =
            distributor.IsDistributing ||
            (round.Instance != null &&
             round.Instance.currentState != round.RoundState.Playing);

        menuButton.interactable = !busy;
    }

    // ─────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────
    void PlayClick()
    {
        if (clickAudioSource != null && clickSound != null)
            clickAudioSource.PlayOneShot(clickSound);
    }

    void RefreshSoundLabel()
    {
        if (soundButtonText != null)
            soundButtonText.text = soundEnabled ? "Sound: ON" : "Sound: OFF";
    }

    // ─────────────────────────────────────────────────────────────
    // ☰  MENU BUTTON (top-left hamburger)
    // ─────────────────────────────────────────────────────────────
    public void ToggleMenu()
    {
        // Block while game is busy
        bool busy =
            distributor.IsDistributing ||
            (round.Instance != null &&
             round.Instance.currentState != round.RoundState.Playing);

        if (busy) return;

        PlayClick();

        // If options panel is open, close it first
        if (optionsPanel != null && optionsPanel.activeSelf)
            optionsPanel.SetActive(false);

        // Toggle main menu panel
        bool opening = menuPanel != null && !menuPanel.activeSelf;
        if (menuPanel != null) menuPanel.SetActive(opening);
    }

    // ─────────────────────────────────────────────────────────────
    // ❌  CLOSE / BACK button inside menu
    // ─────────────────────────────────────────────────────────────
    public void CloseMenu()
    {
        PlayClick();
        if (menuPanel    != null) menuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    // ⚙  OPTIONS button inside menu
    // ─────────────────────────────────────────────────────────────
    public void OpenOptions()
    {
        PlayClick();
        if (menuPanel    != null) menuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        RefreshSoundLabel();
    }

    // ─────────────────────────────────────────────────────────────
    // 🔊  SOUND TOGGLE button inside options panel
    // ─────────────────────────────────────────────────────────────
    public void ToggleSound()
    {
        PlayClick();

        // ✅ Use sound.Instance so icon AND text both update together
        if (sound.Instance != null)
            sound.Instance.ToggleSound();

        // ✅ Read actual state from sound.Instance
        soundEnabled = sound.Instance != null ? sound.Instance.IsSoundOn() : !soundEnabled;
        RefreshSoundLabel();
    }

    // ─────────────────────────────────────────────────────────────
    // 🚪  LEAVE button inside menu
    // ─────────────────────────────────────────────────────────────
    public void LeaveGame()
    {
        PlayClick();
        if (menuPanel    != null) menuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (exitPanel    != null) exitPanel.SetActive(true);
    }

    // ─────────────────────────────────────────────────────────────
    // EXIT PANEL  — NO button
    // ─────────────────────────────────────────────────────────────
    public void ExitNo()
    {
        PlayClick();
        if (exitPanel != null) exitPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    // EXIT PANEL  — YES button
    // Only THIS player leaves; the other player stays in the game.
    // ─────────────────────────────────────────────────────────────
    public void ExitYes()
    {
        PlayClick();

        // Stop card dragging for this player
        pausemanager.GameInputEnabled = false;

        // Leave Photon room — triggers OnPlayerLeftRoom on the other client
        PhotonNetwork.LeaveRoom();

        // Load this player's main menu
        SceneManager.LoadScene("Main menu");
    }
}
