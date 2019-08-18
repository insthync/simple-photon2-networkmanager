using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;

public class UINetworkLoading : MonoBehaviour
{
    [System.Serializable]
    public class ErrorEvent : UnityEvent<string> { }

    public static UINetworkLoading Singleton { get; private set; }
    public GameObject connectingObject;
    public GameObject joiningLobbyObject;
    public GameObject joiningRoomObject;

    public UnityEvent onConnectingToMaster;
    public UnityEvent onConnectedToMaster;
    public UnityEvent onDisconnected;
    public UnityEvent onJoiningLobby;
    public UnityEvent onJoinedLobby;
    public UnityEvent onJoiningRoom;
    public UnityEvent onJoinedRoom;
    public ErrorEvent onConnectionError;
    public ErrorEvent onRoomConnectError;

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Singleton = this;
        SimplePhotonNetworkManager.onConnectingToMaster += OnConnectingToMaster;
        SimplePhotonNetworkManager.onConnectedToMaster += OnConnectedToMaster;
        SimplePhotonNetworkManager.onDisconnected += OnDisconnected;
        SimplePhotonNetworkManager.onJoiningLobby += OnJoiningLobby;
        SimplePhotonNetworkManager.onJoiningRoom += OnJoiningRoom;
        SimplePhotonNetworkManager.onJoinedLobby += OnJoinedLobby;
        SimplePhotonNetworkManager.onJoinedRoom += OnJoinedRoom;
        SimplePhotonNetworkManager.onConnectionError += OnConnectionError;
        SimplePhotonNetworkManager.onRoomConnectError += OnRoomConnectError;
    }

    public void OnConnectingToMaster()
    {
        if (connectingObject != null)
            connectingObject.SetActive(true);
        onConnectingToMaster.Invoke();
    }

    public void OnConnectedToMaster()
    {
        if (connectingObject != null)
            connectingObject.SetActive(false);
        onConnectedToMaster.Invoke();
    }

    public void OnDisconnected()
    {
        if (connectingObject != null)
            connectingObject.SetActive(false);

        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(false);

        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(false);

        onDisconnected.Invoke();
    }

    public void OnJoiningLobby()
    {
        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(true);
        onJoiningLobby.Invoke();
    }

    public void OnJoiningRoom()
    {
        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(true);
        onJoiningRoom.Invoke();
    }

    public void OnJoinedLobby()
    {
        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(false);
        onJoinedLobby.Invoke();
    }

    public void OnJoinedRoom()
    {
        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(false);
        onJoinedRoom.Invoke();
    }

    public void OnConnectionError(DisconnectCause error)
    {
        if (connectingObject != null)
            connectingObject.SetActive(false);

        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(false);

        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(false);

        onConnectionError.Invoke(error.ToString());
    }

    public void OnRoomConnectError(short code, string msg)
    {
        if (connectingObject != null)
            connectingObject.SetActive(false);

        if (joiningLobbyObject != null)
            joiningLobbyObject.SetActive(false);

        if (joiningRoomObject != null)
            joiningRoomObject.SetActive(false);

        onRoomConnectError.Invoke(msg);
    }
}
