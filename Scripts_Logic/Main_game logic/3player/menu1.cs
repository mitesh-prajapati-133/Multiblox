using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class menu1 : MonoBehaviour
{
   
   public GameObject menuPanel;
    public GameObject optionsPanel;
    public GameObject exitPanel;

    [Header("Audio")]
    public AudioSource clickAudioSource;   // AudioSource
    public AudioClip clickSound;          
    
public Button menuButton; // click sound

    void Start()
    {
        menuPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }
    void Update()
{
    bool isGameBusy =
        // 2P distribution
        NewBehaviourScript1.IsDistributing ||

        // 2P collect / redeal
        (roundmanager1.Instance != null &&
         roundmanager1.Instance.currentState != roundmanager1.RoundState.Playing);

    // 🔒 Disable menu button when busy
    if (menuButton != null)
        menuButton.interactable = !isGameBusy;
}

    // 🔊 Play click sound
    void PlayClick()
    {
        if (clickAudioSource != null && clickSound != null)
        {
            clickAudioSource.PlayOneShot(clickSound);
        }
    }

    // ☰ Menu button
    public void ToggleMenu()
    {
         bool isGameBusy =
        NewBehaviourScript1.IsDistributing ||
        (roundmanager1.Instance != null &&
         roundmanager1.Instance.currentState != roundmanager1.RoundState.Playing);

    if (isGameBusy)
        return;

    PlayClick();

    if (optionsPanel.activeSelf)
    {
        optionsPanel.SetActive(false);
    }

    bool isOpening = !menuPanel.activeSelf;
    menuPanel.SetActive(isOpening);

     
    }

    // ❌ Close button
    public void CloseMenu()
    {
       PlayClick();

    if (menuPanel != null)
        menuPanel.SetActive(false);

    if (optionsPanel != null)
        optionsPanel.SetActive(false);

    if (sound.Instance != null)
        sound.Instance.ClosePanel();

    Time.timeScale = 1f;
    }

    // ⚙ Options button
    public void OpenOptions()
    {
        
    PlayClick();

    // Close menu
    menuPanel.SetActive(false);

    // Open options panel
    optionsPanel.SetActive(true);
    if (sound.Instance != null && sound.Instance.soundPanel != null)
    {
        sound.Instance.soundPanel.SetActive(true);
    }

    // 🔑 VERY IMPORTANT: re-enable sound panel
 
    }

    // 🚪 Leave button
    public void LeaveGame()
    {
        PlayClick();
        
    pausemanager.GameInputEnabled = false;
        Time.timeScale = 0f;

            exitPanel.SetActive(true);
    }
}
