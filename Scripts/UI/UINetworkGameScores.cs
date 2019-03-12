using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UINetworkGameScores : MonoBehaviour
{
    public UINetworkGameScoreEntry[] userRankings;
    public UINetworkGameScoreEntry localRanking;

    public void UpdateRankings(NetworkGameScore[] rankings)
    {
        var i = 0;
        var j = 0;
        for (; i < rankings.Length; ++i)
        {
            var ranking = rankings[i];
            if (ranking.viewId > 0)
            {
                if (i < userRankings.Length)
                {
                    var userRanking = userRankings[i];
                    userRanking.SetData(j + 1, ranking);
                }

                var isLocal = BaseNetworkGameCharacter.Local != null && ranking.viewId.Equals(BaseNetworkGameCharacter.Local.photonView.ViewID);
                if (isLocal)
                    UpdateLocalRank(j + 1, ranking);
                ++j;
            }
        }

        for (; i < userRankings.Length; ++i)
        {
            var userRanking = userRankings[i];
            userRanking.Clear();
        }
    }

    public void UpdateLocalRank(int rank, NetworkGameScore ranking)
    {
        if (localRanking != null)
            localRanking.SetData(rank, ranking);
    }
}
