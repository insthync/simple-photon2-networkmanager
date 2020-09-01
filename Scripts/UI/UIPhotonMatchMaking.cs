using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UIPhotonMatchMaking : UIBase
{
    [Serializable]
    public struct FilterKeyValue
    {
        public string key;
        public string value;
    }

    [Serializable]
    public struct FilterSetting
    {
        public string title;
        public string description;
        [Header("Filters")]
        public SceneNameField onlineScene;
        public BaseNetworkGameRule gameplayRule;
        public FilterKeyValue[] customFilters;
        [Header("Create Room Settings")]
        [Tooltip("It will be used when create room and this value more than 0 to change creating room max connections")]
        public byte maxConnections;
    }

    public FilterSetting[] filters;
    [Multiline]
    public string formatMatchMakingCount = "Matching...\n {0}";
    public Text textMatchMakingCount;
    public GameObject[] showingObjectsWhileMatchMaking;
    public GameObject[] hiddingObjectsWhileMatchMaking;
    public Dropdown filterOptionDropdown;
    public int selectedFilter;

    private void Start()
    {
        if (filterOptionDropdown)
        {
            filterOptionDropdown.ClearOptions();
            if (filters != null && filters.Length > 0)
            {
                List<string> options = new List<string>();
                foreach (var filter in filters)
                {
                    options.Add(filter.title);
                }
                filterOptionDropdown.AddOptions(options);
            }
            filterOptionDropdown.onValueChanged.AddListener(OnFilterOptionChanged);
        }
    }

    public void OnFilterOptionChanged(int index)
    {
        selectedFilter = index;
    }

    private void Update()
    {
        if (textMatchMakingCount != null)
            textMatchMakingCount.text = GetTimeText();

        foreach (var obj in showingObjectsWhileMatchMaking)
        {
            if (!obj) continue;
            obj.SetActive(SimplePhotonNetworkManager.Singleton.isMatchMaking);
        }

        foreach (var obj in hiddingObjectsWhileMatchMaking)
        {
            if (!obj) continue;
            obj.SetActive(!SimplePhotonNetworkManager.Singleton.isMatchMaking);
        }
    }

    private string GetTimeText()
    {
        TimeSpan t = TimeSpan.FromSeconds(Time.unscaledTime - SimplePhotonNetworkManager.Singleton.startMatchMakingTime);
        return string.Format(formatMatchMakingCount, string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds));
    }

    public void OnClickStartMatchMaking()
    {
        SimplePhotonNetworkManager.Singleton.StartMatchMaking(GetFilters());
    }

    public void OnClickStopMatchMaking()
    {
        SimplePhotonNetworkManager.Singleton.StopMatchMaking();
    }

    public Hashtable GetFilters()
    {
        var result = new Hashtable();
        if (filters != null && filters.Length > 0 && selectedFilter >= 0 && selectedFilter < filters.Length)
        {
            var filter = filters[selectedFilter];
            // Filter scene
            if (filter.onlineScene != null)
            {
                SimplePhotonNetworkManager.Singleton.onlineScene = filter.onlineScene;
                result[SimplePhotonNetworkManager.CUSTOM_ROOM_SCENE_NAME] = filter.onlineScene.SceneName;
            }
            // Filter gameplay rule
            if (filter.gameplayRule != null && BaseNetworkGameInstance.GameRules.ContainsKey(filter.gameplayRule.name))
                result[BaseNetworkGameManager.CUSTOM_ROOM_GAME_RULE] = filter.gameplayRule.name;
            // Custom filters
            if (filter.customFilters != null && filter.customFilters.Length > 0)
            {
                foreach (var customFilter in filter.customFilters)
                {
                    if (string.IsNullOrEmpty(customFilter.key)) continue;
                    result[customFilter.key] = customFilter.value;
                }
            }
            if (filter.maxConnections > 0)
                SimplePhotonNetworkManager.Singleton.maxConnections = filter.maxConnections;
        }
        return result;
    }
}
