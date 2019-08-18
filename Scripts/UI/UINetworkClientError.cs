using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Realtime;

public class UINetworkClientError : MonoBehaviour
{
    [System.Serializable]
    public class ErrorEvent : UnityEvent<string> { }

    public static UINetworkClientError Singleton { get; private set; }
    public UIMessageDialog messageDialog;
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
        SimplePhotonNetworkManager.onConnectionError += OnConnectionError;
        SimplePhotonNetworkManager.onRoomConnectError += OnRoomConnectError;
    }

    public void OnConnectionError(DisconnectCause error)
    {
        if (messageDialog == null)
            return;

        messageDialog.Show(error.ToString());
        onConnectionError.Invoke(error.ToString());
    }

    public void OnRoomConnectError(short code, string msg)
    {
        if (messageDialog == null)
            return;

        messageDialog.Show(msg);
        onRoomConnectError.Invoke(msg);
    }
}
