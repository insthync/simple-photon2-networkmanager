using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class UIPhotonStats : MonoBehaviour
{
    public string formatConnectedRegion = "Connected to {0}";
    public string formatPing = "{0} ms";
    public string formatCountOfPlayers = "<color=green>ONLINE :</color> {0}";
    public string formatCountOfPlayersInRoom = "<color=green>ONLINE :</color> {0}";
    public string formatCountOfPlayersOnMaster = "<color=green>ONLINE :</color> {0}";
    public string formatCountOfRooms = "<color=green>ROOM :</color> {0}";
    public Text textConnectedRegion;
    public Text textPing;
    public Text textCountOfPlayers;
    public Text textCountOfPlayersInRoom;
    public Text textCountOfPlayersOnMaster;
    public Text textCountOfRooms;

    private void Update()
    {
        if (textConnectedRegion != null)
            textConnectedRegion.text = string.Format(formatConnectedRegion, PhotonNetwork.CloudRegion);

        if (textPing != null)
            textPing.text = string.Format(formatPing, PhotonNetwork.GetPing().ToString("N0"));

        if (textCountOfPlayers != null)
            textCountOfPlayers.text = string.Format(formatCountOfPlayers, PhotonNetwork.CountOfPlayers.ToString("N0"));

        if (textCountOfPlayersInRoom != null)
            textCountOfPlayersInRoom.text = string.Format(formatCountOfPlayersInRoom, PhotonNetwork.CountOfPlayersInRooms.ToString("N0"));

        if (textCountOfPlayersOnMaster != null)
            textCountOfPlayersOnMaster.text = string.Format(formatCountOfPlayersOnMaster, PhotonNetwork.CountOfPlayersOnMaster.ToString("N0"));

        if (textCountOfRooms != null)
            textCountOfRooms.text = string.Format(formatCountOfRooms, PhotonNetwork.CountOfRooms.ToString("N0"));
    }
}
