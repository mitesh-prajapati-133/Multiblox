using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class pausemanager4 : MonoBehaviour
{
  [Header("UI References")]
    public GameObject pausePanel;
    public GameObject exitPanel;
    public Button clockButton;
    public Button resumeButton;
    public Button replayButton;
    public Button leaveButton;
    public Button soundButton;

    [Header("Sound Button Images")]
    public Image soundButtonImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip clockClickSound;
    public AudioClip resumeClickSound;
    public static bool GameInputEnabled = true;

    private bool isPaused = false;
    private bool isMuted = false;

    void Start()
    {
        pausePanel.SetActive(false);
        if (exitPanel) exitPanel.SetActive(false);
    }

    void Update()
    {
        bool isGameBusy =
            carddistributor4.IsDistributing ||
            (roundmanager4.Instance != null &&
             roundmanager4.Instance.currentState != roundmanager4.RoundState.Playing);

        // 🔒 Disable pause button when game is busy
        clockButton.interactable = !isGameBusy;

        // ANDROID BACK BUTTON (pause only)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
        }
    }

    // CLOCK BUTTON
    public void PauseGame()
    {
        GameInputEnabled = false;

        if (carddistributor4.IsDistributing ||
            (roundmanager4.Instance != null &&
             roundmanager4.Instance.currentState != roundmanager4.RoundState.Playing))
            return;

        PlayClockSound();
        pausePanel.SetActive(true);
        isPaused = true;
        GameInputEnabled = false;
        Time.timeScale = 0f;
    }

    // RESUME BUTTON
    public void ResumeGame()
    {
        GameInputEnabled = true;

        PlayResumeSound();
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
        GameInputEnabled = true;
    }

    // REPLAY BUTTON
    public void ReplayGame()
    {
        PlayResumeSound(); // reuse resume sound
        StartCoroutine(ReloadSceneAfterSound());
    }

    IEnumerator ReloadSceneAfterSound()
    {
        Time.timeScale = 1f;
        GameInputEnabled = true;
        yield return new WaitForSecondsRealtime(resumeClickSound.length);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // LEAVE BUTTON
    public void LeaveGame()
    {
        if (exitPanel)
        {
            exitPanel.SetActive(true);
        }
    }

    // SOUND TOGGLE BUTTON
    public void ToggleSound()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            MuteAllSounds();
            if (soundButtonImage && soundOffSprite)
                soundButtonImage.sprite = soundOffSprite;
        }
        else
        {
            UnmuteAllSounds();
            if (soundButtonImage && soundOnSprite)
                soundButtonImage.sprite = soundOnSprite;
        }
    }

    void MuteAllSounds()
    {
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (var audio in allAudio)
        {
            if (audio != null)
                audio.mute = true;   // ✅ mute instead of stop
        }
    }

    void UnmuteAllSounds()
    {
        AudioSource[] allAudio = FindObjectsOfType<AudioSource>();
        foreach (var audio in allAudio)
        {
            if (audio != null)
                audio.mute = false;  // ✅ unmute to restore playback
        }
    }

    void PlayClockSound()
    {
        if (uiAudioSource && clockClickSound)
            uiAudioSource.PlayOneShot(clockClickSound);
    }

    void PlayResumeSound()
    {
        if (uiAudioSource && resumeClickSound)
            uiAudioSource.PlayOneShot(resumeClickSound);
    }

}
