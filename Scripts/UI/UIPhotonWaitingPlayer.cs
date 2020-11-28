using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using PlayerState = SimplePhotonNetworkManager.PlayerState;

public class UIPhotonWaitingPlayer : MonoBehaviour
{
    public Text textPlayerName;
    public Text textPlayerState;
    public string playerStateReady = "Ready";
    public string playerStateNotReady = "Not Ready";
    public GameObject[] hostObjects;
    public GameObject[] owningObjects;
    public UIPhotonWaitingRoom Room { get; private set; }
    public Player Player { get; private set; }

    public void SetData(UIPhotonWaitingRoom room, Player player)
    {
        Room = room;
        Player = player;
        PlayerState state = (PlayerState)SimplePhotonNetworkManager.Singleton.GetRoomPlayerProperty(SimplePhotonNetworkManager.CUSTOM_PLAYER_STATE, player, (byte)PlayerState.NotReady);

        if (textPlayerName != null)
            textPlayerName.text = player.NickName;

        if (textPlayerState != null)
        {
            switch (state)
            {
                case PlayerState.Ready:
                    textPlayerState.text = playerStateReady;
                    break;
                case PlayerState.NotReady:
                    textPlayerState.text = playerStateNotReady;
                    break;
            }
        }

        foreach (var hostObject in hostObjects)
        {
            hostObject.SetActive(room.HostPlayerID == player.UserId);
        }

        foreach (var owningObject in owningObjects)
        {
            owningObject.SetActive(PhotonNetwork.LocalPlayer.UserId == player.UserId);
        }
    }
}
