using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRegion : MonoBehaviour
{
    [System.Serializable]
    public struct RegionName
    {
        public string code;
        public string name;
    }

    public GameObject lowPingSign;
    public Color lowPingColor = Color.green;
    public int highPing = 100;
    public GameObject highPingSign;
    public Color highPingColor = Color.yellow;
    public int veryHighPing = 200;
    public GameObject veryHighPingSign;
    public Color veryHighPingColor = Color.red;
    public Text textPing;
    public Text textRegionCode;
    public RegionName[] regionNames;
    public Text textRegionName;
    public Region Data { get; private set; }

    private Dictionary<string, string> cacheRegionNames;
    public Dictionary<string, string> CacheRegionNames
    {
        get
        {
            if (cacheRegionNames == null)
            {
                cacheRegionNames = new Dictionary<string, string>();
                foreach (var regionName in regionNames)
                {
                    cacheRegionNames[regionName.code] = regionName.name;
                }
            }
            return cacheRegionNames;
        }
    }

    private void Update()
    {
        if (Data == null)
            return;

        if (lowPingSign)
            lowPingSign.SetActive(Data.Ping < highPing);

        if (highPingSign)
            highPingSign.SetActive(Data.Ping >= highPing && Data.Ping < veryHighPing);

        if (veryHighPingSign)
            veryHighPingSign.SetActive(Data.Ping >= veryHighPing);

        if (textPing)
        {
            textPing.text = Data.WasPinged ? Data.Ping.ToString("N0") : "N/A";
            textPing.color = lowPingColor;
            if (Data.Ping >= highPing)
                textPing.color = highPingColor;
            if (Data.Ping >= veryHighPing)
                textPing.color = veryHighPingColor;
        }
    }

    public void SetData(Region data)
    {
        Data = data;

        if (textRegionCode)
            textRegionCode.text = Data.Code;

        if (textRegionName)
            textRegionName.text = CacheRegionNames.ContainsKey(Data.Code) ? CacheRegionNames[Data.Code] : Data.Code;
    }

    public void OnClickSelectRegion()
    {
        SimplePhotonNetworkManager.Singleton.region = Data.Code;
        SimplePhotonNetworkManager.Singleton.ConnectToRegion();
    }
}
