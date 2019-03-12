using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class UINetworkLoading : MonoBehaviour
{
    public static UINetworkLoading Singleton { get; private set; }
    public GameObject joiningLobbyObject;
    public GameObject joiningRoomObject;

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Singleton = this;
        SimplePhotonNetworkManager.onJoiningLobby += OnJoiningLobby;
        SimplePhotonNetworkManager.onJoiningRoom += OnJoiningRoom;
        SimplePhotonNetworkManager.onJoinedLobby += OnJoinedLobby;
        SimplePhotonNetworkManager.onJoinedRoom += OnJoinedRoom;
        SimplePhotonNetworkManager.onConnectionError += OnConnectionError;
        SimplePhotonNetworkManager.onRoomConnectError += OnRoomConnectError;
    }

    public void OnJoiningLobby()
    {
        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(true);
    }

    public void OnJoiningRoom()
    {
        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(true);
    }

    public void OnJoinedLobby()
    {
        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(false);
    }

    public void OnJoinedRoom()
    {
        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(false);
    }

    public void OnConnectionError(DisconnectCause error)
    {
        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(false);

        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(false);
    }

    public void OnRoomConnectError(short code, string msg)
    {
        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(false);

        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(false);
    }
}
