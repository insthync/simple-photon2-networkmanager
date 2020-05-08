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
    
    private float _matchTimeCountdown = 0f;
    public float MatchTimeCountdown
    {
        get
        {
            try { return (float)PhotonNetwork.CurrentRoom.CustomProperties[MatchTimeCountdownKey]; } catch { }
            return _matchTimeCountdown;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != MatchTimeCountdown)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { MatchTimeCountdownKey, value } });
                _matchTimeCountdown = value;
            }
        }
    }

    private bool _isMatchEnded = false;
    public bool IsMatchEnded
    {
        get
        {
            try { return (bool)PhotonNetwork.CurrentRoom.CustomProperties[IsMatchEndedKey]; } catch { }
            return _isMatchEnded;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != IsMatchEnded)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { IsMatchEndedKey, value } });
                _isMatchEnded = value;
            }
        }
    }

    private int _botCount = 0;
    public int BotCount
    {
        get
        {
            try { return (int)PhotonNetwork.CurrentRoom.CustomProperties[BotCountKey]; } catch { }
            return _botCount;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != BotCount)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { BotCountKey, value } });
                _botCount = value;
            }
        }
    }

    private int _matchTime = 0;
    public int MatchTime
    {
        get
        {
            try { return (int)PhotonNetwork.CurrentRoom.CustomProperties[MatchTimeKey]; } catch { }
            return _matchTime;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != MatchTime)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { MatchTimeKey, value } });
                _matchTime = value;
            }
        }
    }

    private int _matchKill = 0;
    public int MatchKill
    {
        get
        {
            try { return (int)PhotonNetwork.CurrentRoom.CustomProperties[MatchKillKey]; } catch { }
            return _matchKill;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != MatchKill)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { MatchKillKey, value } });
                _matchKill = value;
            }
        }
    }

    private int _matchScore = 0;
    public int MatchScore
    {
        get
        {
            try { return (int)PhotonNetwork.CurrentRoom.CustomProperties[MatchScoreKey]; } catch { }
            return _matchScore;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != MatchScore)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { MatchScoreKey, value } });
                _matchScore = value;
            }
        }
    }

    private int _teamScoreA = 0;
    public int TeamScoreA
    {
        get
        {
            try { return (int)PhotonNetwork.CurrentRoom.CustomProperties[TeamScoreAKey]; } catch { }
            return _teamScoreA;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != TeamScoreA)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { TeamScoreAKey, value } });
                _teamScoreA = value;
            }
        }
    }

    private int _teamScoreB = 0;
    public int TeamScoreB
    {
        get
        {
            try { return (int)PhotonNetwork.CurrentRoom.CustomProperties[TeamScoreBKey]; } catch { }
            return _teamScoreB;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != TeamScoreB)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { TeamScoreBKey, value } });
                _teamScoreB = value;
            }
        }
    }

    private int _teamKillA = 0;
    public int TeamKillA
    {
        get
        {
            try { return (int)PhotonNetwork.CurrentRoom.CustomProperties[TeamKillAKey]; } catch { }
            return _teamKillA;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != TeamKillA)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { TeamKillAKey, value } });
                _teamKillA = value;
            }
        }
    }

    private int _teamKillB = 0;
    public int TeamKillB
    {
        get
        {
            try { return (int)PhotonNetwork.CurrentRoom.CustomProperties[TeamKillBKey]; } catch { }
            return _teamKillB;
        }
        protected set
        {
            if (PhotonNetwork.IsMasterClient && value != TeamKillB)
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable() { { TeamKillBKey, value } });
                _teamKillB = value;
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
