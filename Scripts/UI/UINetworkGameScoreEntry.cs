using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;

public class UINetworkGameScoreEntry : MonoBehaviour
{
    public Text textRank;
    public Text textName;
    public Text textScore;
    public Text textKillCount;
    public Text textAssistCount;
    public Text textDieCount;
    public Text textTeam;
    public string noTeam = "None";
    public string teamA = "Team A";
    public string teamB = "Team B";
    public Color normalTextColor = Color.white;
    public Color localTextColor = Color.yellow;
    public void SetData(int rank, NetworkGameScore ranking)
    {
        Clear();
        if (ranking.Equals(NetworkGameScore.Empty) || ranking.viewId <= 0)
            return;
        if (textRank != null)
            textRank.text = "#" + rank;
        if (textName != null)
            textName.text = ranking.playerName;
        if (textScore != null)
            textScore.text = ranking.score.ToString("N0");
        if (textKillCount != null)
            textKillCount.text = ranking.killCount.ToString("N0");
        if (textAssistCount != null)
            textAssistCount.text = ranking.assistCount.ToString("N0");
        if (textDieCount != null)
            textDieCount.text = ranking.dieCount.ToString("N0");
        if (textTeam != null)
        {
            switch (ranking.team)
            {
                case 1:
                    textTeam.text = teamA;
                    break;
                case 2:
                    textTeam.text = teamB;
                    break;
                default:
                    textTeam.text = noTeam;
                    break;
            }
        }

        var isLocal = BaseNetworkGameCharacter.Local != null && ranking.viewId.Equals(BaseNetworkGameCharacter.Local.photonView.ViewID);
        SetTextColor(isLocal, textRank);
        SetTextColor(isLocal, textName);
        SetTextColor(isLocal, textScore);
        SetTextColor(isLocal, textKillCount);
        SetTextColor(isLocal, textAssistCount);
        SetTextColor(isLocal, textDieCount);
        SetTextColor(isLocal, textTeam);
    }

    public void Clear()
    {
        if (textRank != null)
            textRank.text = "";
        if (textName != null)
            textName.text = "";
        if (textScore != null)
            textScore.text = "";
        if (textKillCount != null)
            textKillCount.text = "";
        if (textAssistCount != null)
            textAssistCount.text = "";
        if (textDieCount != null)
            textDieCount.text = "";
        if (textTeam != null)
            textTeam.text = "";
    }

    private void SetTextColor(bool isLocal, Text text)
    {
        if (text == null)
            return;
        text.color = isLocal ? localTextColor : normalTextColor;
    }
}
