using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class UINetworkClientError : MonoBehaviour
{
    public static UINetworkClientError Singleton { get; private set; }
    public UIMessageDialog messageDialog;

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
    }

    public void OnRoomConnectError(short code, string msg)
    {
        if (messageDialog == null)
            return;

        messageDialog.Show(code + "\n(" + msg + ")");
    }
}
