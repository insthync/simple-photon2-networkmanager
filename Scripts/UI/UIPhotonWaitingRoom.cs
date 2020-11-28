using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using PlayerState = SimplePhotonNetworkManager.PlayerState;

public class UIPhotonWaitingRoom : UIBase
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
    public UIPhotonWaitingPlayer waitingPlayerPrefab;
    public GameObject waitingPlayerListRoot;
    public Transform waitingPlayerListContainer;
    public GameObject waitingPlayerTeamAListRoot;
    public Transform waitingPlayerTeamAListContainer;
    public GameObject waitingPlayerTeamBListRoot;
    public Transform waitingPlayerTeamBListContainer;
    public GameObject[] hostObjects;
    public GameObject[] nonHostObjects;
    public bool hostAlwaysReady = true;
    public int autoStartWhenPlayersReadyAtLeast = 0;
    public int canStartWhenPlayersReadyAtLeast = 0;
    public string HostPlayerID { get; private set; }

    private readonly Dictionary<string, Player> players = new Dictionary<string, Player>();
    private readonly Dictionary<string, UIPhotonWaitingPlayer> waitingPlayers = new Dictionary<string, UIPhotonWaitingPlayer>();
    private readonly Dictionary<string, UIPhotonWaitingPlayer> waitingTeamAPlayers = new Dictionary<string, UIPhotonWaitingPlayer>();
    private readonly Dictionary<string, UIPhotonWaitingPlayer> waitingTeamBPlayers = new Dictionary<string, UIPhotonWaitingPlayer>();


    private void OnEnable()
    {
        SimplePhotonNetworkManager.onJoinedRoom += OnJoinedRoomCallback;
        SimplePhotonNetworkManager.onPlayerConnected += OnPlayerConnectedCallback;
        SimplePhotonNetworkManager.onPlayerDisconnected += OnPlayerDisconnectedCallback;
        SimplePhotonNetworkManager.onPlayerPropertiesChanged += OnPlayerPropertiesChangedCallback;
        SimplePhotonNetworkManager.onCustomRoomPropertiesChanged += OnCustomRoomPropertiesChangedCallback;
        SimplePhotonNetworkManager.onMasterClientSwitched += OnMasterClientSwitchedCallback;
        OnJoinedRoomCallback();
    }

    private void OnDisable()
    {
        SimplePhotonNetworkManager.onJoinedRoom -= OnJoinedRoomCallback;
        SimplePhotonNetworkManager.onPlayerConnected -= OnPlayerConnectedCallback;
        SimplePhotonNetworkManager.onPlayerDisconnected -= OnPlayerDisconnectedCallback;
        SimplePhotonNetworkManager.onPlayerPropertiesChanged -= OnPlayerPropertiesChangedCallback;
        SimplePhotonNetworkManager.onCustomRoomPropertiesChanged -= OnCustomRoomPropertiesChangedCallback;
        SimplePhotonNetworkManager.onMasterClientSwitched -= OnMasterClientSwitchedCallback;
    }

    private void UpdateRoomData()
    {
        var room = PhotonNetwork.CurrentRoom;
        var customProperties = room.CustomProperties;
        object tempObj;
        // Room name
        string roomName = string.Empty;
        if (customProperties.TryGetValue(SimplePhotonNetworkManager.CUSTOM_ROOM_ROOM_NAME, out tempObj))
            roomName = (string)tempObj;
        // Player Id
        string playerId = string.Empty;
        if (customProperties.TryGetValue(SimplePhotonNetworkManager.CUSTOM_ROOM_PLAYER_ID, out tempObj))
            playerId = (string)tempObj;
        // Player Name
        string playerName = string.Empty;
        if (customProperties.TryGetValue(SimplePhotonNetworkManager.CUSTOM_ROOM_PLAYER_NAME, out tempObj))
            playerName = (string)tempObj;
        // Room Scene Name
        string sceneName = string.Empty;
        if (customProperties.TryGetValue(SimplePhotonNetworkManager.CUSTOM_ROOM_SCENE_NAME, out tempObj))
            sceneName = (string)tempObj;
        // Room State
        byte state = 0;
        if (customProperties.TryGetValue(SimplePhotonNetworkManager.CUSTOM_ROOM_STATE, out tempObj))
            state = (byte)tempObj;

        if (textName != null)
            textName.text = room.Name;
        if (textRoomName != null)
            textRoomName.text = string.IsNullOrEmpty(roomName) ? "Untitled" : roomName;
        if (textPlayerName != null)
            textPlayerName.text = playerName;
        if (textSceneName != null)
            textSceneName.text = BaseNetworkGameInstance.GetMapNameByScene(sceneName);
        if (textPlayerCount != null)
            textPlayerCount.text = room.PlayerCount + "/" + room.MaxPlayers;
        if (textRoomState != null)
        {
            switch ((SimplePhotonNetworkManager.RoomState)state)
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
        if (customProperties.TryGetValue(BaseNetworkGameManager.CUSTOM_ROOM_GAME_RULE, out tempObj) &&
            BaseNetworkGameInstance.GameRules.TryGetValue((string)tempObj, out gameRule))
        {
            if (textGameRule != null)
                textGameRule.text = gameRule == null ? "Unknow" : gameRule.Title;

            waitingPlayerListRoot.SetActive(!gameRule.IsTeamGameplay);
            waitingPlayerTeamAListRoot.SetActive(gameRule.IsTeamGameplay);
            waitingPlayerTeamBListRoot.SetActive(gameRule.IsTeamGameplay);
        }
        
        if (textBotCount != null &&
            customProperties.TryGetValue(BaseNetworkGameManager.CUSTOM_ROOM_GAME_RULE_BOT_COUNT, out tempObj))
        {
            textBotCount.text = ((int)tempObj).ToString("N0");
            textBotCount.gameObject.SetActive(gameRule != null && gameRule.HasOptionBotCount);
        }
        
        if (textMatchTime != null &&
            customProperties.TryGetValue(BaseNetworkGameManager.CUSTOM_ROOM_GAME_RULE_MATCH_TIME, out tempObj))
        {
            textMatchTime.text = ((int)tempObj).ToString("N0");
            textMatchTime.gameObject.SetActive(gameRule != null && gameRule.HasOptionMatchTime);
        }
        
        if (textMatchKill != null &&
            customProperties.TryGetValue(BaseNetworkGameManager.CUSTOM_ROOM_GAME_RULE_MATCH_KILL, out tempObj))
        {
            textMatchKill.text = ((int)tempObj).ToString("N0");
            textMatchKill.gameObject.SetActive(gameRule != null && gameRule.HasOptionMatchKill);
        }
        
        if (textMatchScore != null &&
            customProperties.TryGetValue(BaseNetworkGameManager.CUSTOM_ROOM_GAME_RULE_MATCH_SCORE, out tempObj))
        {
            textMatchScore.text = ((int)tempObj).ToString("N0");
            textMatchScore.gameObject.SetActive(gameRule != null && gameRule.HasOptionMatchScore);
        }

        HostPlayerID = playerId;
    }

    public virtual void OnClickLeaveRoom()
    {
        SimplePhotonNetworkManager.Singleton.LeaveRoom();
    }

    public virtual void OnClickStartGame()
    {
        if (SimplePhotonNetworkManager.Singleton.CountPlayerWithState(PlayerState.Ready) >= canStartWhenPlayersReadyAtLeast)
            SimplePhotonNetworkManager.Singleton.StartGame();
    }

    public virtual void OnClickReady()
    {
        SimplePhotonNetworkManager.Singleton.TogglePlayerState();
    }

    public virtual void OnClickChangeTeam()
    {
        BaseNetworkGameManager.Singleton.ChangePlayerTeam();
    }

    private void OnJoinedRoomCallback()
    {
        UpdateRoomData();
        string key = PhotonNetwork.LocalPlayer.UserId;
        if (string.IsNullOrEmpty(key))
            key = SimplePhotonNetworkManager.OFFLINE_USER_ID;
        // Set waiting player list
        for (var i = waitingPlayerListContainer.childCount - 1; i >= 0; --i)
        {
            var child = waitingPlayerListContainer.GetChild(i);
            Destroy(child.gameObject);
        }

        for (var i = waitingPlayerTeamAListContainer.childCount - 1; i >= 0; --i)
        {
            var child = waitingPlayerTeamAListContainer.GetChild(i);
            Destroy(child.gameObject);
        }

        for (var i = waitingPlayerTeamBListContainer.childCount - 1; i >= 0; --i)
        {
            var child = waitingPlayerTeamBListContainer.GetChild(i);
            Destroy(child.gameObject);
        }
        players.Clear();
        waitingPlayers.Clear();
        waitingTeamAPlayers.Clear();
        waitingTeamBPlayers.Clear();
        foreach (Player data in PhotonNetwork.PlayerList)
        {
            CreatePlayerUI(data);
        }
        foreach (var hostObject in hostObjects)
        {
            hostObject.SetActive(HostPlayerID == key);
        }
        foreach (var nonHostObject in nonHostObjects)
        {
            nonHostObject.SetActive(HostPlayerID != key);
        }
        if (PhotonNetwork.LocalPlayer.IsMasterClient && hostAlwaysReady)
            SimplePhotonNetworkManager.Singleton.SetPlayerState(PlayerState.Ready);
    }

    private void DestroyPlayerUI(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;
        if (waitingPlayers.ContainsKey(id))
        {
            Destroy(waitingPlayers[id].gameObject);
            waitingPlayers.Remove(id);
        }
        if (waitingTeamAPlayers.ContainsKey(id))
        {
            Destroy(waitingTeamAPlayers[id].gameObject);
            waitingTeamAPlayers.Remove(id);
        }
        if (waitingTeamBPlayers.ContainsKey(id))
        {
            Destroy(waitingTeamBPlayers[id].gameObject);
            waitingTeamBPlayers.Remove(id);
        }
        players.Remove(id);
    }

    private void CreatePlayerUI(Player player)
    {
        string key = player.UserId;
        if (string.IsNullOrEmpty(key))
            key = SimplePhotonNetworkManager.OFFLINE_USER_ID;
        DestroyPlayerUI(key);

        byte team = SimplePhotonNetworkManager.Singleton.GetTeam(player);
        Transform container = waitingPlayerListContainer;
        Dictionary<string, UIPhotonWaitingPlayer> uiDict = waitingPlayers;
        switch (team)
        {
            case 1:
                container = waitingPlayerTeamAListContainer;
                uiDict = waitingTeamAPlayers;
                break;
            case 2:
                container = waitingPlayerTeamBListContainer;
                uiDict = waitingTeamBPlayers;
                break;
        }
        UIPhotonWaitingPlayer newEntry = Instantiate(waitingPlayerPrefab, container);
        newEntry.SetData(this, player);
        newEntry.gameObject.SetActive(true);
        uiDict.Add(key, newEntry);

        players[key] = player;
    }

    private void UpdatePlayerUI(Player player)
    {
        byte team = SimplePhotonNetworkManager.Singleton.GetTeam(player);
        string key = player.UserId;
        if (string.IsNullOrEmpty(key))
            key = SimplePhotonNetworkManager.OFFLINE_USER_ID;
        if (waitingPlayers.ContainsKey(key))
        {
            if (team != 0)
            {
                // If player team changed, recreate waiting player UI
                CreatePlayerUI(player);
                return;
            }
            waitingPlayers[key].SetData(this, player);
        }
        if (waitingTeamAPlayers.ContainsKey(key))
        {
            if (team == 0 || team != 1)
            {
                // If player team changed, recreate waiting player UI
                CreatePlayerUI(player);
                return;
            }
            waitingTeamAPlayers[key].SetData(this, player);
        }
        if (waitingTeamBPlayers.ContainsKey(key))
        {
            if (team == 0 || team != 2)
            {
                // If player team changed, recreate waiting player UI
                CreatePlayerUI(player);
                return;
            }
            waitingTeamBPlayers[key].SetData(this, player);
        }
    }

    private void OnPlayerConnectedCallback(Player player)
    {
        UpdateRoomData();
        CreatePlayerUI(player);
    }

    private void OnPlayerDisconnectedCallback(Player player)
    {
        UpdateRoomData();
        string key = player.UserId;
        if (string.IsNullOrEmpty(key))
            key = SimplePhotonNetworkManager.OFFLINE_USER_ID;
        DestroyPlayerUI(key);
    }

    private void OnPlayerPropertiesChangedCallback(Player player, Hashtable props)
    {
        string key = player.UserId;
        if (string.IsNullOrEmpty(key))
            key = SimplePhotonNetworkManager.OFFLINE_USER_ID;
        if (players.ContainsKey(key))
            UpdatePlayerUI(player);
        else
            CreatePlayerUI(player);

        if (PhotonNetwork.IsMasterClient && autoStartWhenPlayersReadyAtLeast > 0 &&
            SimplePhotonNetworkManager.Singleton.CountPlayerWithState(PlayerState.Ready) >= autoStartWhenPlayersReadyAtLeast)
        {
            // Start game automatically when ready player reached `autoStartGameWhenPlayersReady` amount
            OnClickStartGame();
        }
    }

    private void OnCustomRoomPropertiesChangedCallback(Hashtable propertiesThatChanged)
    {
        var room = PhotonNetwork.CurrentRoom;
        var customProperties = room.CustomProperties;
        var playerId = (string)customProperties[SimplePhotonNetworkManager.CUSTOM_ROOM_PLAYER_ID];
        if (playerId != HostPlayerID)
        {
            // Update with `OnJoinedRoomCallback` to refresh all data
            OnJoinedRoomCallback();
        }
        else
            UpdateRoomData();
    }

    private void OnMasterClientSwitchedCallback(Player player)
    {
        if (hostAlwaysReady && player.IsLocal)
            SimplePhotonNetworkManager.Singleton.SetPlayerState(PlayerState.Ready);
    }
}
