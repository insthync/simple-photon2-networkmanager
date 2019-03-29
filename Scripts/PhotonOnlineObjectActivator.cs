using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonOnlineObjectActivator : MonoBehaviour
{
    public GameObject[] onlineObjects;
    public GameObject[] offlineObjects;
    
    void Update()
    {
        foreach (var obj in onlineObjects)
        {
            obj.SetActive(!PhotonNetwork.OfflineMode);
        }
        foreach (var obj in offlineObjects)
        {
            obj.SetActive(PhotonNetwork.OfflineMode);
        }
    }
}
