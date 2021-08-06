using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public abstract class BaseNetworkGameCharacter : MonoBehaviourPunCallbacks, System.IComparable<BaseNetworkGameCharacter>
{
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

    protected int _score;
    public int SyncScore
    {
        get { return _score; }
        set
        {
            if (PhotonNetwork.IsMasterClient && value != SyncScore)
            {
                if (value > SyncScore && NetworkManager != null)
                    NetworkManager.OnScoreIncrease(this, value - SyncScore);
                _score = value;
                photonView.OthersRPC(RpcUpdateScore, value);
            }
        }
    }
    public int Score
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroScoreWhenDead)
                return 0;
            return SyncScore;
        }
    }

    protected int _killCount;
    public int SyncKillCount
    {
        get { return _killCount; }
        set
        {
            if (PhotonNetwork.IsMasterClient && value != SyncKillCount)
            {
                if (value > SyncKillCount && NetworkManager != null)
                    NetworkManager.OnKillIncrease(this, value - SyncKillCount);
                _killCount = value;
                photonView.OthersRPC(RpcUpdateKillCount, value);
            }
        }
    }
    public int KillCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroKillCountWhenDead)
                return 0;
            return SyncKillCount;
        }
    }

    protected int _assistCount;
    public int SyncAssistCount
    {
        get { return _assistCount; }
        set
        {
            if (PhotonNetwork.IsMasterClient && value != SyncAssistCount)
            {
                _assistCount = value;
                photonView.OthersRPC(RpcUpdateAssistCount, value);
            }
        }
    }
    public int AssistCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroAssistCountWhenDead)
                return 0;
            return SyncAssistCount;
        }
    }

    protected int _dieCount;
    public int SyncDieCount
    {
        get { return _dieCount; }
        set
        {
            if (PhotonNetwork.IsMasterClient && value != SyncDieCount)
            {
                _dieCount = value;
                photonView.OthersRPC(RpcUpdateDieCount, value);
            }
        }
    }
    public int DieCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroDieCountWhenDead)
                return 0;
            return SyncDieCount;
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

    protected virtual void Init()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        SyncScore = 0;
        SyncKillCount = 0;
        SyncAssistCount = 0;
        SyncDieCount = 0;
    }

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void SyncData()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        photonView.OthersRPC(RpcUpdateScore, SyncScore);
        photonView.OthersRPC(RpcUpdateKillCount, SyncKillCount);
        photonView.OthersRPC(RpcUpdateAssistCount, SyncAssistCount);
        photonView.OthersRPC(RpcUpdateDieCount, SyncDieCount);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        photonView.TargetRPC(RpcUpdateScore, newPlayer, SyncScore);
        photonView.TargetRPC(RpcUpdateKillCount, newPlayer, SyncKillCount);
        photonView.TargetRPC(RpcUpdateAssistCount, newPlayer, SyncAssistCount);
        photonView.TargetRPC(RpcUpdateDieCount, newPlayer, SyncDieCount);
    }

    protected virtual void OnStartServer()
    {
        SyncData();
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
        SyncScore = 0;
    }

    public void ResetKillCount()
    {
        SyncKillCount = 0;
    }

    public void ResetAssistCount()
    {
        SyncAssistCount = 0;
    }

    public void ResetDieCount()
    {
        SyncDieCount = 0;
    }

    public int CompareTo(BaseNetworkGameCharacter other)
    {
        return ((-1 * Score.CompareTo(other.Score)) * 10) + photonView.ViewID.CompareTo(other.photonView.ViewID);
    }

    #region Update RPCs
    [PunRPC]
    protected void RpcUpdateScore(int score)
    {
        _score = score;
    }
    [PunRPC]
    protected void RpcUpdateKillCount(int killCount)
    {
        _killCount = killCount;
    }
    [PunRPC]
    protected void RpcUpdateAssistCount(int assistCount)
    {
        _assistCount = assistCount;
    }
    [PunRPC]
    protected void RpcUpdateDieCount(int dieCount)
    {
        _dieCount = dieCount;
    }
    #endregion
}
