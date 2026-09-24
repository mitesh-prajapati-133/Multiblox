using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class GameProfileDisplay : MonoBehaviourPunCallbacks
{
     public static GameProfileDisplay Instance;

    [Header("Player 1 - Host (Bottom Left)")]
    public TMP_Text player1Name;
    public Image player1Avatar;
    public Image player1Flag;

    [Header("Player 2 - Joiner (Top Right)")]
    public TMP_Text player2Name;
    public Image player2Avatar;
    public Image player2Flag;

    [Header("Sprites")]
    public Sprite[] avatarSprites;
    public Sprite[] countryFlags;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
          StartCoroutine(ApplyProfilesDelayed());
    }
    IEnumerator ApplyProfilesDelayed()
{
    yield return new WaitForSeconds(0.5f);
    ApplyProfiles();
}
    void ApplyProfiles()
    {
        var players = PhotonNetwork.PlayerList;

        foreach (var player in players)
        {
            // ActorNumber 1 = host = Player 1 (bottom left)
            // ActorNumber 2 = joiner = Player 2 (top right)
            if (player.ActorNumber == 1)
                ApplyToUI(player, player1Name, player1Avatar, player1Flag);
            else
                ApplyToUI(player, player2Name, player2Avatar, player2Flag);
        }
    }

    void ApplyToUI(Player player, TMP_Text nameText, Image avatarImage, Image flagImage)
    {
        // Name
     

        // Avatar
        if (avatarImage != null && avatarSprites != null)
        {
            int aIndex = 0;
            if (player.CustomProperties.ContainsKey("avatarIndex"))
                aIndex = (int)player.CustomProperties["avatarIndex"];
            if (aIndex < avatarSprites.Length)
                avatarImage.sprite = avatarSprites[aIndex];
        }

        // Flag
        if (flagImage != null && countryFlags != null)
        {
            int cIndex = 0;
            if (player.CustomProperties.ContainsKey("countryIndex"))
                cIndex = (int)player.CustomProperties["countryIndex"];
            if (cIndex < countryFlags.Length)
                flagImage.sprite = countryFlags[cIndex];
        }
        if (nameText != null)
{
    string pName = "";
    if (player.CustomProperties.ContainsKey("playerName"))
        pName = (string)player.CustomProperties["playerName"];
    else if (player.NickName != "")
        pName = player.NickName;
    else
        pName = PlayerPrefs.GetString("playerName", "Player");
    nameText.text = pName;
}
    }

    // Refresh when properties update
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        ApplyProfiles();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        ApplyProfiles();
    }
}
