using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPhotonCurrentRoom : MonoBehaviour
{
    public Text textName;
    public Text textPlayerCount;

    private void Update()
    {
        var room = PhotonNetwork.CurrentRoom;
        if (textName != null)
            textName.text = room.Name;
        if (textPlayerCount != null)
            textPlayerCount.text = room.PlayerCount + "/" + room.MaxPlayers;
    }
}
