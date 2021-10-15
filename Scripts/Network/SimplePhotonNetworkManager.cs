using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public class RoomData
    {
        public bool HasUpdate { get; set; }
        public float SetDataTime { get; set; }
        public object Data { get; set; }
    }    

    public const string OFFLINE_USER_ID = "OFFLINE_USER";
    public const int UNIQUE_VIEW_ID = 999;
    public const string CUSTOM_ROOM_ROOM_NAME = "R";
    public const string CUSTOM_ROOM_ROOM_PASSWORD = "RP";
    public const string CUSTOM_ROOM_PLAYER_ID = "Id";
    public const string CUSTOM_ROOM_PLAYER_NAME = "P";
    public const string CUSTOM_ROOM_SCENE_NAME = "S";
    public const string CUSTOM_ROOM_MATCH_MAKE = "MM";
    public const string CUSTOM_ROOM_STATE = "St";
    public const string CUSTOM_ROOM_BOTS_TEAMS = "BT";
    public const string CUSTOM_PLAYER_STATE = "St";
    public const string CUSTOM_PLAYER_TEAM = "T";
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
    public static System.Action<RegionHandler> onRegionListReceived;

    public bool isLog;
    public SceneNameField offlineScene;
    public SceneNameField onlineScene;
    public GameObject playerPrefab;
    public string saveSelectedRegionKey = "SAVE_SELECTED_REGION";
    public string gameVersion = "1";
    public string masterAddress = "localhost";
    public int masterPort = 5055;
    public string region;
    public int sendRate = 20;
    public byte maxConnections = 10;
    public byte matchMakingConnections = 2;
    public float maxMatchMakingTime = 60f;
    public float updateRoomPropertyInterval = 1f;
    public bool randomAlphaNumericName = true;
    public int alphaNumericNameLength = 8;
    public string roomName;
    public string roomPassword;
    public SimplePhotonStartPoint[] StartPoints { get; protected set; }
    public bool isConnectOffline { get; protected set; }
    public bool isMatchMaking { get; protected set; }
    public float startMatchMakingTime { get; protected set; }

    private bool startGameOnRoomCreated;
    private Hashtable tempMatchMakingFilters;
    protected bool isConnectingToBestRegion;
    protected bool isConnectedToBestRegion;
    protected bool isConnectingToSelectedRegion;
    protected bool isQuitting;
    protected bool onlineSceneLoaded;

    private Dictionary<string, RoomData> roomData = new Dictionary<string, RoomData>();
    private Dictionary<int, byte> botsTeams = new Dictionary<int, byte>();
    public static readonly Dictionary<string, NetworkDiscoveryData> Rooms = new Dictionary<string, NetworkDiscoveryData>();
    public static readonly Dictionary<string, Region> EnabledRegions = new Dictionary<string, Region>();

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

    protected virtual void Update()
    {
        if (isMatchMaking && maxMatchMakingTime > 0 && Time.unscaledTime - startMatchMakingTime >= maxMatchMakingTime)
        {
            StartGame();
        }

        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            var time = Time.unscaledTime;
            var hashTable = new Hashtable();
            foreach (var key in roomData.Keys)
            {
                if (time - roomData[key].SetDataTime >= updateRoomPropertyInterval && roomData[key].HasUpdate)
                {
                    hashTable.Add(key, roomData[key].Data);
                    roomData[key].HasUpdate = false;
                }
            }
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashTable);
        }
    }

    public T GetRoomProperty<T>(string key, T defaultValue = default)
    {
        if (PhotonNetwork.IsMasterClient && roomData.ContainsKey(key))
            return (T)roomData[key].Data;
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key))
            return (T)PhotonNetwork.CurrentRoom.CustomProperties[key];
        return defaultValue;
    }

    public void SetRoomProperty<T>(string key, T value, bool updateImmediately = false)
    {
        roomData[key] = new RoomData()
        {
            Data = value,
            SetDataTime = Time.unscaledTime,
            HasUpdate = true,
        };
        if (updateImmediately && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            roomData[key].HasUpdate = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable()
            {
                { key, value }
            });
        }
    }

    public T GetRoomPlayerProperty<T>(string key, Player player, T defaultValue = default)
    {
        if (player.CustomProperties.ContainsKey(key))
            return (T)player.CustomProperties[key];
        return defaultValue;
    }

    public void SetRoomPlayerProperty<T>(string key, Player player, T value)
    {
        var properties = player.CustomProperties;
        if (properties.ContainsKey(key))
            properties[key] = value;
        else
            properties.Add(key, value);
        player.SetCustomProperties(properties);
    }

    protected virtual void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                ChangeMasterClientifAvailble();
                PhotonNetwork.SendAllOutgoingCommands();

            }
        }
    }

    public void ChangeMasterClientifAvailble()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            return;
        }
        PhotonNetwork.SetMasterClient(PhotonNetwork.MasterClient.GetNext());
    }

    public virtual void ConnectToMaster()
    {
        if (PhotonNetwork.InLobby || PhotonNetwork.InRoom)
            return;
        isConnectOffline = false;
        PhotonNetwork.AuthValues = new AuthenticationValues(System.Guid.NewGuid().ToString());
        PhotonNetwork.NetworkingClient.SerializationProtocol = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV16;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.ConnectToMaster(masterAddress, masterPort, PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
        if (onConnectingToMaster != null)
            onConnectingToMaster.Invoke();
    }

    public virtual void ConnectToBestCloudServer()
    {
        if (PhotonNetwork.InLobby || PhotonNetwork.InRoom)
            return;
        isConnectOffline = false;
        // Delete saved best region, to re-ping all regions, to fix unknow ping problem
        ServerSettings.ResetBestRegionCodeInPreferences();
        PhotonNetwork.NetworkingClient.SerializationProtocol = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV18;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.ConnectToBestCloudServer();
        isConnectingToBestRegion = true;
        if (onConnectingToMaster != null)
            onConnectingToMaster.Invoke();
    }

    public virtual void ConnectToRegion()
    {
        if (PhotonNetwork.InLobby || PhotonNetwork.InRoom)
            return;
        if (isLog) Debug.Log("Connecting to region " + region);
        // Hacking PUN, It seems like PUN won't connect to name server when call `PhotonNetwork.ConnectToRegion()`
        // Have to connect to best cloud server to make it connect to name server to get all regions list
        if (!isConnectedToBestRegion)
        {
            // If not connected to best region once, connect to best region to ping all regions
            ConnectToBestCloudServer();
            isConnectingToSelectedRegion = true;
        }
        else
        {
            // It's ready to connect to selected region because it was connected to best region once and pinged all regions
            PhotonNetwork.NetworkingClient.SerializationProtocol = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV18;
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.ConnectToRegion(region);
        }
    }

    public virtual void ConnectToSavedCloudServer()
    {
        if (PhotonNetwork.InLobby || PhotonNetwork.InRoom)
            return;
        region = PlayerPrefs.GetString(saveSelectedRegionKey, string.Empty);
        if (string.IsNullOrEmpty(region))
            ConnectToBestCloudServer();
        else
            ConnectToRegion();
    }

    public virtual void PlayOffline()
    {
        isConnectOffline = true; // Set the condition to start offline mode when player create game or room.
        PhotonNetwork.NetworkingClient.SerializationProtocol = ExitGames.Client.Photon.SerializationProtocol.GpBinaryV18;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.OfflineMode = false; // This will turn to be `TRUE` when player create game or room.
        if (onJoinedLobby != null)
            onJoinedLobby.Invoke();
    }

    public void CreateRoom()
    {
        if (isMatchMaking)
        {
            if (isLog) Debug.Log("Cannot create room, match making started");
            return;
        }
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
        if (isMatchMaking)
        {
            if (isLog) Debug.Log("Cannot create waiting room, match making started");
            return;
        }
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

    public string GenerateName()
    {
        if (randomAlphaNumericName)
        {
            string name = string.Empty;
            for (int i = 0; i < alphaNumericNameLength; ++i)
            {
                name += Random.Range(0, 9);
            }
            return name;
        }
        return string.Empty;
    }

    private void SetupAndCreateRoom()
    {
        var roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = GetCustomRoomPropertiesForLobby();
        roomOptions.MaxPlayers = maxConnections;
        roomOptions.PublishUserId = true;
        PhotonNetwork.CreateRoom(GenerateName(), roomOptions, null);
    }

    protected virtual string[] GetCustomRoomPropertiesForLobby()
    {
        return new string[]
        {
            CUSTOM_ROOM_ROOM_NAME,
            CUSTOM_ROOM_ROOM_PASSWORD,
            CUSTOM_ROOM_PLAYER_ID,
            CUSTOM_ROOM_PLAYER_NAME,
            CUSTOM_ROOM_SCENE_NAME,
            CUSTOM_ROOM_MATCH_MAKE,
            CUSTOM_ROOM_STATE,
            CUSTOM_ROOM_BOTS_TEAMS
        };
    }

    public void StartMatchMaking()
    {
        StartMatchMaking(null);
    }

    public virtual void StartMatchMaking(Hashtable filters)
    {
        if (isMatchMaking)
        {
            if (isLog) Debug.Log("Cannot start match making, match making started");
            return;
        }
        if (isConnectOffline)
        {
            PhotonNetwork.OfflineMode = true;
            StartGame();
            return;
        }
        startGameOnRoomCreated = false;
        isMatchMaking = true;
        if (filters == null)
            filters = new Hashtable();
        filters[CUSTOM_ROOM_MATCH_MAKE] = true;
        PhotonNetwork.JoinRandomRoom(filters, 0);
        tempMatchMakingFilters = filters;
        startMatchMakingTime = Time.unscaledTime;
        if (onMatchMakingStarted != null)
            onMatchMakingStarted.Invoke();
    }

    private void SetupAndCreateMatchMakingRoom()
    {
        var roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = GetCustomRoomPropertiesForLobby();
        roomOptions.MaxPlayers = maxConnections;
        roomOptions.PublishUserId = true;
        PhotonNetwork.CreateRoom(GenerateName(), roomOptions, null);
    }

    public void StopMatchMaking()
    {
        LeaveRoom();
    }

    public void OnStopMatchMaking()
    {
        if (isMatchMaking)
        {
            isMatchMaking = false;
            if (onMatchMakingStopped != null)
                onMatchMakingStopped.Invoke();
        }
    }

    public void SetRoomName(string roomName)
    {
        // If room not created, set data to field to use later
        this.roomName = roomName;
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            SetRoomProperty(CUSTOM_ROOM_ROOM_NAME, roomName);
        }
    }

    public void SetRoomPassword(string roomPassword)
    {
        // If room not created, set data to field to use later
        this.roomPassword = roomPassword;
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            SetRoomProperty(CUSTOM_ROOM_ROOM_PASSWORD, roomPassword);
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
            SetRoomProperty(CUSTOM_ROOM_SCENE_NAME, onlineScene.SceneName);
        }
    }

    public virtual void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        if (onJoiningRoom != null)
            onJoiningRoom.Invoke();
    }

    public void JoinRandomRoom()
    {
        JoinRandomRoom(null);
    }

    public virtual void JoinRandomRoom(Hashtable filter)
    {
        if (filter == null)
            filter = new Hashtable();
        filter[CUSTOM_ROOM_MATCH_MAKE] = false;
        PhotonNetwork.JoinRandomRoom(filter, 0);
        if (onJoiningRoom != null)
            onJoiningRoom.Invoke();
    }

    public virtual void LeaveRoom()
    {
        if (isConnectOffline)
            Disconnect();
        else
            PhotonNetwork.LeaveRoom();
    }

    public virtual void Disconnect()
    {
        if (isConnectOffline && !PhotonNetwork.OfflineMode)
        {
            if (onDisconnected != null)
                onDisconnected.Invoke();
        }
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        CacheRoom(rooms);
        if (onReceivedRoomListUpdate != null)
            onReceivedRoomListUpdate.Invoke(new List<NetworkDiscoveryData>(Rooms.Values));
    }

    protected void CacheRoom(List<RoomInfo> rooms)
    {
        foreach (RoomInfo room in rooms)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (Rooms.ContainsKey(room.Name))
                {
                    Rooms.Remove(room.Name);
                }

                continue;
            }

            NetworkDiscoveryData discoveryData;
            if (CanAddRoom(room, out discoveryData))
            {
                // Update cached room info
                if (Rooms.ContainsKey(room.Name))
                {
                    Rooms[room.Name] = discoveryData;
                }
                // Add new room info to cache
                else
                {
                    Rooms.Add(room.Name, discoveryData);
                }
            }
        }
    }

    protected virtual bool CanAddRoom(RoomInfo room, out NetworkDiscoveryData discoveryData)
    {
        discoveryData = new NetworkDiscoveryData();
        var customProperties = room.CustomProperties;
        if (customProperties.Count == 0)
            return false;
        var isMatchMaking = (bool)customProperties[CUSTOM_ROOM_MATCH_MAKE];
        if (isMatchMaking)
            return false;
        discoveryData.name = room.Name;
        discoveryData.roomName = (string)customProperties[CUSTOM_ROOM_ROOM_NAME];
        discoveryData.roomPassword = (string)customProperties[CUSTOM_ROOM_ROOM_PASSWORD];
        discoveryData.playerId = (string)customProperties[CUSTOM_ROOM_PLAYER_ID];
        discoveryData.playerName = (string)customProperties[CUSTOM_ROOM_PLAYER_NAME];
        discoveryData.sceneName = (string)customProperties[CUSTOM_ROOM_SCENE_NAME];
        discoveryData.state = (byte)customProperties[CUSTOM_ROOM_STATE];
        discoveryData.numPlayers = room.PlayerCount;
        discoveryData.maxPlayers = room.MaxPlayers;
        discoveryData.fullProperties = customProperties;
        return true;
    }

    protected void OnApplicationQuit()
    {
        isQuitting = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (isQuitting)
            return;
        onlineSceneLoaded = false;
        if (!SceneManager.GetActiveScene().name.Equals(offlineScene.SceneName))
            SceneManager.LoadScene(offlineScene.SceneName);
        OnStopMatchMaking();
        roomData.Clear();
        botsTeams.Clear();
        Rooms.Clear();
        if (isConnectOffline)
        {
            // Connect offline, so it won't cause connection error
            if (onDisconnected != null)
                onDisconnected.Invoke();
            return;
        }
        if (isLog) Debug.Log("OnDisconnected " + cause.ToString());
        switch (cause)
        {
            case DisconnectCause.None:
            case DisconnectCause.DisconnectByClientLogic:
                if (onDisconnected != null)
                    onDisconnected.Invoke();
                break;
            default:
                if (onConnectionError != null)
                    onConnectionError.Invoke(cause);
                break;
        }
    }

    public override void OnLeftLobby()
    {
        if (isQuitting)
            return;
        roomData.Clear();
        botsTeams.Clear();
        Rooms.Clear();
    }

    public override void OnLeftRoom()
    {
        if (isQuitting)
            return;
        if (isLog) Debug.Log("OnLeftRoom");
        onlineSceneLoaded = false;
        if (!SceneManager.GetActiveScene().name.Equals(offlineScene.SceneName))
            SceneManager.LoadScene(offlineScene.SceneName);
        if (onLeftRoom != null)
            onLeftRoom.Invoke();
        OnStopMatchMaking();
        roomData.Clear();
        botsTeams.Clear();
        Rooms.Clear();
    }

    public override void OnCreateRoomFailed(short code, string msg)
    {
        if (isLog) Debug.Log("OnCreateRoomFailed " + code + " " + msg);
        if (onRoomConnectError != null)
            onRoomConnectError.Invoke(code, msg);
        OnStopMatchMaking();
    }

    public override void OnJoinRoomFailed(short code, string msg)
    {
        if (isLog) Debug.Log("OnJoinRoomFailed " + code + " " + msg);
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
        customProperties[CUSTOM_ROOM_ROOM_PASSWORD] = roomPassword;
        customProperties[CUSTOM_ROOM_PLAYER_ID] = PhotonNetwork.LocalPlayer.UserId;
        customProperties[CUSTOM_ROOM_PLAYER_NAME] = PhotonNetwork.LocalPlayer.NickName;
        customProperties[CUSTOM_ROOM_SCENE_NAME] = onlineScene.SceneName;
        customProperties[CUSTOM_ROOM_MATCH_MAKE] = false;
        customProperties[CUSTOM_ROOM_STATE] = (byte)RoomState.Waiting;
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

        if (isMatchMaking)
        {
            if (maxMatchMakingTime <= 0 || Time.unscaledTime - startMatchMakingTime < maxMatchMakingTime)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount < matchMakingConnections)
                {
                    Debug.LogError("Player is not enough to start game");
                    return;
                }
            }
        }

        isMatchMaking = false;

        LoadOnlineScene();
    }

    async void LoadOnlineScene()
    {
        PhotonNetwork.LoadLevel(onlineScene.SceneName);
        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            await Task.Yield();
        }
        // Change room state to playing
        SetRoomProperty(CUSTOM_ROOM_STATE, (byte)RoomState.Playing);
        // Setup start points for master client
        StartPoints = FindObjectsOfType<SimplePhotonStartPoint>();
    }

    public override void OnJoinedRoom()
    {
        if (isLog) Debug.Log("OnJoinedRoom");
        if (isConnectOffline)
        {
            // Don't do anything while connect offline, because it don't have to join room.
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (isMatchMaking)
            {
                // Set match making state to true for join random filter later
                PhotonNetwork.CurrentRoom.SetCustomProperties(tempMatchMakingFilters);

            }
        }
        // Set player state to not ready
        SetRoomPlayerProperty(CUSTOM_PLAYER_STATE, PhotonNetwork.LocalPlayer, (byte)PlayerState.NotReady);

        if (!isMatchMaking)
        {
            if (onJoinedRoom != null)
                onJoinedRoom.Invoke();
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isLog) Debug.Log("OnConnectedToMaster " + PhotonNetwork.CloudRegion);
        if (onConnectedToMaster != null)
            onConnectedToMaster.Invoke();
        if (isConnectOffline)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PlayerPrefs.SetString(saveSelectedRegionKey, PhotonNetwork.CloudRegion.TrimEnd('/', '*'));
            PlayerPrefs.Save();
            if (isConnectingToBestRegion)
            {
                isConnectingToBestRegion = false;
                isConnectedToBestRegion = true;
            }
            if (isConnectingToSelectedRegion)
            {
                isConnectingToSelectedRegion = false;
                PhotonNetwork.Disconnect();
                ConnectToRegion();
            }
            else
            {
                PhotonNetwork.JoinLobby();
                if (onJoiningLobby != null)
                    onJoiningLobby.Invoke();
            }
        }
    }

    /// <summary>
    /// Override this to initialize something after scene changed
    /// </summary>
    public virtual void OnOnlineSceneChanged()
    {
        onlineSceneLoaded = true;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        SetRoomProperty(CUSTOM_ROOM_PLAYER_ID, newMasterClient.UserId);
        SetRoomProperty(CUSTOM_ROOM_PLAYER_NAME, newMasterClient.NickName);
        if (onMasterClientSwitched != null)
            onMasterClientSwitched.Invoke(newMasterClient);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (isLog) Debug.Log("OnPlayerEnteredRoom");
        if (onPlayerConnected != null)
            onPlayerConnected.Invoke(newPlayer);
        if (PhotonNetwork.IsMasterClient && isMatchMaking && PhotonNetwork.CurrentRoom.PlayerCount >= matchMakingConnections)
            StartGame();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (isLog) Debug.Log("OnPlayerLeftRoom " + otherPlayer.NickName);
        if (onPlayerDisconnected != null)
            onPlayerDisconnected.Invoke(otherPlayer);
    }

    public override void OnPlayerPropertiesUpdate(Player player, Hashtable props)
    {
        if (isLog) Debug.Log("OnPhotonPlayerPropertiesChanged " + player.NickName + " " + props.ToStringFull());
        if (onPlayerPropertiesChanged != null)
            onPlayerPropertiesChanged.Invoke(player, props);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (isLog) Debug.Log("OnPhotonCustomRoomPropertiesChanged " + propertiesThatChanged.ToStringFull());
        if (onCustomRoomPropertiesChanged != null)
            onCustomRoomPropertiesChanged.Invoke(propertiesThatChanged);
        botsTeams = GetBotsTeams();
    }

    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        if (isLog) Debug.Log("OnRegionListReceived");
        EnabledRegions.Clear();
        foreach (var region in regionHandler.EnabledRegions)
        {
            EnabledRegions[region.Code] = region;
        }
        if (onRegionListReceived != null)
            onRegionListReceived.Invoke(regionHandler);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isLog)
            Debug.Log("OnSceneLoaded " + scene.name);
        OnSceneLoadedRoutine(scene, mode);
    }

    async void OnSceneLoadedRoutine(Scene scene, LoadSceneMode mode)
    {
        while (!PhotonNetwork.IsMessageQueueRunning)
            await Task.Yield();
        if ((offlineScene.SceneName == onlineScene.SceneName || offlineScene.SceneName != scene.name) && PhotonNetwork.InRoom)
        {
            isMatchMaking = false;
            // Send client ready to spawn player at master client
            OnOnlineSceneChanged();
            photonView.MasterRPC(RpcPlayerSceneChanged, PhotonNetwork.LocalPlayer.UserId);
        }
    }

    public void TogglePlayerState()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Cannot toggle player state because you are not in room");
            return;
        }

        PlayerState state;
        state = (PlayerState)GetRoomPlayerProperty(CUSTOM_PLAYER_STATE, PhotonNetwork.LocalPlayer, (byte)PlayerState.NotReady);

        if (state == PlayerState.NotReady)
            state = PlayerState.Ready;
        else if (state == PlayerState.Ready)
            state = PlayerState.NotReady;

        SetRoomPlayerProperty(CUSTOM_PLAYER_STATE, PhotonNetwork.LocalPlayer, (byte)state);
    }

    public void SetPlayerState(PlayerState playerState)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("Cannot set player state because you are not in room");
            return;
        }

        SetRoomPlayerProperty(CUSTOM_PLAYER_STATE, PhotonNetwork.LocalPlayer, (byte)playerState);
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
            PlayerState playerState = (PlayerState)GetRoomPlayerProperty(CUSTOM_PLAYER_STATE, player, (byte)PlayerState.NotReady);
            if (playerState == state)
                result++;
        }
        return result;
    }

    [PunRPC]
    protected virtual void RpcAddPlayer()
    {
        if (playerPrefab == null)
            return;
        Vector3 position;
        Quaternion rotation;
        RandomStartPoint(out position, out rotation);
        PhotonNetwork.Instantiate(playerPrefab.name, position, rotation, 0);
    }

    [PunRPC]
    protected virtual void RpcPlayerSceneChanged(string id)
    {
        Player foundPlayer = GetPlayerById(id);
        if (foundPlayer != null)
            photonView.TargetRPC(RpcAddPlayer, foundPlayer);
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

    public RoomState GetRoomState()
    {
        if (PhotonNetwork.OfflineMode)
            return RoomState.Playing;

        if (!PhotonNetwork.InRoom)
            return RoomState.Waiting;

        return (RoomState)GetRoomProperty(CUSTOM_ROOM_STATE, (byte)RoomState.Waiting);
    }

    public byte GetTeam(Player player)
    {
        return GetRoomPlayerProperty(CUSTOM_PLAYER_TEAM, player, (byte)0);
    }

    public void SetTeam(Player player, byte team)
    {
        SetRoomPlayerProperty(CUSTOM_PLAYER_TEAM, player, team);
    }

    public void LeaveTeam(Player player)
    {
        SetTeam(player, 0);
    }

    protected Dictionary<int, byte> GetBotsTeams()
    {
        Dictionary<int, byte> result = new Dictionary<int, byte>();
        string savedValue = GetRoomProperty(CUSTOM_ROOM_BOTS_TEAMS, string.Empty);
        string[] splitedEntry = savedValue.Split(';');
        foreach (var entry in splitedEntry)
        {
            string[] splitedKV = entry.Split(':');
            if (splitedKV == null || splitedKV.Length != 2) continue;
            result[int.Parse(splitedKV[0])] = byte.Parse(splitedKV[1]);
        }
        return result;
    }

    protected void SetBotsTeams(Dictionary<int, byte> botsTeam)
    {
        string savingValue = string.Empty;
        foreach (var entry in botsTeam)
        {
            savingValue += $"{entry.Key}:{entry.Value};";
        }
        SetRoomProperty(CUSTOM_ROOM_BOTS_TEAMS, savingValue);
    }

    public byte GetBotTeam(int viewId)
    {
        if (botsTeams.ContainsKey(viewId))
            return botsTeams[viewId];
        return 0;
    }

    public void SetBotTeam(int viewId, byte team)
    {
        botsTeams[viewId] = team;
        SetBotsTeams(botsTeams);
    }

    public void CountTeamPlayers(out int countA, out int countB)
    {
        countA = 0;
        countB = 0;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var team = GetTeam(player);
            if (team == 1)
                countA++;
            if (team == 2)
                countB++;
        }
        HashSet<int> viewIds = new HashSet<int>();
        foreach (var view in PhotonNetwork.PhotonViewCollection)
        {
            viewIds.Add(view.ViewID);
        }
        foreach (var entry in botsTeams)
        {
            if (!viewIds.Contains(entry.Key))
                continue;
            if (entry.Value == 1)
                countA++;
            if (entry.Value == 2)
                countB++;
        }
    }
}
