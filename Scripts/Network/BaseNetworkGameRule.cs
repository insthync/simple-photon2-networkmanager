using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public abstract class BaseNetworkGameRule : ScriptableObject
{
    public const string MatchTimeCountdownKey = "rCD";
    public const string IsMatchEndedKey = "rMN";
    public const string BotCountKey = "rBC";
    public const string MatchTimeKey = "rMT";
    public const string MatchKillKey = "rMK";
    public const string MatchScoreKey = "rMS";
    public const string TeamScoreAKey = "tSA";
    public const string TeamScoreBKey = "tSB";
    public const string TeamKillAKey = "tKA";
    public const string TeamKillBKey = "tKB";

    [SerializeField]
    private string title;
    [SerializeField, TextArea]
    private string description;
    [SerializeField]
    private int defaultBotCount;
    [HideInInspector]
    public int botCount;
    [SerializeField, Tooltip("Time in seconds, 0 = Unlimit")]
    private int defaultMatchTime;
    [HideInInspector]
    public int matchTime;
    [SerializeField, Tooltip("Match kill limit, 0 = Unlimit")]
    private int defaultMatchKill;
    [HideInInspector]
    public int matchKill;
    [SerializeField, Tooltip("Match score limit, 0 = Unlimit")]
    private int defaultMatchScore;
    [HideInInspector]
    public int matchScore;
    protected BaseNetworkGameManager networkManager;
    public string Title { get { return title; } }
    public string Description { get { return description; } }
    protected abstract BaseNetworkGameCharacter NewBot();
    protected abstract void EndMatch();
    public int DefaultBotCount { get { return defaultBotCount; } }
    public int DefaultMatchTime { get { return defaultMatchTime; } }
    public int DefaultMatchKill { get { return defaultMatchKill; } }
    public int DefaultMatchScore { get { return defaultMatchScore; } }
    public virtual bool HasOptionBotCount { get { return false; } }
    public virtual bool HasOptionMatchTime { get { return false; } }
    public virtual bool HasOptionMatchKill { get { return false; } }
    public virtual bool HasOptionMatchScore { get { return false; } }
    public virtual bool IsTeamGameplay { get { return false; } }
    public virtual bool ShowZeroScoreWhenDead { get { return false; } }
    public virtual bool ShowZeroKillCountWhenDead { get { return false; } }
    public virtual bool ShowZeroAssistCountWhenDead { get { return false; } }
    public virtual bool ShowZeroDieCountWhenDead { get { return false; ; } }
    public abstract bool CanCharacterRespawn(BaseNetworkGameCharacter character, params object[] extraParams);
    public abstract bool RespawnCharacter(BaseNetworkGameCharacter character, params object[] extraParams);

    protected readonly List<BaseNetworkGameCharacter> Bots = new List<BaseNetworkGameCharacter>();
    protected readonly Dictionary<int, int> CharacterCollectedScore = new Dictionary<int, int>();
    protected readonly Dictionary<int, int> CharacterCollectedKill = new Dictionary<int, int>();

    public float RemainsMatchTime
    {
        get
        {
            if (HasOptionMatchTime && MatchTime > 0 && MatchTimeCountdown > 0 && !IsMatchEnded)
                return MatchTimeCountdown;
            return 0f;
        }
    }
    
    public int MatchTimeCountdown
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(MatchTimeCountdownKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(MatchTimeCountdownKey, value);
            }
        }
    }
    
    public bool IsMatchEnded
    {
        get
        {
            return (bool)SimplePhotonNetworkManager.Singleton.GetRoomProperty(IsMatchEndedKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(IsMatchEndedKey, value);
            }
        }
    }
    
    public int BotCount
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(BotCountKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(BotCountKey, value);
            }
        }
    }
    
    public int MatchTime
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(MatchTimeKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(MatchTimeKey, value);
            }
        }
    }
    
    public int MatchKill
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(MatchKillKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(MatchKillKey, value);
            }
        }
    }
    
    public int MatchScore
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(MatchScoreKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(MatchScoreKey, value);
            }
        }
    }
    
    public int TeamScoreA
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(TeamScoreAKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(TeamScoreAKey, value);
            }
        }
    }
    
    public int TeamScoreB
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(TeamScoreBKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(TeamScoreBKey, value);
            }
        }
    }
    
    public int TeamKillA
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(TeamKillAKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(TeamKillAKey, value);
            }
        }
    }
    
    public int TeamKillB
    {
        get
        {
            return (int)SimplePhotonNetworkManager.Singleton.GetRoomProperty(TeamKillBKey);
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SimplePhotonNetworkManager.Singleton.SetRoomProperty(TeamKillBKey, value);
            }
        }
    }

    private float matchTimeReduceTimer;
    private byte tempTeam;

    public virtual void AddBots()
    {
        if (!HasOptionBotCount)
            return;
        int addAmount = BotCount;
        Bots.Clear();
        // Adjust bot count
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        if (networkManager.isConnectOffline)
            maxPlayers = networkManager.maxConnections;
        if (PhotonNetwork.CurrentRoom.PlayerCount + addAmount > maxPlayers)
            addAmount = maxPlayers - PhotonNetwork.CurrentRoom.PlayerCount;
        for (var i = 0; i < addAmount; ++i)
        {
            var character = NewBot();
            if (IsTeamGameplay)
            {
                // TODO: Improve team codes
                character.playerTeam = tempTeam = (byte)(tempTeam == 1 ? 2 : 1);
            }
            networkManager.RegisterCharacter(character);
            Bots.Add(character);
        }
    }

    public virtual void AdjustBots()
    {
        if (!HasOptionBotCount)
            return;

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        if (networkManager.isConnectOffline)
            maxPlayers = networkManager.maxConnections;

        // Remove bots if needed, will remove bots if player count + added bots > max players
        while (Bots.Count > 0 && playerCount + Bots.Count > maxPlayers)
        {
            int index = Bots.Count - 1;
            BaseNetworkGameCharacter botCharacter = Bots[index];
            PhotonNetwork.Destroy(botCharacter.photonView);
            Bots.RemoveAt(index);
        }

        // Add bots if needed, will add bots if added bots < bot count && player count + added bots < max players
        while (Bots.Count < BotCount && playerCount + Bots.Count < maxPlayers)
        {
            var character = NewBot();
            if (IsTeamGameplay)
            {
                // TODO: Improve team codes
                character.playerTeam = tempTeam = (byte)(tempTeam == 1 ? 2 : 1);
            }
            networkManager.RegisterCharacter(character);
            Bots.Add(character);
        }
    }

    public virtual void OnStartServer(BaseNetworkGameManager manager)
    {
        networkManager = manager;
        BotCount = botCount;
        MatchTime = matchTime;
        MatchKill = matchKill;
        MatchScore = matchScore;
        MatchTimeCountdown = MatchTime;
        IsMatchEnded = false;
        AddBots();
    }

    public virtual void OnStopConnection(BaseNetworkGameManager manager)
    {

    }

    public virtual void OnMasterChange(BaseNetworkGameManager manager)
    {
        networkManager = manager;
    }

    public virtual void OnUpdate()
    {
        // Make match time reduce every seconds (not every loops)
        matchTimeReduceTimer += Time.unscaledDeltaTime;
        if (matchTimeReduceTimer >= 1 && MatchTimeCountdown > 0)
        {
            matchTimeReduceTimer = 0;
            MatchTimeCountdown--;
        }

        if (HasOptionMatchTime && MatchTime > 0 && MatchTimeCountdown <= 0 && !IsMatchEnded)
        {
            IsMatchEnded = true;
            EndMatch();
        }
    }

    public virtual void OnScoreIncrease(BaseNetworkGameCharacter character, int increaseAmount)
    {
        if (!CharacterCollectedScore.ContainsKey(character.photonView.ViewID))
            CharacterCollectedScore[character.photonView.ViewID] = increaseAmount;
        else
            CharacterCollectedScore[character.photonView.ViewID] += increaseAmount;

        if (IsTeamGameplay)
        {
            // TODO: Improve team codes
            switch (character.playerTeam)
            {
                case 1:
                    TeamScoreA += increaseAmount;
                    break;
                case 2:
                    TeamScoreB += increaseAmount;
                    break;
            }
        }
    }

    public virtual void OnKillIncrease(BaseNetworkGameCharacter character, int increaseAmount)
    {
        if (!CharacterCollectedKill.ContainsKey(character.photonView.ViewID))
            CharacterCollectedKill[character.photonView.ViewID] = increaseAmount;
        else
            CharacterCollectedKill[character.photonView.ViewID] += increaseAmount;

        if (IsTeamGameplay)
        {
            // TODO: Improve team codes
            switch (character.playerTeam)
            {
                case 1:
                    TeamKillA += increaseAmount;
                    break;
                case 2:
                    TeamKillB += increaseAmount;
                    break;
            }
        }
    }

    public virtual void OnUpdateCharacter(BaseNetworkGameCharacter character)
    {
        int checkScore = character.Score;
        int checkKill = character.KillCount;
        if (IsTeamGameplay)
        {
            // Use team score / kill as checker
            switch (character.playerTeam)
            {
                case 1:
                    checkScore = TeamScoreA;
                    checkKill = TeamKillA;
                    break;
                case 2:
                    checkScore = TeamScoreB;
                    checkKill = TeamKillB;
                    break;
            }
        }

        if (HasOptionMatchScore && MatchScore > 0 && checkScore >= MatchScore)
        {
            IsMatchEnded = true;
            EndMatch();
        }

        if (HasOptionMatchKill && MatchKill > 0 && checkKill >= MatchKill)
        {
            IsMatchEnded = true;
            EndMatch();
        }
    }

    public virtual void InitData()
    {
        botCount = DefaultBotCount;
        matchTime = DefaultMatchTime;
        matchKill = DefaultMatchKill;
        matchScore = DefaultMatchScore;
    }

    public abstract void InitialClientObjects();
}
