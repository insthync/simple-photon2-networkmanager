using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

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
    public UIPhotonNetworkingEntry entryPrefab;
    public Transform gameListContainer;
    private readonly Dictionary<string, UIPhotonNetworkingEntry> entries = new Dictionary<string, UIPhotonNetworkingEntry>();
    private readonly Dictionary<int, string> regions = new Dictionary<int, string>();

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
    }

    private void OnDisable()
    {
        SimplePhotonNetworkManager.onReceivedRoomListUpdate -= OnReceivedRoomListUpdateCallback;
        SimplePhotonNetworkManager.onJoinedLobby -= OnJoinedLobbyCallback;
        SimplePhotonNetworkManager.onJoinedRoom -= OnJoinedRoomCallback;
        SimplePhotonNetworkManager.onLeftRoom -= OnLeftRoomCallback;
        SimplePhotonNetworkManager.onDisconnected -= OnDisconnectedCallback;
    }

    private void OnDestroy()
    {
        SimplePhotonNetworkManager.onReceivedRoomListUpdate -= OnReceivedRoomListUpdateCallback;
        SimplePhotonNetworkManager.onJoinedLobby -= OnJoinedLobbyCallback;
        SimplePhotonNetworkManager.onJoinedRoom -= OnJoinedRoomCallback;
        SimplePhotonNetworkManager.onLeftRoom -= OnLeftRoomCallback;
        SimplePhotonNetworkManager.onDisconnected -= OnDisconnectedCallback;
    }

    private void OnReceivedRoomListUpdateCallback(List<NetworkDiscoveryData> discoveryData)
    {
        for (var i = gameListContainer.childCount - 1; i >= 0; --i)
        {
            var child = gameListContainer.GetChild(i);
            Destroy(child.gameObject);
        }
        entries.Clear();
        foreach (var data in discoveryData)
        {
            var key = data.name;
            if (!entries.ContainsKey(key))
            {
                var newEntry = Instantiate(entryPrefab, gameListContainer);
                newEntry.SetData(data);
                newEntry.gameObject.SetActive(true);
                entries.Add(key, newEntry);
            }
        }
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

    public virtual void OnClickCreateWaitingRoom()
    {
        SimplePhotonNetworkManager.Singleton.CreateWaitingRoom();
    }
}
