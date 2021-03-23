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

    protected int _score;
    protected int _killCount;
    protected int _assistCount;
    protected int _dieCount;

    public virtual string playerName
    {
        get { return (photonView != null && photonView.Owner != null) ? photonView.Owner.NickName : ""; }
        set { if (photonView.IsMine) photonView.Owner.NickName = value; }
    }
    public virtual byte playerTeam
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
    public int score
    {
        get { return _score; }
        set
        {
            if (PhotonNetwork.IsMasterClient && value != score)
            {
                if (value > score && NetworkManager != null)
                    NetworkManager.OnScoreIncrease(this, value - score);
                _score = value;
                photonView.OthersRPC(RpcUpdateScore, value);
            }
        }
    }
    public int killCount
    {
        get { return _killCount; }
        set
        {
            if (PhotonNetwork.IsMasterClient && value != killCount)
            {
                if (value > killCount && NetworkManager != null)
                    NetworkManager.OnKillIncrease(this, value - killCount);
                _killCount = value;
                photonView.OthersRPC(RpcUpdateKillCount, value);
            }
        }
    }
    public int assistCount
    {
        get { return _assistCount; }
        set
        {
            if (PhotonNetwork.IsMasterClient && value != assistCount)
            {
                _assistCount = value;
                photonView.OthersRPC(RpcUpdateAssistCount, value);
            }
        }
    }
    public int dieCount
    {
        get { return _dieCount; }
        set
        {
            if (PhotonNetwork.IsMasterClient && value != dieCount)
            {
                _dieCount = value;
                photonView.OthersRPC(RpcUpdateDieCount, value);
            }
        }
    }
    public int Score
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroScoreWhenDead)
                return 0;
            return score;
        }
    }
    public int KillCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroKillCountWhenDead)
                return 0;
            return killCount;
        }
    }
    public int AssistCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroAssistCountWhenDead)
                return 0;
            return assistCount;
        }
    }
    public int DieCount
    {
        get
        {
            if (IsDead && NetworkManager != null && NetworkManager.gameRule != null && NetworkManager.gameRule.ShowZeroDieCountWhenDead)
                return 0;
            return dieCount;
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
        score = 0;
        killCount = 0;
        assistCount = 0;
        dieCount = 0;
    }

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual void SyncData()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        photonView.OthersRPC(RpcUpdateScore, score);
        photonView.OthersRPC(RpcUpdateKillCount, killCount);
        photonView.OthersRPC(RpcUpdateAssistCount, assistCount);
        photonView.OthersRPC(RpcUpdateDieCount, dieCount);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        photonView.TargetRPC(RpcUpdateScore, newPlayer, score);
        photonView.TargetRPC(RpcUpdateKillCount, newPlayer, killCount);
        photonView.TargetRPC(RpcUpdateAssistCount, newPlayer, assistCount);
        photonView.TargetRPC(RpcUpdateDieCount, newPlayer, dieCount);
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
            obj.SetActive(playerTeam == 0);
        }
        foreach (var obj in teamAObjects)
        {
            obj.SetActive(playerTeam == 1);
        }
        foreach (var obj in teamBObjects)
        {
            obj.SetActive(playerTeam == 2);
        }
    }

    public void ResetScore()
    {
        score = 0;
    }

    public void ResetKillCount()
    {
        killCount = 0;
    }

    public void ResetAssistCount()
    {
        assistCount = 0;
    }

    public void ResetDieCount()
    {
        dieCount = 0;
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
