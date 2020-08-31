using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public struct NetworkDiscoveryData
{
    public string name;
    public string roomName;
    public string roomPassword;
    public string playerId;
    public string playerName;
    public string sceneName;
    public byte state;
    public int numPlayers;
    public int maxPlayers;
    public string gameRule;
    public int botCount;
    public int matchTime;
    public int matchKill;
    public int matchScore;
    public Hashtable fullProperties;
}
