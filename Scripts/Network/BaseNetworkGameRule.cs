using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

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
    private string title = string.Empty;
    [SerializeField, TextArea]
    private string description = string.Empty;
    [SerializeField]
    private int defaultBotCount = 0;
    [HideInInspector]
    public int botCount;
    [SerializeField, Tooltip("Time in seconds, 0 = Unlimit")]
    private int defaultMatchTime = 0;
    [HideInInspector]
    public int matchTime;
    [SerializeField, Tooltip("Match kill limit, 0 = Unlimit")]
    private int defaultMatchKill = 0;
    [HideInInspector]
    public int matchKill;
    [SerializeField, Tooltip("Match score limit, 0 = Unlimit")]
    private int defaultMatchScore = 0;
    [HideInInspector]
    public int matchScore;
    protected BaseNetworkGameManager networkManager;
    public string Title { get { return title; } }
    public string Description { get { return description; } }
    protected abstract BaseNetworkGameCharacter NewBot();
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
    public virtual bool RankedByKillCount { get { return false; } }
    public abstract bool CanCharacterRespawn(BaseNetworkGameCharacter character, params object[] extraParams);
    public abstract bool RespawnCharacter(BaseNetworkGameCharacter character, params object[] extraParams);

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
    
    private float _matchTimeCountdown = 0f;
    public float MatchTimeCountdown
    {
        get
        {
            try { return networkManager.GetRoomProperty(MatchTimeCountdownKey, _matchTimeCountdown); } catch { }
            return _matchTimeCountdown;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != MatchTimeCountdown)
            {
                networkManager.SetRoomProperty(MatchTimeCountdownKey, value, true);
                _matchTimeCountdown = value;
            }
        }
    }

    private bool _isMatchEnded = false;
    public bool IsMatchEnded
    {
        get
        {
            try { return networkManager.GetRoomProperty(IsMatchEndedKey, _isMatchEnded); } catch { }
            return _isMatchEnded;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != IsMatchEnded)
            {
                networkManager.SetRoomProperty(IsMatchEndedKey, value, true);
                _isMatchEnded = value;
            }
        }
    }

    private int _botCount = 0;
    public int BotCount
    {
        get
        {
            try { return networkManager.GetRoomProperty(BotCountKey, _botCount); } catch { }
            return _botCount;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != BotCount)
            {
                networkManager.SetRoomProperty(BotCountKey, value, true);
                _botCount = value;
            }
        }
    }

    private int _matchTime = 0;
    public int MatchTime
    {
        get
        {
            try { return networkManager.GetRoomProperty(MatchTimeKey, _matchTime); } catch { }
            return _matchTime;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != MatchTime)
            {
                networkManager.SetRoomProperty(MatchTimeKey, value, true);
                _matchTime = value;
            }
        }
    }

    private int _matchKill = 0;
    public int MatchKill
    {
        get
        {
            try { return networkManager.GetRoomProperty(MatchKillKey, _matchKill); } catch { }
            return _matchKill;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != MatchKill)
            {
                networkManager.SetRoomProperty(MatchKillKey, value, true);
                _matchKill = value;
            }
        }
    }

    private int _matchScore = 0;
    public int MatchScore
    {
        get
        {
            try { return networkManager.GetRoomProperty(MatchScoreKey, _matchScore); } catch { }
            return _matchScore;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != MatchScore)
            {
                networkManager.SetRoomProperty(MatchScoreKey, value, true);
                _matchScore = value;
            }
        }
    }

    private int _teamScoreA = 0;
    public int TeamScoreA
    {
        get
        {
            try { return networkManager.GetRoomProperty(TeamScoreAKey, _teamScoreA); } catch { }
            return _teamScoreA;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != TeamScoreA)
            {
                networkManager.SetRoomProperty(TeamScoreAKey, value, true);
                _teamScoreA = value;
            }
        }
    }

    private int _teamScoreB = 0;
    public int TeamScoreB
    {
        get
        {
            try { return networkManager.GetRoomProperty(TeamScoreBKey, _teamScoreB); } catch { }
            return _teamScoreB;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != TeamScoreB)
            {
                networkManager.SetRoomProperty(TeamScoreBKey, value, true);
                _teamScoreB = value;
            }
        }
    }

    private int _teamKillA = 0;
    public int TeamKillA
    {
        get
        {
            try { return networkManager.GetRoomProperty(TeamKillAKey, _teamKillA); } catch { }
            return _teamKillA;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != TeamKillA)
            {
                networkManager.SetRoomProperty(TeamKillAKey, value, true);
                _teamKillA = value;
            }
        }
    }

    private int _teamKillB = 0;
    public int TeamKillB
    {
        get
        {
            try { return networkManager.GetRoomProperty(TeamKillBKey, _teamKillB); } catch { }
            return _teamKillB;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != TeamKillB)
            {
                networkManager.SetRoomProperty(TeamKillBKey, value, true);
                _teamKillB = value;
            }
        }
    }

    private float matchTimeReduceTimer;

    public virtual void AddBots()
    {
        if (!HasOptionBotCount)
            return;
        int addAmount = BotCount;
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
                int countA;
                int countB;
                networkManager.CountTeamPlayers(out countA, out countB);
                character.PlayerTeam = (byte)(countA > countB ? 2 : 1);
            }
            networkManager.RegisterCharacter(character);
        }
    }

    protected virtual List<BaseNetworkGameCharacter> GetBots()
    {
        List<BaseNetworkGameCharacter> result = new List<BaseNetworkGameCharacter>(FindObjectsOfType<BaseNetworkGameCharacter>());
        for (int i = result.Count - 1; i >= 0; --i)
        {
            if (!result[i].IsBot)
                result.RemoveAt(i);
        }
        return result;
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
        List<BaseNetworkGameCharacter> bots = GetBots();
        int count = bots.Count;
        while (count > 0 && playerCount + count > maxPlayers)
        {
            int index = bots.Count - 1;
            BaseNetworkGameCharacter botCharacter = bots[index];
            PhotonNetwork.Destroy(botCharacter.photonView);
            count--;
        }

        // Add bots if needed, will add bots if added bots < bot count && player count + added bots < max players
        while (count < BotCount && playerCount + count < maxPlayers)
        {
            var character = NewBot();
            if (IsTeamGameplay)
            {
                int countA;
                int countB;
                networkManager.CountTeamPlayers(out countA, out countB);
                character.PlayerTeam = (byte)(countA > countB ? 2 : 1);
            }
            networkManager.RegisterCharacter(character);
            count++;
        }
    }

    public virtual void OnStartClient(BaseNetworkGameManager manager)
    {
        networkManager = manager;
    }

    public virtual void OnStartMaster(BaseNetworkGameManager manager)
    {
        networkManager = manager;
        BotCount = botCount;
        MatchTime = matchTime;
        MatchKill = matchKill;
        MatchScore = matchScore;
        TeamScoreA = 0;
        TeamScoreB = 0;
        TeamKillA = 0;
        TeamKillB = 0;
        MatchTimeCountdown = MatchTime;
        IsMatchEnded = false;
        AddBots();
    }

    public virtual void OnStopConnection(BaseNetworkGameManager manager)
    {
        BotCount = botCount;
        MatchTime = matchTime;
        MatchKill = matchKill;
        MatchScore = matchScore;
        TeamScoreA = 0;
        TeamScoreB = 0;
        TeamKillA = 0;
        TeamKillB = 0;
        MatchTimeCountdown = MatchTime;
        IsMatchEnded = false;
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
            switch (character.PlayerTeam)
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
            switch (character.PlayerTeam)
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
        if (IsMatchEnded)
            return;

        int checkScore = character.Score;
        int checkKill = character.KillCount;
        if (IsTeamGameplay)
        {
            // Use team score / kill as checker
            switch (character.PlayerTeam)
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


    public virtual void EndMatch()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
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
