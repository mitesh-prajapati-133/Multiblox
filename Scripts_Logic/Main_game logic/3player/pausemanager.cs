using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class pausemanager : MonoBehaviour
{
  [Header("UI References")]
    public GameObject pausePanel;
    public Button clockButton;
    public Button resumeButton;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip clockClickSound;
    public AudioClip resumeClickSound;
    public static bool GameInputEnabled = true;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);
    }
    void Update()
{   
    bool isGameBusy =
        // 2P distribution
        NewBehaviourScript.IsDistributing ||

        // 3P distribution
        NewBehaviourScript1.IsDistributing ||

        // 2P collect / redeal
        (roundmanager.Instance != null &&
         roundmanager.Instance.currentState != roundmanager.RoundState.Playing) ||

        // 3P collect / redeal
        (roundmanager1.Instance != null &&
         roundmanager1.Instance.currentState != roundmanager1.RoundState.Playing);

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
         if (
        NewBehaviourScript.IsDistributing ||
        NewBehaviourScript1.IsDistributing ||

        (roundmanager.Instance != null &&
         roundmanager.Instance.currentState != roundmanager.RoundState.Playing) ||

        (roundmanager1.Instance != null &&
         roundmanager1.Instance.currentState != roundmanager1.RoundState.Playing)
    )
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
        PlayResumeSound();
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
            GameInputEnabled = true; 
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
    public void ReplayGame()
{
    PlayResumeSound(); // or create a separate replaySound if you want
    StartCoroutine(ReloadSceneAfterSound());
}
IEnumerator ReloadSceneAfterSound()
{
    // Resume time before reloading
    Time.timeScale = 1f;
    GameInputEnabled = true;

    // Wait for sound to finish (timescale safe)
    yield return new WaitForSecondsRealtime(resumeClickSound.length);

    // Reload current scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
}
