using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UIPhotonNetworking : UIBase
{
    [System.Serializable]
    public struct SelectableRegionEntry
    {
        public string title;
        public string regionCode;
    }
    public SelectableRegionEntry[] selectableRegions;
    public Dropdown dropdownRegion;
    public UIBase uiDisconnected;
    public UIBase uiConnected;
    public UIEnterNetworkAddress uiEnterNetworkAddress;
    public UIPhotonWaitingRoom uiWaitingRoom;
    public UIPhotonEnterPassword uiEnterPassword;
    public UIPhotonNetworkingEntry entryPrefab;
    public GameObject noEntryObject;
    public Transform gameListContainer;
    private readonly Dictionary<int, string> regions = new Dictionary<int, string>();
    private readonly List<NetworkDiscoveryData> discoveryList = new List<NetworkDiscoveryData>();
    private Hashtable discoveryFilters;

    private void OnEnable()
    {
        if (dropdownRegion != null)
        {
            dropdownRegion.ClearOptions();
            var titles = new List<string>();
            for (var i = 0; i < selectableRegions.Length; ++i)
            {
                var selectableRegion = selectableRegions[i];
                titles.Add(selectableRegion.title);
                regions[i] = selectableRegion.regionCode;
            }
            dropdownRegion.AddOptions(titles);
        }

        if (PhotonNetwork.IsConnectedAndReady)
            OnJoinedLobbyCallback();
        else
            OnDisconnectedCallback();

        SimplePhotonNetworkManager.onReceivedRoomListUpdate += OnReceivedRoomListUpdateCallback;
        SimplePhotonNetworkManager.onJoinedLobby += OnJoinedLobbyCallback;
        SimplePhotonNetworkManager.onJoinedRoom += OnJoinedRoomCallback;
        SimplePhotonNetworkManager.onLeftRoom += OnLeftRoomCallback;
        SimplePhotonNetworkManager.onDisconnected += OnDisconnectedCallback;
        SimplePhotonNetworkManager.onConnectionError += OnConnectionErrorCallback;
        discoveryList.Clear();
        discoveryList.AddRange(SimplePhotonNetworkManager.Rooms.Values);
        CreateRoomList();
    }

    private void OnDisable()
    {
        SimplePhotonNetworkManager.onReceivedRoomListUpdate -= OnReceivedRoomListUpdateCallback;
        SimplePhotonNetworkManager.onJoinedLobby -= OnJoinedLobbyCallback;
        SimplePhotonNetworkManager.onJoinedRoom -= OnJoinedRoomCallback;
        SimplePhotonNetworkManager.onLeftRoom -= OnLeftRoomCallback;
        SimplePhotonNetworkManager.onDisconnected -= OnDisconnectedCallback;
        SimplePhotonNetworkManager.onConnectionError -= OnConnectionErrorCallback;
    }

    private void Update()
    {
        if (noEntryObject != null)
            noEntryObject.SetActive(gameListContainer == null || gameListContainer.childCount <= 0);
    }

    private void OnReceivedRoomListUpdateCallback(List<NetworkDiscoveryData> list)
    {
        discoveryList.Clear();
        discoveryList.AddRange(SimplePhotonNetworkManager.Rooms.Values);
        CreateRoomList();
    }

    public void FilterDiscovery(Hashtable filters)
    {
        discoveryFilters = filters;
        CreateRoomList();
    }

    private void CreateRoomList()
    {
        if (entryPrefab == null || gameListContainer == null)
            return;
        for (var i = gameListContainer.childCount - 1; i >= 0; --i)
        {
            var child = gameListContainer.GetChild(i);
            Destroy(child.gameObject);
        }
        foreach (var data in discoveryList)
        {
            if (!PassDiscoveryFilters(data.fullProperties)) continue;
            var newEntry = Instantiate(entryPrefab, gameListContainer);
            newEntry.SetData(data);
            newEntry.gameObject.SetActive(true);
        }
    }

    private bool PassDiscoveryFilters(Hashtable properties)
    {
        if (discoveryFilters == null || discoveryFilters.Count == 0)
            return true;
        foreach (var key in discoveryFilters.Keys)
        {
            if (properties == null ||
                !properties.ContainsKey(key) ||
                !properties[key].Equals(discoveryFilters[key]))
            {
                return false;
            }
        }
        return true;
    }

    private void OnJoinedLobbyCallback()
    {
        if (uiDisconnected != null)
            uiDisconnected.Hide();

        if (uiConnected != null)
            uiConnected.Show();
    }

    private void OnJoinedRoomCallback()
    {
        if (uiWaitingRoom != null)
            uiWaitingRoom.Show();
    }

    private void OnLeftRoomCallback()
    {
        if (uiWaitingRoom != null)
            uiWaitingRoom.Hide();
    }

    private void OnDisconnectedCallback()
    {
        if (uiDisconnected != null)
            uiDisconnected.Show();

        if (uiConnected != null)
            uiConnected.Hide();
    }

    private void OnConnectionErrorCallback(DisconnectCause cause)
    {
        if (uiDisconnected != null)
            uiDisconnected.Show();

        if (uiConnected != null)
            uiConnected.Hide();
    }

    public virtual void OnClickConnectToMaster()
    {
        var networkManager = SimplePhotonNetworkManager.Singleton;
        if (uiEnterNetworkAddress != null)
            uiEnterNetworkAddress.Show();
        else
            networkManager.ConnectToMaster();
    }

    public virtual void OnClickConnectToBestCloudServer()
    {
        var networkManager = SimplePhotonNetworkManager.Singleton;
        networkManager.ConnectToBestCloudServer();
    }

    public virtual void OnClickConnectToRegion()
    {
        var networkManager = SimplePhotonNetworkManager.Singleton;
        string regionCode;
        // TODO: Get region codes
        if (dropdownRegion != null && regions.TryGetValue(dropdownRegion.value, out regionCode))
        {
            networkManager.region = regionCode;
            networkManager.ConnectToRegion();
        }
        else
            networkManager.ConnectToBestCloudServer();
    }

    public virtual void OnClickPlayOffline()
    {
        var networkManager = SimplePhotonNetworkManager.Singleton;
        networkManager.PlayOffline();
    }

    public virtual void OnClickJoinRandomRoom()
    {
        var networkManager = SimplePhotonNetworkManager.Singleton;
        networkManager.JoinRandomRoom();
    }

    public virtual void OnClickDisconnect()
    {
        var networkManager = SimplePhotonNetworkManager.Singleton;
        networkManager.Disconnect();
    }

    public virtual void OnClickCreateRoom()
    {
        SimplePhotonNetworkManager.Singleton.CreateRoom();
    }

    public virtual void OnClickCreateWaitingRoom()
    {
        SimplePhotonNetworkManager.Singleton.CreateWaitingRoom();
    }

    public void ShowEnterPasswordDialog(NetworkDiscoveryData data)
    {
        if (uiEnterPassword == null)
        {
            Debug.LogError("[UIPhotonNetworking] uiEnterPassword is empty");
            return;
        }
        uiEnterPassword.uiPhotonNetworking = this;
        uiEnterPassword.SetData(data);
        uiEnterPassword.Show();
    }
}
