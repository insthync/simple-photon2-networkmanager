using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class SimplePhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public enum RoomState : byte
    {
        Waiting,
        Playing,
    }

    public enum PlayerState : byte
    {
        NotReady,
        Ready,
    }

    public const int UNIQUE_VIEW_ID = 999;
    public const string CUSTOM_ROOM_ROOM_NAME = "R";
    public const string CUSTOM_ROOM_PLAYER_ID = "Id";
    public const string CUSTOM_ROOM_PLAYER_NAME = "P";
    public const string CUSTOM_ROOM_SCENE_NAME = "S";
    public const string CUSTOM_ROOM_MATCH_MAKE = "MM";
    public const string CUSTOM_ROOM_STATE = "St";
    public const string CUSTOM_PLAYER_STATE = "St";
    public static SimplePhotonNetworkManager Singleton { get; protected set; }
    public static System.Action<List<NetworkDiscoveryData>> onReceivedRoomListUpdate;
    public static System.Action<DisconnectCause> onConnectionError;
    public static System.Action<short, string> onRoomConnectError;
    public static System.Action onConnectingToMaster;
    public static System.Action onConnectedToMaster;
    public static System.Action onJoiningLobby;
    public static System.Action onJoinedLobby;
    public static System.Action onJoiningRoom;
    public static System.Action onJoinedRoom;
    public static System.Action onLeftRoom;
    public static System.Action onDisconnected;
    public static System.Action onMatchMakingStarted;
    public static System.Action onMatchMakingStopped;
    public static System.Action<Player> onPlayerConnected;
    public static System.Action<Player> onPlayerDisconnected;
    public static System.Action<Player, Hashtable> onPlayerPropertiesChanged;
    public static System.Action<Player> onMasterClientSwitched;
    public static System.Action<Hashtable> onCustomRoomPropertiesChanged;

    public bool isLog;
    public SceneNameField offlineScene;
    public SceneNameField onlineScene;
    public GameObject playerPrefab;
    public string gameVersion = "1";
    public string masterAddress = "localhost";
    public int masterPort = 5055;
    public string region;
    public int sendRate = 20;
    public byte maxConnections = 10;
    public byte matchMakingConnections = 2;
    public string roomName;
    public bool autoJoinLobby;
    public SimplePhotonStartPoint[] StartPoints { get; protected set; }
    public bool isConnectOffline { get; protected set; }
    public bool isMatchMaking { get; protected set; }
    public float startMatchMakingTime { get; protected set; }
    private bool startGameOnRoomCreated;

    protected virtual void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(Singleton);
        PhotonNetwork.SendRate = sendRate;
        StartPoints = new SimplePhotonStartPoint[0];
        // Set unique view id
        PhotonView view = GetComponent<PhotonView>();
        if (view == null)
            view = gameObject.AddComponent<PhotonView>();
        view.ViewID = UNIQUE_VIEW_ID;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
    }

    protected virtual void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public virtual void ConnectToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectToMaster(masterAddress, masterPort, PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
        if (onJoiningLobby != null)
            onJoiningLobby.Invoke();
    }

    public virtual void ConnectToBestCloudServer()
    {
        isConnectOffline = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.ConnectToBestCloudServer();
        if (onConnectingToMaster != null)
            onConnectingToMaster.Invoke();
    }

    public virtual void ConnectToRegion()
    {
        isConnectOffline = false;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.ConnectToRegion(region);
        if (onConnectingToMaster != null)
            onConnectingToMaster.Invoke();
    }

    public virtual void PlayOffline()
    {
        isConnectOffline = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        if (onConnectingToMaster != null)
            onConnectingToMaster.Invoke();
    }

    public void CreateRoom()
    {
        if (isConnectOffline)
        {
            PhotonNetwork.OfflineMode = true;
            StartGame();
            return;
        }
        SetupAndCreateRoom();
        startGameOnRoomCreated = true;
        isMatchMaking = false;
    }

    public void CreateWaitingRoom()
    {
        if (isConnectOffline)
        {
            PhotonNetwork.OfflineMode = true;
            StartGame();
            return;
        }
        SetupAndCreateRoom();
        startGameOnRoomCreated = false;
        isMatchMaking = false;
    }

    private void SetupAndCreateRoom()
    {
        var roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = GetCustomRoomPropertiesForLobby();
        roomOptions.MaxPlayers = maxConnections;
        roomOptions.PublishUserId = true;
        PhotonNetwork.CreateRoom(string.Empty, roomOptions, null);
    }

    protected virtual string[] GetCustomRoomPropertiesForLobby()
    {
        return new string[]
        {
            CUSTOM_ROOM_ROOM_NAME,
            CUSTOM_ROOM_PLAYER_ID,
            CUSTOM_ROOM_PLAYER_NAME,
            CUSTOM_ROOM_SCENE_NAME,
            CUSTOM_ROOM_STATE
        };
    }

    public void StartMatchMaking()
    {
        if (isConnectOffline)
        {
            PhotonNetwork.OfflineMode = true;
            StartGame();
            return;
        }
        startGameOnRoomCreated = false;
        isMatchMaking = true;
        Hashtable customProperties = new Hashtable();
        customProperties[CUSTOM_ROOM_MATCH_MAKE] = true;
        PhotonNetwork.JoinRandomRoom(customProperties, 0);
        startMatchMakingTime = Time.unscaledTime;
        if (onMatchMakingStarted != null)
            onMatchMakingStarted.Invoke();
    }

    private void SetupAndCreateMatchMakingRoom()
    {
        var roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = GetCustomRoomPropertiesForMatchMaking();
        roomOptions.MaxPlayers = maxConnections;
        roomOptions.PublishUserId = true;
        roomOptions.IsVisible = false;
        PhotonNetwork.CreateRoom(string.Empty, roomOptions, null);
    }

    protected virtual string[] GetCustomRoomPropertiesForMatchMaking()
    {
        return new string[]
        {
            CUSTOM_ROOM_SCENE_NAME,
            CUSTOM_ROOM_MATCH_MAKE
        };
    }

    public void StopMatchMaking()
    {
        LeaveRoom();
        isMatchMaking = false;
        if (onMatchMakingStopped != null)
            onMatchMakingStopped.Invoke();
    }

    public void SetRoomName(string roomName)
    {
        // If room not created, set data to field to use later
        this.roomName = roomName;
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            Hashtable customProperties = new Hashtable();
            customProperties[CUSTOM_ROOM_ROOM_NAME] = roomName;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
    }

    public void SetMaxConnections(byte maxConnections)
    {
        this.maxConnections = maxConnections;
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.MaxPlayers = maxConnections;
        }
    }

    public void SetRoomOnlineScene(SceneNameField onlineScene)
    {
        // If room not created, set data to field to use later
        this.onlineScene = onlineScene;
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            Hashtable customProperties = new Hashtable();
            customProperties[CUSTOM_ROOM_SCENE_NAME] = onlineScene.SceneName;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        }
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
        if (onJoiningLobby != null)
            onJoiningLobby.Invoke();
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        if (onJoiningRoom != null)
            onJoiningRoom.Invoke();
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
        if (onJoiningRoom != null)
            onJoiningRoom.Invoke();
    }

    public virtual void LeaveRoom()
    {
        if (isConnectOffline)
            PhotonNetwork.Disconnect();
        else
            PhotonNetwork.LeaveRoom();
    }

    public virtual void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        var foundRooms = new List<NetworkDiscoveryData>();
        foreach (var room in rooms)
        {
            var customProperties = room.CustomProperties;
            var discoveryData = new NetworkDiscoveryData();
            discoveryData.name = room.Name;
            discoveryData.roomName = (string)customProperties[CUSTOM_ROOM_ROOM_NAME];
            discoveryData.playerId = (string)customProperties[CUSTOM_ROOM_PLAYER_ID];
            discoveryData.playerName = (string)customProperties[CUSTOM_ROOM_PLAYER_NAME];
            discoveryData.sceneName = (string)customProperties[CUSTOM_ROOM_SCENE_NAME];
            discoveryData.state = (byte)customProperties[CUSTOM_ROOM_STATE];
            discoveryData.numPlayers = room.PlayerCount;
            discoveryData.maxPlayers = room.MaxPlayers;
            foundRooms.Add(discoveryData);
        }
        if (onReceivedRoomListUpdate != null)
            onReceivedRoomListUpdate.Invoke(foundRooms);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (isLog) Debug.Log("OnDisconnected " + cause.ToString());
        switch (cause)
        {
            case DisconnectCause.None:
            case DisconnectCause.DisconnectByClientLogic:
                if (!SceneManager.GetActiveScene().name.Equals(offlineScene.SceneName))
                    SceneManager.LoadScene(offlineScene.SceneName);
                if (onDisconnected != null)
                    onDisconnected.Invoke();
                break;
            default:
                if (onConnectionError != null)
                    onConnectionError.Invoke(cause);
                break;
        }
    }

    public override void OnCreateRoomFailed(short code, string msg)
    {
        if (isLog) Debug.Log("OnCreateRoomFailed " + code + " " + msg);
        if (onRoomConnectError != null)
            onRoomConnectError.Invoke(code, msg);
    }

    public override void OnJoinRandomFailed(short code, string msg)
    {
        if (isLog) Debug.Log("OnJoinRandomFailed " + code + " " + msg);
        if (isMatchMaking)
        {
            SetupAndCreateMatchMakingRoom();
        }
        else
        {
            if (onRoomConnectError != null)
                onRoomConnectError.Invoke(code, msg);
        }
    }

    public override void OnJoinedLobby()
    {
        if (isLog) Debug.Log("OnJoinedLobby");
        if (onJoinedLobby != null)
            onJoinedLobby.Invoke();
    }

    public override void OnCreatedRoom()
    {
        if (isLog) Debug.Log("OnCreatedRoom");
        // Set room information
        Hashtable customProperties = new Hashtable();
        customProperties[CUSTOM_ROOM_ROOM_NAME] = roomName;
        customProperties[CUSTOM_ROOM_PLAYER_ID] = PhotonNetwork.LocalPlayer.UserId;
        customProperties[CUSTOM_ROOM_PLAYER_NAME] = PhotonNetwork.LocalPlayer.NickName;
        customProperties[CUSTOM_ROOM_SCENE_NAME] = onlineScene.SceneName;
        customProperties[CUSTOM_ROOM_STATE] = (byte) RoomState.Waiting;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        if (startGameOnRoomCreated)
            StartGame();
    }

    public void StartGame()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Player not joined room, cannot start game");
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Player is not master client, cannot start game");
            return;
        }

        if (isMatchMaking && PhotonNetwork.CurrentRoom.PlayerCount < matchMakingConnections)
        {
            Debug.LogError("Player is not enough to start game");
            return;
        }

        StartCoroutine(LoadOnlineScene());
    }

    protected IEnumerator LoadOnlineScene()
    {
        PhotonNetwork.LoadLevel(onlineScene.SceneName);
        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            yield return null;
        }
        // Change room state to playing
        Hashtable customProperties = new Hashtable();
        customProperties[CUSTOM_ROOM_STATE] = (byte)RoomState.Playing;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        // Setup start points for master client
        StartPoints = FindObjectsOfType<SimplePhotonStartPoint>();
    }

    public override void OnJoinedRoom()
    {
        if (isLog) Debug.Log("OnJoinedRoom");
        if (PhotonNetwork.IsMasterClient)
        {
            if (startGameOnRoomCreated)
            {
                // If master client joined room, wait for scene change if needed
                StartCoroutine(MasterWaitOnlineSceneLoaded());
            }
            if (isMatchMaking)
            {
                // Set match making state to true for join random filter later
                Hashtable roomCustomProperties = new Hashtable();
                roomCustomProperties[CUSTOM_ROOM_MATCH_MAKE] = true;
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomCustomProperties);
            }
        }
        // Set player state to not ready
        Hashtable playerCustomProperties = new Hashtable();
        playerCustomProperties[CUSTOM_PLAYER_STATE] = (byte)PlayerState.NotReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerCustomProperties);
        if (onJoinedRoom != null)
            onJoinedRoom.Invoke();
    }

    protected IEnumerator MasterWaitOnlineSceneLoaded()
    {
        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            yield return null;
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isLog) Debug.Log("OnConnectedToMaster");
        if (onConnectedToMaster != null)
            onConnectedToMaster.Invoke();
        if (isConnectOffline)
            PhotonNetwork.JoinRandomRoom();
        else if (autoJoinLobby)
            JoinLobby();
    }

    /// <summary>
    /// Override this to initialize something after scene changed
    /// </summary>
    public virtual void OnOnlineSceneChanged()
    {
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Hashtable customProperties = new Hashtable();
        customProperties[CUSTOM_ROOM_PLAYER_ID] = newMasterClient.UserId;
        customProperties[CUSTOM_ROOM_PLAYER_NAME] = newMasterClient.NickName;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        if (onMasterClientSwitched != null)
            onMasterClientSwitched.Invoke(newMasterClient);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (isLog) Debug.Log("OnPlayerEnteredRoom");
        if (onPlayerConnected != null)
            onPlayerConnected.Invoke(newPlayer);
        if (isMatchMaking && PhotonNetwork.CurrentRoom.PlayerCount >= matchMakingConnections)
            StartGame();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isLog) Debug.Log("OnPlayerLeftRoom");
        if (onPlayerDisconnected != null)
            onPlayerDisconnected.Invoke(otherPlayer);
    }

    public override void OnPlayerPropertiesUpdate(Player player, Hashtable props)
    {
        if (isLog) Debug.Log("OnPhotonPlayerPropertiesChanged");
        if (onPlayerPropertiesChanged != null)
            onPlayerPropertiesChanged.Invoke(player, props);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (isLog) Debug.Log("OnPhotonCustomRoomPropertiesChanged");
        if (onCustomRoomPropertiesChanged != null)
            onCustomRoomPropertiesChanged.Invoke(propertiesThatChanged);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isLog)
            Debug.Log("OnSceneLoaded " + scene.name);
        StartCoroutine(OnSceneLoadedRoutine(scene, mode));
    }

    IEnumerator OnSceneLoadedRoutine(Scene scene, LoadSceneMode mode)
    {
        yield return null;
        while (!PhotonNetwork.IsMessageQueueRunning)
            yield return null;
        if ((offlineScene.SceneName == onlineScene.SceneName || offlineScene.SceneName != scene.name) && PhotonNetwork.InRoom)
        {
            // Send client ready to spawn player at master client
            OnOnlineSceneChanged();
            photonView.RPC("RpcPlayerSceneChanged", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.UserId);
        }
    }

    public void TogglePlayerState()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Cannot toggle player state because you are not in room");
            return;
        }

        Hashtable customProperties = new Hashtable();
        PlayerState state = PlayerState.NotReady;
        object stateObj;
        if (customProperties.TryGetValue(CUSTOM_PLAYER_STATE, out stateObj))
            state = (PlayerState)(byte)stateObj;
        // Toggle state
        if (state == PlayerState.NotReady)
            state = PlayerState.Ready;
        else if (state == PlayerState.Ready)
            state = PlayerState.NotReady;
        // Set state property
        customProperties[CUSTOM_PLAYER_STATE] = (byte)state;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    public void SetPlayerState(PlayerState playerState)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Cannot set player state because you are not in room");
            return;
        }

        Hashtable customProperties = new Hashtable();
        customProperties[CUSTOM_PLAYER_STATE] = (byte)playerState;
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    public Player GetPlayerById(string id)
    {
        Player foundPlayer = null;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.UserId == id)
            {
                foundPlayer = player;
                break;
            }
        }
        return foundPlayer;
    }

    public int CountPlayerWithState(PlayerState state)
    {
        int result = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            object stateObject;
            if (player.CustomProperties.TryGetValue(CUSTOM_PLAYER_STATE, out stateObject) && (PlayerState)stateObject == state)
                result++;
        }
        return result;
    }

    [PunRPC]
    protected virtual void RpcAddPlayer()
    {
        Vector3 position = Vector3.zero;
        var rotation = Quaternion.identity;
        RandomStartPoint(out position, out rotation);
        PhotonNetwork.Instantiate(playerPrefab.name, position, rotation, 0);
    }

    [PunRPC]
    protected virtual void RpcPlayerSceneChanged(string id)
    {
        Player foundPlayer = GetPlayerById(id);
        if (foundPlayer != null)
            photonView.RPC("RpcAddPlayer", foundPlayer);
    }

    public bool RandomStartPoint(out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;
        if (StartPoints == null || StartPoints.Length <= 0)
            return false;
        var point = StartPoints[Random.Range(0, StartPoints.Length)];
        position = point.position;
        rotation = point.rotation;
        return true;
    }

    public override void OnLeftRoom()
    {
        if (isLog) Debug.Log("OnLeftRoom");
        if (!SceneManager.GetActiveScene().name.Equals(offlineScene.SceneName))
            SceneManager.LoadScene(offlineScene.SceneName);
        if (onLeftRoom != null)
            onLeftRoom.Invoke();
    }

    public static RoomState GetRoomState()
    {
        if (!PhotonNetwork.InRoom)
            return RoomState.Waiting;

        var customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (customProperties.ContainsKey(CUSTOM_PLAYER_STATE))
            return (RoomState)customProperties[CUSTOM_PLAYER_STATE];

        return RoomState.Waiting;
    }
}
