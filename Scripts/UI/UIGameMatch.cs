using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIGameMatch : MonoBehaviour
{
    public Text textMatchCountDown;
    public Text textMatchScore;
    public Text textMatchKill;
    public Text textTeamScoreA;
    public Text textTeamKillA;
    public Text textTeamScoreB;
    public Text textTeamKillB;

    public BaseNetworkGameManager NetworkGameManager
    {
        get { return BaseNetworkGameManager.Singleton; }
    }
    
    void Update()
    {
        if (NetworkGameManager == null || NetworkGameManager.gameRule == null)
            return;

        if (textMatchCountDown != null)
        {
            var formattedTime = string.Empty;
            var timer = NetworkGameManager.gameRule.RemainsMatchTime;
            if (timer > 0f)
            {
                int minutes = Mathf.FloorToInt(timer / 60f);
                int seconds = Mathf.FloorToInt(timer - minutes * 60);
                formattedTime = string.Format("{0:0}:{1:00}", minutes, seconds);
            }
            textMatchCountDown.text = formattedTime;
        }

        if (textMatchScore != null)
            textMatchScore.text = NetworkGameManager.gameRule.MatchScore.ToString("N0");

        if (textMatchKill != null)
            textMatchKill.text = NetworkGameManager.gameRule.MatchKill.ToString("N0");

        if (textTeamScoreA != null)
            textTeamScoreA.text = NetworkGameManager.gameRule.TeamScoreA.ToString("N0");

        if (textTeamKillA != null)
            textTeamKillA.text = NetworkGameManager.gameRule.TeamKillA.ToString("N0");

        if (textTeamScoreB != null)
            textTeamScoreB.text = NetworkGameManager.gameRule.TeamScoreB.ToString("N0");

        if (textTeamKillB != null)
            textTeamKillB.text = NetworkGameManager.gameRule.TeamKillB.ToString("N0");
    }
}
