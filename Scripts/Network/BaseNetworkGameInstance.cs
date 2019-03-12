using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseNetworkGameInstance : MonoBehaviour
{
    public static BaseNetworkGameInstance Singleton { get; private set; }
    public BaseNetworkGameRule[] gameRules;
    public static Dictionary<string, BaseNetworkGameRule> GameRules = new Dictionary<string, BaseNetworkGameRule>();
    protected virtual void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
        GameRules.Clear();
        foreach (var gameRule in gameRules)
        {
            GameRules[gameRule.name] = gameRule;
        }
    }

    protected virtual void Start()
    {
        // The photon version not supports arguments
    }
}
