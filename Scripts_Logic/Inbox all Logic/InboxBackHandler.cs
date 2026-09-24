using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InboxBackHandler : MonoBehaviour
{
  [Header("Main Panels")]
    public GameObject mainInboxPanel;
    public GameObject invitePanel;

    [Header("Other Panels")]
    public GameObject requestsPanel;
    public GameObject friendsPanel;
    public GameObject addFriendPanel;

    [Header("Inner Panels")]
    public GameObject profilePanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ===== CLOSE PROFILE PANEL FIRST =====
            if (profilePanel != null && profilePanel.activeSelf)
            {
                profilePanel.SetActive(false);
                return;
            }

            // ===== IF INVITE PANEL OPEN → CLOSE WHOLE INBOX =====
            if (invitePanel != null && invitePanel.activeSelf)
            {
                mainInboxPanel.SetActive(false);
                return;
            }

            // ===== OTHER 3 PANELS → GO BACK TO INVITE PANEL =====
            if (requestsPanel != null && requestsPanel.activeSelf)
            {
                requestsPanel.SetActive(false);
                invitePanel.SetActive(true);
                return;
            }

            if (friendsPanel != null && friendsPanel.activeSelf)
            {
                friendsPanel.SetActive(false);
                invitePanel.SetActive(true);
                return;
            }

            if (addFriendPanel != null && addFriendPanel.activeSelf)
            {
                addFriendPanel.SetActive(false);
                invitePanel.SetActive(true);
                return;
            }
        }
    }
}
