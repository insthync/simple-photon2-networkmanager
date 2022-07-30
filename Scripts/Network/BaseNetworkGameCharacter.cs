using UnityEngine;
using Photon.Pun;
using System;
using System.Reflection;
using System.Collections.Generic;

[RequireComponent(typeof(SyncScoreRpcComponent))]
[RequireComponent(typeof(SyncKillCountRpcComponent))]
[RequireComponent(typeof(SyncAssistCountRpcComponent))]
[RequireComponent(typeof(SyncDieCountRpcComponent))]
public abstract class BaseNetworkGameCharacter : MonoBehaviourPunCallbacks, IComparable<BaseNetworkGameCharacter>
{
    protected static readonly Dictionary<string, List<FieldInfo>> cachedFunctions = new Dictionary<string, List<FieldInfo>>();
    public static BaseNetworkGameCharacter Local { get; private set; }
    public static int LocalViewId { get; set; }
    public static int LocalRank { get; set; }

    public GameObject[] noTeamObjects;
    public GameObject[] teamAObjects;
    public GameObject[] teamBObjects;

    public abstract bool IsDead { get; }
    public abstract bool IsBot { get; }

    public virtual string PlayerName
    {
        get { return (photonView != null && photonView.Owner != null) ? photonView.Owner.NickName : ""; }
        set { if (photonView.IsMine) photonView.Owner.NickName = value; }
    }

    public virtual byte PlayerTeam
    {
        get
        {
            if (IsBot)
                return SimplePhotonNetworkManager.Singleton.GetBotTeam(photonView.ViewID);
            if (photonView != null && photonView.Owner != null)
                return SimplePhotonNetworkManager.Singleton.GetTeam(photonView.Owner);
            else
                return 0;
        }
        set
        {
            if (IsBot)
                SimplePhotonNetworkManager.Singleton.SetBotTeam(photonView.ViewID, value);
            if (!IsBot && photonView.IsMine)
                SimplePhotonNetworkManager.Singleton.SetTeam(photonView.Owner, value);
        }
    }

    protected SyncScoreRpcComponent syncScore;
    public int Score
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroScoreWhenDead)
                return 0;
            return syncScore.Value;
        }
    }

    protected SyncKillCountRpcComponent syncKillCount;
    public int KillCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroKillCountWhenDead)
                return 0;
            return syncKillCount.Value;
        }
    }

    protected SyncAssistCountRpcComponent syncAssistCount;
    public int AssistCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroAssistCountWhenDead)
                return 0;
            return syncAssistCount.Value;
        }
    }

    protected SyncDieCountRpcComponent syncDieCount;
    public int DieCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroDieCountWhenDead)
                return 0;
            return syncDieCount.Value;
        }
    }

    public BaseNetworkGameManager NetworkManager { get; protected set; }
    public void RegisterNetworkGameManager(BaseNetworkGameManager networkManager)
    {
        NetworkManager = networkManager;
    }

    public virtual bool CanRespawn(params object[] extraParams)
    {
        if (NetworkManager != null)
            return NetworkManager.CanCharacterRespawn(this, extraParams);
        return true;
    }

    public virtual bool Respawn(params object[] extraParams)
    {
        if (NetworkManager != null)
            return NetworkManager.RespawnCharacter(this, extraParams);
        return true;
    }

    protected void InitSyncVarComponents()
    {
        Type lookupType = GetType();
        string typeName = lookupType.Name;
        if (!cachedFunctions.ContainsKey(typeName))
        {
            List<FieldInfo> cachingFields = new List<FieldInfo>();
            do
            {
                var fields = lookupType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    Type fieldType = field.FieldType;
                    if (!fieldType.IsSubclassOf(typeof(BaseSyncVarRpcComponent))) continue;
                    cachingFields.Add(field);
                    var comp = GetComponent(fieldType);
                    if (comp == null)
                        comp = gameObject.AddComponent(fieldType);
                    field.SetValue(this, comp);
                }
                lookupType = lookupType.BaseType;
            } while (lookupType != typeof(MonoBehaviourPunCallbacks));
            cachedFunctions[typeName] = cachingFields;
        }
        else
        {
            var fields = cachedFunctions[typeName];
            foreach (var field in fields)
            {
                Type fieldType = field.FieldType;
                if (!fieldType.IsSubclassOf(typeof(BaseSyncVarRpcComponent))) continue;
                var comp = GetComponent(fieldType);
                if (comp == null)
                    comp = gameObject.AddComponent(fieldType);
                field.SetValue(this, comp);
            }
        }
    }

    protected virtual void Init()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        syncScore.Value = 0;
        syncKillCount.Value = 0;
        syncAssistCount.Value = 0;
        syncDieCount.Value = 0;
    }

    protected virtual void Awake()
    {
        InitSyncVarComponents();
        Init();
    }

    protected virtual void OnStartServer()
    {
    }

    protected virtual void OnStartClient()
    {
    }

    protected virtual void OnStartLocalPlayer()
    {
    }

    protected virtual void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            OnStartServer();

        if (photonView.IsMine)
        {
            OnStartLocalPlayer();
            SetLocalPlayer();
        }

        OnStartClient();

        NetworkManager = FindObjectOfType<BaseNetworkGameManager>();
    }

    protected virtual void SetLocalPlayer()
    {
        if (Local != null)
            return;

        Local = this;
        LocalViewId = photonView.ViewID;
        LocalRank = 0;
    }

    protected virtual void Update()
    {
        if (NetworkManager != null)
            NetworkManager.OnUpdateCharacter(this);

        foreach (var obj in noTeamObjects)
        {
            obj.SetActive(PlayerTeam == 0);
        }
        foreach (var obj in teamAObjects)
        {
            obj.SetActive(PlayerTeam == 1);
        }
        foreach (var obj in teamBObjects)
        {
            obj.SetActive(PlayerTeam == 2);
        }
    }

    public void ResetScore()
    {
        syncScore.Value = 0;
    }

    public void ResetKillCount()
    {
        syncKillCount.Value = 0;
    }

    public void ResetAssistCount()
    {
        syncAssistCount.Value = 0;
    }

    public void ResetDieCount()
    {
        syncDieCount.Value = 0;
    }

    public int CompareTo(BaseNetworkGameCharacter other)
    {
        if (NetworkManager.RankedByKillCount)
            return ((-1 * KillCount.CompareTo(other.KillCount)) * 100) + ((-1 * AssistCount.CompareTo(other.AssistCount)) * 10) + photonView.ViewID.CompareTo(other.photonView.ViewID);
        else
            return ((-1 * Score.CompareTo(other.Score)) * 10) + photonView.ViewID.CompareTo(other.photonView.ViewID);
    }
}
