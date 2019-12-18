using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIPhotonEnterPassword : UIBase
{
    public UIPhotonNetworkingEntry uiRoomData;
    public InputField inputPassword;
    public UnityEvent onEnterWrongPassword;
    [System.NonSerialized]
    public UIPhotonNetworking uiPhotonNetworking;
    public NetworkDiscoveryData Data { get; private set; }

    public void OnClickJoin()
    {
        var password = inputPassword.text;
        if (password.Equals(Data.roomPassword))
        {
            Hide();
            SimplePhotonNetworkManager.Singleton.JoinRoom(Data.name);
            return;
        }
        if (onEnterWrongPassword != null)
            onEnterWrongPassword.Invoke();
    }

    public void SetData(NetworkDiscoveryData data)
    {
        Data = data;

        if (uiRoomData != null)
            uiRoomData.SetData(data);
    }
}
