using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPhotonNetworkingEntry : MonoBehaviour
{
    public Text textName;
    public Text textRoomName;
    public Text textPlayerName;
    public Text textSceneName;
    public Text textPlayerCount;
    public Text textRoomState;
    public string roomStateWaiting = "Waiting";
    public string roomStatePlaying = "Playing";
    public Text textGameRule;
    public Text textBotCount;
    public Text textMatchTime;
    public Text textMatchKill;
    public Text textMatchScore;
    public GameObject hasPasswordObject;
    public NetworkDiscoveryData Data { get; private set; }

    public void SetData(NetworkDiscoveryData data)
    {
        Data = data;
        if (textName != null)
            textName.text = data.name;
        if (textRoomName != null)
            textRoomName.text = string.IsNullOrEmpty(data.roomName) ? "Untitled" : data.roomName;
        if (textPlayerName != null)
            textPlayerName.text = data.playerName;
        if (textSceneName != null)
            textSceneName.text = BaseNetworkGameInstance.GetMapNameByScene(data.sceneName);
        if (textPlayerCount != null)
            textPlayerCount.text = data.numPlayers + "/" + data.maxPlayers;
        if (textRoomState != null)
        {
            switch ((SimplePhotonNetworkManager.RoomState)data.state)
            {
                case SimplePhotonNetworkManager.RoomState.Waiting:
                    textRoomState.text = roomStateWaiting;
                    break;
                case SimplePhotonNetworkManager.RoomState.Playing:
                    textRoomState.text = roomStatePlaying;
                    break;
            }
        }
        
        BaseNetworkGameRule gameRule = null;
        if (textGameRule != null &&
            BaseNetworkGameInstance.GameRules.TryGetValue(data.gameRule, out gameRule))
            textGameRule.text = gameRule == null ? "" : gameRule.Title;

        if (textBotCount != null)
        {
            textBotCount.text = data.botCount.ToString("N0");
            textBotCount.gameObject.SetActive(gameRule != null && gameRule.HasOptionBotCount);
        }

        if (textMatchTime != null)
        {
            textMatchTime.text = data.matchTime.ToString("N0");
            textMatchTime.gameObject.SetActive(gameRule != null && gameRule.HasOptionMatchTime);
        }

        if (textMatchKill != null)
        {
            textMatchKill.text = data.matchKill.ToString("N0");
            textMatchKill.gameObject.SetActive(gameRule != null && gameRule.HasOptionMatchKill);
        }

        if (textMatchScore != null)
        {
            textMatchScore.text = data.matchScore.ToString("N0");
            textMatchScore.gameObject.SetActive(gameRule != null && gameRule.HasOptionMatchScore);
        }

        if (hasPasswordObject != null)
            hasPasswordObject.SetActive(!string.IsNullOrEmpty(data.roomPassword));
    }

    public virtual void OnClickJoinButton()
    {
        SimplePhotonNetworkManager.Singleton.JoinRoom(Data.name);
    }
}
