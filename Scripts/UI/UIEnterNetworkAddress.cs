using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnterNetworkAddress : UIBase
{
    public InputField inputAddress;
    public InputField inputPort;
    protected override void Awake()
    {
        base.Awake();
        inputPort.contentType = InputField.ContentType.IntegerNumber;
    }

    public virtual void OnClickConnect()
    {
        var networkManager = SimplePhotonNetworkManager.Singleton;
        networkManager.masterAddress = inputAddress.text;
        networkManager.masterPort = int.Parse(inputPort.text);
        networkManager.ConnectToMaster();
    }
}
