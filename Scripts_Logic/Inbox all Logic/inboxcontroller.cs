using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class inboxcontroller : MonoBehaviour
{
      public GameObject inviteFrame;
    public GameObject inviteDarkIcon;

    public GameObject inboxFrame;
    public GameObject messageIcon;

    public Image inviteButtonImage;
    public Image inboxButtonImage;


public GameObject inbox_main;
public GameObject invite;
public GameObject sub_inbox;
public GameObject notification;
public GameObject addfriend;
public GameObject chat;
public GameObject profile;

public GameObject add_friend_image;
public GameObject add_friend_tick;

public GameObject noticePanel;
public GameObject connectNetworkPanel;
public Button inboxOpenButton;
public GameObject signInPopUp;
[Header("Top Button Sound")]
public AudioSource buttonAudioSource;
public AudioClip buttonClickSound;

public Button inviteTopButton;
public Button inboxTopButton;
public Button bellButton;
public Button addFriendButton;
    void Start()
    {
     FriendManager.Instance.StartBadgeListener();
     inviteTopButton.onClick.AddListener(PlayButtonSound);
inboxTopButton.onClick.AddListener(PlayButtonSound);
bellButton.onClick.AddListener(PlayButtonSound);
addFriendButton.onClick.AddListener(PlayButtonSound);
    }
    void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        // ===== PROFILE → CLOSE ONLY PROFILE =====
        if (profile.activeSelf)
        {
            profile.SetActive(false);
            ShowInvite();
            return;
        }

        // ===== NOTIFICATION → GO TO INVITE =====
        if (notification.activeSelf)
        {
            ShowInvite();
            return;
        }

        // ===== ADD FRIEND → GO TO INVITE =====
        if (addfriend.activeSelf)
        {
            ShowInvite();
            return;
        }

        // ===== CHAT → GO TO INVITE =====
        if (chat.activeSelf)
        {
            ShowInvite();
            return;
        }
if (sub_inbox.activeSelf)
{
    ShowInvite();
    return;
}
        // ===== INVITE PANEL → CLOSE WHOLE INBOX =====
      if (invite.activeSelf)
{
    inbox_main.SetActive(false);
    HideAll();
    return;
}
    }
}
public void main_inbox()

{
    inbox_main.SetActive(true);
    ShowInvite();
}
    public void ShowInvite()
    {
        inviteFrame.SetActive(true);
        inviteDarkIcon.SetActive(true);

        inboxFrame.SetActive(false);
        messageIcon.SetActive(false);
invite.SetActive(true);

sub_inbox.SetActive(false);
notification.SetActive(false);
addfriend.SetActive(false);
chat.SetActive(false);
profile.SetActive(false);
        SetAlpha(inviteButtonImage, 52f);
        SetAlpha(inboxButtonImage, 0f);
        FriendManager.Instance.StopRequestListener();
        FriendManager.Instance.StopFriendsListener();
        FriendManager.Instance.LoadInviteFriends();
    }

    public void ShowInbox()
    {
        inviteFrame.SetActive(false);
        inviteDarkIcon.SetActive(false);

        inboxFrame.SetActive(true);
        messageIcon.SetActive(true);
        invite.SetActive(false);
sub_inbox.SetActive(true);
notification.SetActive(false);
addfriend.SetActive(false);
chat.SetActive(false);
profile.SetActive(false);
        SetAlpha(inviteButtonImage, 0f);
        SetAlpha(inboxButtonImage, 52f);
        FriendManager.Instance.StopRequestListener();
        FriendManager.Instance.LoadFriends();
        FriendManager.Instance.StopInviteListener();
    }

    public void HideAll()
    {
        inviteFrame.SetActive(false);
        inviteDarkIcon.SetActive(false);

        inboxFrame.SetActive(false);
        messageIcon.SetActive(false);

        SetAlpha(inviteButtonImage, 0f);
        SetAlpha(inboxButtonImage, 0f);
        FriendManager.Instance.StopRequestListener();
        FriendManager.Instance.StopFriendsListener();
        FriendManager.Instance.StopInviteListener();
    }

    // ✅ MUST BE INSIDE CLASS (this was your error)
    void SetAlpha(Image img, float alphaValue)
    {
        Color c = img.color;
        c.a = alphaValue / 255f;
        img.color = c;
    }
    public void OnBellClicked()
{
    // Disable frames
    inviteFrame.SetActive(false);
    inboxFrame.SetActive(false);

    // Disable icons
    inviteDarkIcon.SetActive(false);
    messageIcon.SetActive(false);
       invite.SetActive(false);
sub_inbox.SetActive(false);
notification.SetActive(true);
addfriend.SetActive(false);
chat.SetActive(false);
profile.SetActive(false);
    // Reset alpha
    SetAlpha(inviteButtonImage, 0f);
    SetAlpha(inboxButtonImage, 0f);
      FriendManager.Instance.LoadFriendRequests();
      FriendManager.Instance.StopFriendsListener();
    FriendManager.Instance.StopInviteListener();

    // (Later you can open notification panel here)
}
public void OnAddFriendClicked()
{
    // Disable frames
    inviteFrame.SetActive(false);
    inboxFrame.SetActive(false);

    // Disable icons
    inviteDarkIcon.SetActive(false);
    messageIcon.SetActive(false);
     invite.SetActive(false);
sub_inbox.SetActive(false);
notification.SetActive(false);
addfriend.SetActive(true);
chat.SetActive(false);
profile.SetActive(false);
    // Reset alpha
    // Reset alpha
    SetAlpha(inviteButtonImage, 0f);
    SetAlpha(inboxButtonImage, 0f);

    // (Later you can open add friend panel here)
        FriendManager.Instance.ResetAddFriendPanel();
        FriendManager.Instance.StopRequestListener();
        FriendManager.Instance.StopFriendsListener();
        FriendManager.Instance.StopInviteListener();
}
public void chat_menu()
{
      inviteFrame.SetActive(false);
    inboxFrame.SetActive(false);

    // Disable icons
    inviteDarkIcon.SetActive(false);
    messageIcon.SetActive(false);
     invite.SetActive(false);
sub_inbox.SetActive(false);
notification.SetActive(false);
addfriend.SetActive(false);
chat.SetActive(true);
profile.SetActive(false);
FriendManager.Instance.StopRequestListener();
FriendManager.Instance.StopFriendsListener();
FriendManager.Instance.StopInviteListener();
}
public void openprofie()
{
         inviteFrame.SetActive(false);
    inboxFrame.SetActive(false);

    // Disable icons
    inviteDarkIcon.SetActive(false);
    messageIcon.SetActive(false);
     invite.SetActive(false);
sub_inbox.SetActive(false);
notification.SetActive(false);
addfriend.SetActive(false);
chat.SetActive(false);
profile.SetActive(true);
FriendManager.Instance.StopRequestListener();
FriendManager.Instance.StopFriendsListener();
FriendManager.Instance.StopInviteListener();
}
public void add_friend_button()
{
     add_friend_image.SetActive(false);
add_friend_tick.SetActive(true);
FriendManager.Instance.StopRequestListener();
FriendManager.Instance.StopFriendsListener();
FriendManager.Instance.StopInviteListener();
}
public void OpenNotice()
{
    if (noticePanel != null)
        noticePanel.SetActive(true);
}
public void CloseNotice()
{
    if (noticePanel != null)
        noticePanel.SetActive(false);

    main_inbox();
}
public void OpenInboxWithInternetCheck()
{
    // ✅ User entered without account
    if (PlayerPrefs.GetString("offlineEntry") == "true")
    {
        if (signInPopUp != null)
        {
            signInPopUp.SetActive(true);

            CancelInvoke(nameof(HideSignInPopup));
            Invoke(nameof(HideSignInPopup), 5f);
        }

        return;
    }

    // ✅ Check internet
    bool isOnline =
        Application.internetReachability != NetworkReachability.NotReachable;

    // ❌ No internet
    if (!isOnline)
    {
        if (connectNetworkPanel != null)
        {
            connectNetworkPanel.SetActive(true);

            CancelInvoke(nameof(HideNetworkPanel));
            Invoke(nameof(HideNetworkPanel), 5f);
        }

        return;
    }

    // ✅ Internet + Signed In
    main_inbox();
}

void HideSignInPopup()
{
    if (signInPopUp != null)
        signInPopUp.SetActive(false);
}
void HideNetworkPanel()
{
    if (connectNetworkPanel != null)
        connectNetworkPanel.SetActive(false);
}
void PlayButtonSound()
{
    if (buttonAudioSource != null && buttonClickSound != null)
    {
        buttonAudioSource.PlayOneShot(buttonClickSound);
    }
}
}











