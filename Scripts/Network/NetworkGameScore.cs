[System.Serializable]
public struct NetworkGameScore
{
    public static readonly NetworkGameScore Empty = new NetworkGameScore();
    public int viewId;
    public string playerName;
    public byte team;
    public int score;
    public int killCount;
    public int assistCount;
    public int dieCount;
}
