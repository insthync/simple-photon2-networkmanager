using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPhotonJoinRoom : UIBase
{
    public InputField roomNameField;

    public void OnClickJoin()
    {
        SimplePhotonNetworkManager.Singleton.JoinRoom(roomNameField.text);
    }
}
