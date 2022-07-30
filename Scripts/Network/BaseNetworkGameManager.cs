using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public abstract class BaseNetworkGameManager : SimplePhotonNetworkManager
{
    public static new BaseNetworkGameManager Singleton
    {
        get { return SimplePhotonNetworkManager.Singleton as BaseNetworkGameManager; }
    }

    public const string CUSTOM_ROOM_GAME_RULE = "G";
    public const string CUSTOM_ROOM_GAME_RULE_BOT_COUNT = "Gbc";
    public const string CUSTOM_ROOM_GAME_RULE_MATCH_TIME = "Gmt";
    public const string CUSTOM_ROOM_GAME_RULE_MATCH_KILL = "Gmk";
    public const string CUSTOM_ROOM_GAME_RULE_MATCH_SCORE = "Gms";

    public BaseNetworkGameRule gameRule;
    protected float updateScoreTime;
    protected float updateMatchTime;
    public readonly List<BaseNetworkGameCharacter> Characters = new List<BaseNetworkGameCharacter>();
    public float RemainsMatchTime { get; protected set; }
    public bool IsMatchEnded { get; protected set; }
    public float MatchEndedAt { get; protected set; }
    public bool MasterStarted { get; protected set; }
    public bool ClientStarted { get; protected set; }
    public bool RankedByKillCount
    {
        get
        {
            if (gameRule != null)
                return gameRule.RankedByKillCount;
            return false;
        }
    }

    public int CountAliveCharacters()
    {
        var count = 0;
        foreach (var character in Characters)
        {
            if (character == null)
                continue;
            if (!character.IsDead)
                ++count;
        }
        return count;
    }

    public int CountDeadCharacters()
    {
        var count = 0;
        foreach (var character in Characters)
        {
            if (character == null)
                continue;
            if (character.IsDead)
                ++count;
        }
        return count;
    }

    protected override void Update()
    {
        base.Update();
        if (PhotonNetwork.IsMasterClient)
            ServerUpdate();
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
            ClientUpdate();
    }

    protected override string[] GetCustomRoomPropertiesForLobby()
    {
        return new string[]
        {
            CUSTOM_ROOM_ROOM_NAME,
            CUSTOM_ROOM_ROOM_PASSWORD,
            CUSTOM_ROOM_PLAYER_ID,
            CUSTOM_ROOM_PLAYER_NAME,
            CUSTOM_ROOM_SCENE_NAME,
            CUSTOM_ROOM_MATCH_MAKE,
            CUSTOM_ROOM_STATE,
            CUSTOM_ROOM_BOTS_TEAMS,
            CUSTOM_ROOM_GAME_RULE,
            CUSTOM_ROOM_GAME_RULE_BOT_COUNT,
            CUSTOM_ROOM_GAME_RULE_MATCH_TIME,
            CUSTOM_ROOM_GAME_RULE_MATCH_KILL,
            CUSTOM_ROOM_GAME_RULE_MATCH_SCORE,
        };
    }

    protected override bool CanAddRoom(RoomInfo room, out NetworkDiscoveryData discoveryData)
    {
        discoveryData = new NetworkDiscoveryData();
        var customProperties = room.CustomProperties;
        if (customProperties.Count == 0)
            return false;
        var isMatchMaking = (bool)customProperties[CUSTOM_ROOM_MATCH_MAKE];
        if (isMatchMaking)
            return false;
        discoveryData.name = room.Name;
        discoveryData.roomName = (string)customProperties[CUSTOM_ROOM_ROOM_NAME];
        discoveryData.roomPassword = (string)customProperties[CUSTOM_ROOM_ROOM_PASSWORD];
        discoveryData.playerId = (string)customProperties[CUSTOM_ROOM_PLAYER_ID];
        discoveryData.playerName = (string)customProperties[CUSTOM_ROOM_PLAYER_NAME];
        discoveryData.sceneName = (string)customProperties[CUSTOM_ROOM_SCENE_NAME];
        discoveryData.state = (byte)customProperties[CUSTOM_ROOM_STATE];
        discoveryData.numPlayers = room.PlayerCount;
        discoveryData.maxPlayers = room.MaxPlayers;
        discoveryData.gameRule = (string)customProperties[CUSTOM_ROOM_GAME_RULE];
        discoveryData.botCount = (int)customProperties[CUSTOM_ROOM_GAME_RULE_BOT_COUNT];
        discoveryData.matchTime = (int)customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_TIME];
        discoveryData.matchKill = (int)customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_KILL];
        discoveryData.matchScore = (int)customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_SCORE];
        discoveryData.fullProperties = customProperties;
        return true;
    }

    protected virtual void ServerUpdate()
    {
        if (GetRoomState() == RoomState.Waiting)
            return;

        if (!MasterStarted)
            return;

        if (gameRule != null)
        {
            gameRule.OnUpdate();

            if (!IsMatchEnded && gameRule.IsMatchEnded)
            {
                UpdateMatchScores();
                if (isConnectOffline)
                    RpcMatchStatus(gameRule.RemainsMatchTime, gameRule.IsMatchEnded);
                else
                    photonView.OthersRPC(RpcMatchStatus, gameRule.RemainsMatchTime, gameRule.IsMatchEnded);
                IsMatchEnded = true;
                MatchEndedAt = Time.unscaledTime;
            }

            if (!IsMatchEnded && Time.unscaledTime - updateMatchTime >= 1f)
            {
                RemainsMatchTime = gameRule.RemainsMatchTime;
                if (isConnectOffline)
                    RpcMatchStatus(gameRule.RemainsMatchTime, gameRule.IsMatchEnded);
                else
                    photonView.OthersRPC(RpcMatchStatus, gameRule.RemainsMatchTime, gameRule.IsMatchEnded);
                updateMatchTime = Time.unscaledTime;
            }
        }

        if (!IsMatchEnded && Time.unscaledTime - updateScoreTime >= 1f)
        {
            if (gameRule == null || !gameRule.IsMatchEnded)
                UpdateMatchScores();
            updateScoreTime = Time.unscaledTime;
        }
    }

    protected virtual void ClientUpdate()
    {

    }

    protected void UpdateMatchScores()
    {
        int length;
        List<object> objects;
        GetSortedScoresAsObjects(out length, out objects);
        if (isConnectOffline)
            RpcUpdateScores(length, objects.ToArray());
        else
            photonView.AllRPC(RpcUpdateScores, length, objects.ToArray());
    }

    public void SendKillNotify(string killerName, string victimName, string weaponId)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        photonView.AllRPC(RpcKillNotify, killerName, victimName, weaponId);
    }

    public NetworkGameScore[] GetSortedScores()
    {
        for (var i = Characters.Count - 1; i >= 0; --i)
        {
            var character = Characters[i];
            if (character == null)
                Characters.RemoveAt(i);
        }
        Characters.Sort();
        var scores = new NetworkGameScore[Characters.Count];
        for (var i = 0; i < Characters.Count; ++i)
        {
            var character = Characters[i];
            var score = new NetworkGameScore();
            score.viewId = character.photonView.ViewID;
            score.playerName = character.PlayerName;
            score.team = character.PlayerTeam;
            score.score = character.Score;
            score.killCount = character.KillCount;
            score.assistCount = character.AssistCount;
            score.dieCount = character.DieCount;
            if (score.viewId == BaseNetworkGameCharacter.LocalViewId)
                BaseNetworkGameCharacter.LocalRank = i + 1;
            scores[i] = score;
        }
        return scores;
    }

    public void GetSortedScoresAsObjects(out int length, out List<object> objects)
    {
        objects = new List<object>();
        var sortedScores = GetSortedScores();
        length = sortedScores.Length;
        foreach (var sortedScore in sortedScores)
        {
            objects.Add(sortedScore.viewId);
            objects.Add(sortedScore.playerName);
            objects.Add(sortedScore.team);
            objects.Add(sortedScore.score);
            objects.Add(sortedScore.killCount);
            objects.Add(sortedScore.assistCount);
            objects.Add(sortedScore.dieCount);
        }
    }

    public void RegisterCharacter(BaseNetworkGameCharacter character)
    {
        if (character == null || Characters.Contains(character))
            return;
        character.RegisterNetworkGameManager(this);
        Characters.Add(character);
    }

    public bool CanCharacterRespawn(BaseNetworkGameCharacter character, params object[] extraParams)
    {
        if (gameRule != null)
            return gameRule.CanCharacterRespawn(character, extraParams);
        return true;
    }

    public bool RespawnCharacter(BaseNetworkGameCharacter character, params object[] extraParams)
    {
        if (gameRule != null)
            return gameRule.RespawnCharacter(character, extraParams);
        return true;
    }

    public virtual void OnScoreIncrease(BaseNetworkGameCharacter character, int increaseAmount)
    {
        if (gameRule != null && Characters.Contains(character))
            gameRule.OnScoreIncrease(character, increaseAmount);
    }

    public virtual void OnKillIncrease(BaseNetworkGameCharacter character, int increaseAmount)
    {
        if (gameRule != null && Characters.Contains(character))
            gameRule.OnKillIncrease(character, increaseAmount);
    }

    public virtual void OnUpdateCharacter(BaseNetworkGameCharacter character)
    {
        if (gameRule != null && Characters.Contains(character))
            gameRule.OnUpdateCharacter(character);
    }

    public void SetGameRule(BaseNetworkGameRule gameRule)
    {
        // If room not created, set data to field to use later
        this.gameRule = gameRule;
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            Hashtable customProperties = new Hashtable();
            customProperties[CUSTOM_ROOM_GAME_RULE] = gameRule == null ? "" : gameRule.name;
            customProperties[CUSTOM_ROOM_GAME_RULE_BOT_COUNT] = gameRule == null ? 0 : gameRule.botCount;
            customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_TIME] = gameRule == null ? 0 : gameRule.matchTime;
            customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_KILL] = gameRule == null ? 0 : gameRule.matchKill;
            customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_SCORE] = gameRule == null ? 0 : gameRule.matchScore;
            PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

            foreach (var player in PhotonNetwork.PlayerList)
            {
                SetPlayerTeam(player);
            }
        }
    }

    public override void OnCreatedRoom()
    {
        ResetGame();
        Hashtable customProperties = new Hashtable();
        customProperties[CUSTOM_ROOM_GAME_RULE] = gameRule == null ? "" : gameRule.name;
        customProperties[CUSTOM_ROOM_GAME_RULE_BOT_COUNT] = gameRule == null ? 0 : gameRule.botCount;
        customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_TIME] = gameRule == null ? 0 : gameRule.matchTime;
        customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_KILL] = gameRule == null ? 0 : gameRule.matchKill;
        customProperties[CUSTOM_ROOM_GAME_RULE_MATCH_SCORE] = gameRule == null ? 0 : gameRule.matchScore;
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
        base.OnCreatedRoom();
    }

    public override void OnLeftRoom()
    {
        if (gameRule != null)
            gameRule.OnStopConnection(this);
        ResetGame();
        base.OnLeftRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (gameRule != null)
            gameRule.OnStopConnection(this);
        ResetGame();
        base.OnDisconnected(cause);
    }

    public override void OnOnlineSceneChanged()
    {
        if (isLog) Debug.Log("OnOnlineSceneChanged");
        ResetGame();
        onlineSceneLoaded = true;
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CUSTOM_ROOM_GAME_RULE))
        {
            InitGameRule((string)PhotonNetwork.CurrentRoom.CustomProperties[CUSTOM_ROOM_GAME_RULE]);
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        if (propertiesThatChanged.ContainsKey(CUSTOM_ROOM_GAME_RULE))
        {
            InitGameRule((string)propertiesThatChanged[CUSTOM_ROOM_GAME_RULE]);
        }
    }

    protected void InitGameRule(string gameRuleName)
    {
        BaseNetworkGameRule foundGameRule;
        if (BaseNetworkGameInstance.GameRules.TryGetValue(gameRuleName, out foundGameRule) && onlineSceneLoaded)
        {
            gameRule = foundGameRule;
            gameRule.InitialClientObjects();
            if (PhotonNetwork.IsMasterClient && !MasterStarted)
            {
                MasterStarted = true;
                gameRule.OnStartMaster(this);
            }
            if (!PhotonNetwork.IsMasterClient && !ClientStarted)
            {
                ClientStarted = true;
                gameRule.OnStartClient(this);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        if (newMasterClient.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            MasterStarted = false;
            return;
        }
        if (GetRoomState() == RoomState.Playing)
        {
            Characters.Clear();
            var characters = FindObjectsOfType<BaseNetworkGameCharacter>();
            foreach (var character in characters)
            {
                Characters.Add(character);
            }
            if (gameRule != null)
                gameRule.OnMasterChange(this);
        }
        MasterStarted = true;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
            SetPlayerTeam(PhotonNetwork.LocalPlayer);
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (GetRoomState() == RoomState.Playing && gameRule != null && !gameRule.IsMatchEnded)
            {
                if (gameRule != null)
                    photonView.TargetRPC(RpcMatchStatus, newPlayer, gameRule.RemainsMatchTime, gameRule.IsMatchEnded);
                // Adjust bots
                gameRule.AdjustBots();
            }
            SetPlayerTeam(newPlayer);
        }
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            if (GetRoomState() == RoomState.Playing && gameRule != null && !gameRule.IsMatchEnded)
            {
                // Adjust bots
                gameRule.AdjustBots();
            }
        }
    }

    protected void SetPlayerTeam(Player player)
    {
        bool isTeamGameplay = gameRule != null && gameRule.IsTeamGameplay;
        if (!isTeamGameplay)
        {
            LeaveTeam(player);
        }
        else
        {
            int countA;
            int countB;
            CountTeamPlayers(out countA, out countB);
            if (countA > countB)
                SetTeam(player, 2);
            else
                SetTeam(player, 1);
        }
    }

    public void ChangePlayerTeam()
    {
        photonView.MasterRPC(RpcChangePlayerTeam, PhotonNetwork.LocalPlayer.UserId);
    }

    [PunRPC]
    protected void RpcUpdateScores(int length, object[] objects)
    {
        if (length == 0 || objects == null)
            return;
        var scores = new NetworkGameScore[length];
        var j = 0;
        for (var i = 0; i < length; ++i)
        {
            var score = new NetworkGameScore();
            score.viewId = (int)objects[j++];
            score.playerName = (string)objects[j++];
            score.team = (byte)objects[j++];
            score.score = (int)objects[j++];
            score.killCount = (int)objects[j++];
            score.assistCount = (int)objects[j++];
            score.dieCount = (int)objects[j++];
            if (score.viewId == BaseNetworkGameCharacter.LocalViewId)
                BaseNetworkGameCharacter.LocalRank = i + 1;
            scores[i] = score;
        }
        UpdateScores(scores);
    }

    [PunRPC]
    protected void RpcMatchStatus(float remainsMatchTime, bool isMatchEnded)
    {
        RemainsMatchTime = remainsMatchTime;
        if (!IsMatchEnded && isMatchEnded)
        {
            IsMatchEnded = true;
            MatchEndedAt = Time.unscaledTime;
        }
    }

    [PunRPC]
    protected void RpcChangePlayerTeam(string id)
    {
        Player foundPlayer = GetPlayerById(id);
        if (foundPlayer != null)
        {
            bool isTeamGameplay = gameRule != null && gameRule.IsTeamGameplay;
            if (!isTeamGameplay)
            {
                LeaveTeam(foundPlayer);
            }
            else
            {
                var maxPlayerEachTeam = PhotonNetwork.CurrentRoom.MaxPlayers / 2;
                int countA;
                int countB;
                CountTeamPlayers(out countA, out countB);
                if (GetTeam(foundPlayer) == 0)
                {
                    if (countA > countB)
                        SetTeam(foundPlayer, 2);
                    else
                        SetTeam(foundPlayer, 1);
                }
                else
                {
                    switch (GetTeam(foundPlayer))
                    {
                        case 1:
                            if (countA < maxPlayerEachTeam)
                                SetTeam(foundPlayer, 2);
                            break;
                        case 2:
                            if (countB < maxPlayerEachTeam)
                                SetTeam(foundPlayer, 1);
                            break;
                    }
                }
            }
        }
    }

    [PunRPC]
    protected void RpcKillNotify(string killerName, string victimName, string weaponId)
    {
        KillNotify(killerName, victimName, weaponId);
    }

    protected void ResetGame()
    {
        Characters.Clear();
        updateScoreTime = 0f;
        updateMatchTime = 0f;
        RemainsMatchTime = 0f;
        IsMatchEnded = false;
        MatchEndedAt = 0f;
        MasterStarted = false;
        ClientStarted = false;
    }

    protected abstract void UpdateScores(NetworkGameScore[] scores);
    protected abstract void KillNotify(string killerName, string victimName, string weaponId);
}
