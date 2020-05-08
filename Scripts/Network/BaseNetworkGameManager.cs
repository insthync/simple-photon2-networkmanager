﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public abstract class BaseNetworkGameManager : SimplePhotonNetworkManager
{
    public static new BaseNetworkGameManager Singleton
    {
        get { return SimplePhotonNetworkManager.Singleton as BaseNetworkGameManager; }
    }

    public PhotonTeamsManager Teams { get; private set; }

    public const string CUSTOM_ROOM_GAME_RULE = "G";
    public const string CUSTOM_ROOM_GAME_RULE_BOT_COUNT = "Gbc";
    public const string CUSTOM_ROOM_GAME_RULE_MATCH_TIME = "Gmt";
    public const string CUSTOM_ROOM_GAME_RULE_MATCH_KILL = "Gmk";
    public const string CUSTOM_ROOM_GAME_RULE_MATCH_SCORE = "Gms";

    public BaseNetworkGameRule gameRule;
    protected float updateScoreTime;
    protected float updateMatchTime;
    protected bool startUpdateGameRule;
    public readonly List<BaseNetworkGameCharacter> Characters = new List<BaseNetworkGameCharacter>();
    public float RemainsMatchTime { get; protected set; }
    public bool IsMatchEnded { get; protected set; }
    public float MatchEndedAt { get; protected set; }

    private void Start()
    {
        // Setup required components
        if (Teams == null)
            Teams = GetComponent<PhotonTeamsManager>();
        if (Teams == null)
            Teams = gameObject.AddComponent<PhotonTeamsManager>();
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

    public override void LeaveRoom()
    {
        if (gameRule != null)
            gameRule.OnStopConnection(this);
        base.LeaveRoom();
    }

    public override void Disconnect()
    {
        if (gameRule != null)
            gameRule.OnStopConnection(this);
        base.Disconnect();
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
            CUSTOM_ROOM_GAME_RULE,
            CUSTOM_ROOM_GAME_RULE_BOT_COUNT,
            CUSTOM_ROOM_GAME_RULE_MATCH_TIME,
            CUSTOM_ROOM_GAME_RULE_MATCH_KILL,
            CUSTOM_ROOM_GAME_RULE_MATCH_SCORE,
        };
    }

    public override void OnRoomListUpdate(List<RoomInfo> rooms)
    {
        var foundRooms = new List<NetworkDiscoveryData>();
        foreach (var room in rooms)
        {
            var customProperties = room.CustomProperties;
            if (customProperties.Count == 0)
                continue;
            var isMatchMaking = (bool)customProperties[CUSTOM_ROOM_MATCH_MAKE];
            if (!isMatchMaking)
            {
                var discoveryData = new NetworkDiscoveryData();
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
                foundRooms.Add(discoveryData);
            }
        }
        if (onReceivedRoomListUpdate != null)
            onReceivedRoomListUpdate.Invoke(foundRooms);
    }

    protected virtual void ServerUpdate()
    {
        if (GetRoomState() == RoomState.Waiting)
            return;

        if (gameRule != null && startUpdateGameRule)
            gameRule.OnUpdate();

        if (Time.unscaledTime - updateScoreTime >= 1f)
        {
            if (gameRule != null && !gameRule.IsMatchEnded)
            {
                int length = 0;
                List<object> objects;
                GetSortedScoresAsObjects(out length, out objects);
                if (isConnectOffline)
                    RpcUpdateScores(length, objects.ToArray());
                else
                    photonView.RPC("RpcUpdateScores", RpcTarget.All, length, objects.ToArray());
            }
            updateScoreTime = Time.unscaledTime;
        }

        if (gameRule != null && Time.unscaledTime - updateMatchTime >= 1f)
        {
            RemainsMatchTime = gameRule.RemainsMatchTime;
            if (isConnectOffline)
                RpcMatchStatus(gameRule.RemainsMatchTime, gameRule.IsMatchEnded);
            else
                photonView.RPC("RpcMatchStatus", RpcTarget.All, gameRule.RemainsMatchTime, gameRule.IsMatchEnded);

            if (!IsMatchEnded && gameRule.IsMatchEnded)
            {
                IsMatchEnded = true;
                MatchEndedAt = Time.unscaledTime;
            }

            updateMatchTime = Time.unscaledTime;
        }
    }

    protected virtual void ClientUpdate()
    {

    }

    public void SendKillNotify(string killerName, string victimName, string weaponId)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        photonView.RPC("RpcKillNotify", RpcTarget.All, killerName, victimName, weaponId);
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
            var ranking = new NetworkGameScore();
            ranking.viewId = character.photonView.ViewID;
            ranking.playerName = character.playerName;
            ranking.team = character.playerTeam;
            ranking.score = character.Score;
            ranking.killCount = character.KillCount;
            ranking.assistCount = character.AssistCount;
            ranking.dieCount = character.DieCount;
            scores[i] = ranking;
        }
        return scores;
    }

    public void GetSortedScoresAsObjects(out int length, out List<object> objects)
    {
        length = 0;
        objects = new List<object>();
        var sortedScores = GetSortedScores();
        length = (sortedScores.Length);
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
        if (gameRule != null)
            gameRule.OnScoreIncrease(character, increaseAmount);
    }

    public virtual void OnKillIncrease(BaseNetworkGameCharacter character, int increaseAmount)
    {
        if (gameRule != null)
            gameRule.OnKillIncrease(character, increaseAmount);
    }

    public virtual void OnUpdateCharacter(BaseNetworkGameCharacter character)
    {
        if (gameRule != null)
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
        // Reset last game/match data
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

    public override void OnOnlineSceneChanged()
    {
        if (isLog) Debug.Log("OnOnlineSceneChanged");
        // Reset last game/match data
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
            if (PhotonNetwork.IsMasterClient && !startUpdateGameRule)
            {
                startUpdateGameRule = true;
                gameRule.OnStartServer(this);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
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
        startUpdateGameRule = true;
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
                int length = 0;
                List<object> objects;
                GetSortedScoresAsObjects(out length, out objects);
                photonView.RPC("RpcUpdateScores", newPlayer, length, objects.ToArray());
                if (gameRule != null)
                    photonView.RPC("RpcMatchStatus", newPlayer, gameRule.RemainsMatchTime, gameRule.IsMatchEnded);
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
            player.LeaveCurrentTeam();
        }
        else
        {
            // TODO: Improve team codes
            Player[] players1;
            Player[] players2;
            if (Teams.TryGetTeamMembers(1, out players1) &&
                Teams.TryGetTeamMembers(2, out players2))
            {
                if (players1.Length > players2.Length)
                    player.JoinOrSwitchTeam(2);
                else
                    player.JoinOrSwitchTeam(1);
            }
        }
    }

    public void ChangePlayerTeam()
    {
        photonView.RPC("RpcChangePlayerTeam", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.UserId);
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
                foundPlayer.LeaveCurrentTeam();
            }
            else
            {
                var maxPlayerEachTeam = PhotonNetwork.CurrentRoom.MaxPlayers / 2;
                // TODO: Improve team codes
                Player[] players1;
                Player[] players2;
                if (Teams.TryGetTeamMembers(1, out players1) &&
                    Teams.TryGetTeamMembers(2, out players2))
                {
                    if (foundPlayer.GetPhotonTeam() == null)
                    {
                        if (players1.Length > players2.Length)
                            foundPlayer.JoinOrSwitchTeam(2);
                        else
                            foundPlayer.JoinOrSwitchTeam(1);
                    }
                    else
                    {
                        switch (foundPlayer.GetPhotonTeam().Code)
                        {
                            case 1:
                                if (players1.Length < maxPlayerEachTeam)
                                    foundPlayer.JoinOrSwitchTeam(2);
                                break;
                            case 2:
                                if (players2.Length < maxPlayerEachTeam)
                                    foundPlayer.JoinOrSwitchTeam(1);
                                break;
                        }
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
        startUpdateGameRule = false;
    }

    protected abstract void UpdateScores(NetworkGameScore[] scores);
    protected abstract void KillNotify(string killerName, string victimName, string weaponId);
}
