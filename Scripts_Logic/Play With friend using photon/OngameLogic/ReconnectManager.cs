using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class ReconnectManager : MonoBehaviourPunCallbacks
{

  public static ReconnectManager Instance;

    float disconnectTime;
    bool isReconnecting = false;
    bool wasInRoom = false;

    const float RECONNECT_LIMIT = 20f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // If disconnected → try reconnect
        if (!PhotonNetwork.IsConnected && !isReconnecting)
        {
            StartReconnect();
        }
    }

    void StartReconnect()
    {
        isReconnecting = true;
        disconnectTime = Time.time;
        wasInRoom = PhotonNetwork.InRoom;

        Debug.Log("⚠️ Disconnected! Trying to reconnect...");
turndown.Instance.enabled = false;
        StartCoroutine(ReconnectRoutine());
    }

    IEnumerator ReconnectRoutine()
    {
        while (Time.time - disconnectTime < RECONNECT_LIMIT)
        {
            if (PhotonNetwork.ReconnectAndRejoin())
            {
                Debug.Log("🔁 Reconnecting to room...");
                yield break;
            }

            yield return new WaitForSeconds(2f);
        }

        // ❌ Failed after 20 sec
        Debug.Log("❌ Reconnect failed after 20s");

        LeaveGame();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("✅ Rejoined Room Successfully");

        isReconnecting = false;

        // Re-enable gameplay
        turndown.Instance.enabled = true;
    }

    void LeaveGame()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        
    }        
}
