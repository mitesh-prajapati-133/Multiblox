using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  

public class exit : MonoBehaviour
{
      [Header("UI")]
    public GameObject exitPanel;
    public Button yesButton;
    public Button noButton;
    public Button closeButton;

    [Header("Audio")]
    public AudioSource uiAudio;
    public AudioClip openSound;
    public AudioClip clickSound;

    private bool isExitOpen = false;

   public void Start()
    {
        exitPanel.SetActive(false);

        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
        closeButton.onClick.AddListener(OnNo);
    }

    void Update()
    {
          if (Input.GetKeyDown(KeyCode.Escape))
          
    {
        // 🚫 Block during 2-Player distribution
        if (NewBehaviourScript.IsDistributing)
            return;

        // 🚫 Block during 3-Player distribution
        if (NewBehaviourScript1.IsDistributing)
            return;
if (carddistributor4.IsDistributing)
    return; 
        // 🚫 Block during 2-Player collect / redeal
        if (roundmanager.Instance != null &&
            roundmanager.Instance.currentState != roundmanager.RoundState.Playing)
            return;

        // 🚫 Block during 3-Player collect / redeal
        if (roundmanager1.Instance != null &&
            roundmanager1.Instance.currentState != roundmanager1.RoundState.Playing)
            return;
            if (roundmanager4.Instance != null &&
    roundmanager4.Instance.currentState != roundmanager4.RoundState.Playing)
    return;

        // 🚫 Block if pause system disabled input
        if (!pausemanager.GameInputEnabled)
            return;

        // ✅ Otherwise allow exit
        if (!isExitOpen)
            OpenExit();
        else
            CloseExit();
    }
    }

   public void OpenExit()
    {
        PlaySound(openSound);
            pausemanager.GameInputEnabled = false; 
           pausemanager4.GameInputEnabled = false;
        exitPanel.SetActive(true);
        Time.timeScale = 0f;
        isExitOpen = true;
    }

   public void CloseExit()
    {
        PlaySound(clickSound);
        
    pausemanager.GameInputEnabled = true;
pausemanager4.GameInputEnabled = true;
        exitPanel.SetActive(false);
        Time.timeScale = 1f;
        isExitOpen = false;
    }

   public void OnNo()
    {
        CloseExit();
    }

  public void OnYes()
    {
        PlaySound(clickSound);
            StartCoroutine(LoadSceneAfterSound());
 
    }

    public void PlaySound(AudioClip clip)
    {
        if (uiAudio && clip)
            uiAudio.PlayOneShot(clip);
    }
    IEnumerator LoadSceneAfterSound()
{
    pausemanager.GameInputEnabled = true;
   pausemanager4.GameInputEnabled = true;
    Time.timeScale = 1f;

    // IMPORTANT: realtime because timescale was 0
    yield return new WaitForSecondsRealtime(clickSound.length);

    SceneManager.LoadScene("Main Menu");
}
}
