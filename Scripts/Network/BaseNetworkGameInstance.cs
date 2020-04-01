using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapSelection
{
    public string mapName;
    public SceneNameField scene;
    public Sprite previewImage;
    public BaseNetworkGameRule[] availableGameRules;
}

public abstract class BaseNetworkGameInstance : MonoBehaviour
{
    public static BaseNetworkGameInstance Singleton { get; private set; }
    public MapSelection[] maps;
    public static Dictionary<string, BaseNetworkGameRule> GameRules = new Dictionary<string, BaseNetworkGameRule>();
    public static Dictionary<string, MapSelection> MapListBySceneNames = new Dictionary<string, MapSelection>();
    protected virtual void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
        SetupMaps();
    }

    public void SetupMaps()
    {
        MapListBySceneNames.Clear();
        GameRules.Clear();
        foreach (var map in maps)
        {
            foreach (var gameRule in map.availableGameRules)
            {
                if (!GameRules.ContainsKey(gameRule.name))
                {
                    gameRule.InitData();
                    GameRules[gameRule.name] = gameRule;
                }
            }
            MapListBySceneNames[map.scene.SceneName] = map;
        }
    }

    protected virtual void Start()
    {
        // The photon version not supports arguments
    }

    public static string GetMapNameByScene(string sceneName)
    {
        if (MapListBySceneNames.ContainsKey(sceneName))
            return MapListBySceneNames[sceneName].mapName;
        return sceneName;
    }
}
