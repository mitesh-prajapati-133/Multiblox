using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button topBarOptionsButton;      // Top bar button to open Options panel
    public Button optionsSettingsButton;    // Button inside Options panel to open Settings
    public Button playVsPlayButton;
    public Button playVsComputerButton;
    public Button playWithFriendsButton;
    public Button leaveButton;
    public Button exitYesButton;
    public Button exitNoButton;
    public Button shopButton;
     public Button Creditbutton;
     public Button done;
    [Header("Audio Settings")]
public Toggle soundToggle;

    [Header("Panels")]
    public GameObject mainMenuPanel;        // The main menu UI
    public GameObject optionsPanel;         // The Options panel
    public GameObject settingsPanel;        // The Settings panel inside Options
    public GameObject exitPanel;  
    public Image toggleBackground;
    public GameObject shopPanel;
public Sprite onSprite;
public Sprite offSprite;          // The Exit confirmation panel
[Header("Extra Panels")]
public GameObject rulesPanel;
public GameObject noticePanel;
public GameObject creditsPanel;
public GameObject helpPanel;
public GameObject termsPanel;
[Header("Button Sound")]
public AudioSource buttonAudioSource;
public AudioClip buttonClickSound;
[Header("Settings Buttons")]
public Button rulesButton;
public Button tutorialButton;
public Button noticeButton;
public Button creditsButton;
public Button helpButton;
public Button termsButton;
[Header("Log out-sign in")]
public Button logoutButton;
public Button signInButton;
public Button popupYesButton;
public Button popupNoButton;
public Button popupCloseButton;
public Button backButton;
[Header("Main Screen Extra Buttons")]
public Button profileButton;
public Button pencilButton;
public Button flagButton;

public Button bottomButton1;
public Button bottomButton2;
public Button bottomButton3;
public Button bottomButton4;
[Header("Main Menu Music")]
public AudioSource backgroundMusic;

[Header("BG Volume")]
public UnityEngine.UI.Slider bgSlider;
public TMPro.TMP_Text bgVolumeText;
[Header("Bottom Bar Panels")]
public GameObject creditPanel;
public GameObject helpSupportPanel;
    public void Start()
    {
        // Attach listeners
        topBarOptionsButton.onClick.AddListener(OpenOptionsPanel);
        optionsSettingsButton.onClick.AddListener(OpenSettingsPanel);
        playVsPlayButton.onClick.AddListener(OpenWarScene);
        playVsComputerButton.onClick.AddListener(OpenWarBotScene);
       playWithFriendsButton.onClick.AddListener(PlayButtonSound);
        leaveButton.onClick.AddListener(OpenExitPanel);
        exitYesButton.onClick.AddListener(ExitGame);
        exitNoButton.onClick.AddListener(CloseExitPanel);
bottomButton1.onClick.AddListener(OpenCreditPanel);
bottomButton2.onClick.AddListener(OpenHelpPanel);
bottomButton4.onClick.AddListener(OpenSettingsPanel);
        // Initial states
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (exitPanel != null) exitPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

    bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
soundToggle.isOn = isSoundOn;
soundToggle.onValueChanged.AddListener(OnSoundToggle);

bgSlider.minValue = 0;
bgSlider.maxValue = 10;
float savedVol = PlayerPrefs.GetFloat("BGVolume", 0f);
bgSlider.value = savedVol;
backgroundMusic.volume = savedVol / 10f;
bgVolumeText.text = Mathf.RoundToInt(savedVol).ToString();
bgSlider.onValueChanged.AddListener(OnBGSliderChanged);

if (backgroundMusic != null)
{
    backgroundMusic.loop = true;
    backgroundMusic.Play();
}
topBarOptionsButton.onClick.AddListener(PlayButtonSound);
optionsSettingsButton.onClick.AddListener(PlayButtonSound);

leaveButton.onClick.AddListener(PlayButtonSound);
exitYesButton.onClick.AddListener(PlayButtonSound);
exitNoButton.onClick.AddListener(PlayButtonSound);
rulesButton.onClick.AddListener(PlayButtonSound);
tutorialButton.onClick.AddListener(PlayButtonSound);
noticeButton.onClick.AddListener(PlayButtonSound);
creditsButton.onClick.AddListener(PlayButtonSound);
helpButton.onClick.AddListener(PlayButtonSound);
termsButton.onClick.AddListener(PlayButtonSound);
logoutButton.onClick.AddListener(PlayButtonSound);
signInButton.onClick.AddListener(PlayButtonSound);

popupYesButton.onClick.AddListener(PlayButtonSound);
popupNoButton.onClick.AddListener(PlayButtonSound);
popupCloseButton.onClick.AddListener(PlayButtonSound);

backButton.onClick.AddListener(PlayButtonSound);
profileButton.onClick.AddListener(PlayButtonSound);
pencilButton.onClick.AddListener(PlayButtonSound);
flagButton.onClick.AddListener(PlayButtonSound);

bottomButton1.onClick.AddListener(PlayButtonSound);
bottomButton2.onClick.AddListener(PlayButtonSound);
bottomButton3.onClick.AddListener(PlayButtonSound);
bottomButton4.onClick.AddListener(PlayButtonSound);

if (shopButton != null)
    shopButton.onClick.AddListener(PlayButtonSound);

if (Creditbutton != null)
    Creditbutton.onClick.AddListener(PlayButtonSound);

if (done != null)
    done.onClick.AddListener(PlayButtonSound);

    
    }
    void OnEnable()
{
    if (backgroundMusic != null && !backgroundMusic.isPlaying)
        backgroundMusic.Play();
}

void OnDisable()
{
    if (backgroundMusic != null)
        backgroundMusic.Stop();
}
public void OnSoundToggle(bool isOn)
{
       // Audio
 float bgVol = backgroundMusic != null ? backgroundMusic.volume : 0f;
    AudioListener.volume = isOn ? 1f : 0f;
    if (backgroundMusic != null)
        backgroundMusic.volume = bgVol;
    PlayerPrefs.SetInt("Sound", isOn ? 1 : 0);
    PlayerPrefs.Save();
    toggleBackground.sprite = isOn ? onSprite : offSprite;
}
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {if (creditPanel.activeSelf)
    {
        creditPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        return;
    }
    else if (helpSupportPanel.activeSelf)
    {
        helpSupportPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        return;
    }
            if (rulesPanel.activeSelf)
{
    rulesPanel.SetActive(false);
    settingsPanel.SetActive(true);
        return;
}

else if (noticePanel.activeSelf)
{
    noticePanel.SetActive(false);
    settingsPanel.SetActive(true);
    return;
}
else if (creditsPanel.activeSelf)
{
    creditsPanel.SetActive(false);
    settingsPanel.SetActive(true);
        return;
}
else if (helpPanel.activeSelf)
{
    helpPanel.SetActive(false);
    settingsPanel.SetActive(true);
        return;
}
else if (termsPanel.activeSelf)
{
    termsPanel.SetActive(false);
    settingsPanel.SetActive(true);
        return;
}
            if (settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                optionsPanel.SetActive(true);   // back to Options
            }
            else if (optionsPanel.activeSelf)
            {
                optionsPanel.SetActive(false);
                mainMenuPanel.SetActive(true);  // back to Main Menu
            }
            else if (exitPanel.activeSelf)
            {
                exitPanel.SetActive(false);
                mainMenuPanel.SetActive(true);  // back to Main Menu
            }
            else
            {
                return; // ensure Main Menu always comes back
            }
        }
    }

    public void OpenOptionsPanel()
    {
        optionsPanel.SetActive(true);
     
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void OpenWarScene()
    {
           PlayButtonSound();
            Invoke(nameof(LoadWarScene), 0.3f);
    }
    void LoadWarScene()
{
    SceneManager.LoadScene("Warscene");
}
public void BackFromSettings()
{
    settingsPanel.SetActive(false);
    mainMenuPanel.SetActive(true);
}
    public void OpenWarBotScene()
    {
         PlayButtonSound();
            Invoke(nameof(LoadWarBotScene), 0.3f);
    }
void LoadWarBotScene()
{
    SceneManager.LoadScene("War Bot");
}
   
    public void OpenExitPanel()
    {
        exitPanel.SetActive(true);
        optionsPanel.SetActive(false);

       
    }

    public void CloseExitPanel()
    {
        exitPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
public void OpenShopPanel()
{
    shopPanel.SetActive(true);
   // optional
}
public void OpenDonw()
{
    shopPanel.SetActive(false);
   // optional
}
public void OpenRules()
{
    settingsPanel.SetActive(false);
    rulesPanel.SetActive(true);
}

public void OpenNoticePanel()
{
    settingsPanel.SetActive(false);
    noticePanel.SetActive(true);
}

public void OpenCredits()
{
    settingsPanel.SetActive(false);
    creditsPanel.SetActive(true);
}

public void OpenHelp()
{
    settingsPanel.SetActive(false);
    helpPanel.SetActive(true);
}

public void OpenTerms()
{
    settingsPanel.SetActive(false);
    termsPanel.SetActive(true);
}
public void OpenTutorial()
{
    SceneManager.LoadScene("tutorial");
}
void PlayButtonSound()
{
    if (buttonAudioSource != null && buttonClickSound != null)
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
    }
}
void OnBGSliderChanged(float value)
{
    if (backgroundMusic != null)
        backgroundMusic.volume = value / 10f;
    bgVolumeText.text = Mathf.RoundToInt(value).ToString();
    PlayerPrefs.SetFloat("BGVolume", value);
    PlayerPrefs.Save();
}
public void OpenCreditPanel()
{
    creditPanel.SetActive(true);
    mainMenuPanel.SetActive(false);
}

public void CloseCreditPanel()
{
    creditPanel.SetActive(false);
    mainMenuPanel.SetActive(true);
}

public void OpenHelpPanel()
{
    helpSupportPanel.SetActive(true);
    mainMenuPanel.SetActive(false);
}

public void CloseHelpPanel()
{
    helpSupportPanel.SetActive(false);
    mainMenuPanel.SetActive(true);
}
}
