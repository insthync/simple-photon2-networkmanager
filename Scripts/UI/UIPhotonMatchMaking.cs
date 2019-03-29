using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPhotonMatchMaking : UIBase
{
    [Multiline]
    public string formatMatchMakingCount = "Matching...\n {0}";
    public Text textMatchMakingCount;
    public GameObject[] showingObjectsWhileMatchMaking;
    public GameObject[] hiddingObjectsWhileMatchMaking;

    private void Update()
    {
        if (textMatchMakingCount != null)
            textMatchMakingCount.text = GetTimeText();

        foreach (var obj in showingObjectsWhileMatchMaking)
        {
            obj.SetActive(SimplePhotonNetworkManager.Singleton.isMatchMaking);
        }

        foreach (var obj in hiddingObjectsWhileMatchMaking)
        {
            obj.SetActive(!SimplePhotonNetworkManager.Singleton.isMatchMaking);
        }
    }

    private string GetTimeText()
    {
        TimeSpan t = TimeSpan.FromSeconds(Time.unscaledTime - SimplePhotonNetworkManager.Singleton.startMatchMakingTime);
        return string.Format(formatMatchMakingCount, string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds));
}
}
