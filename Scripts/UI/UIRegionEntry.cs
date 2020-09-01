using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRegionEntry : MonoBehaviour
{
    [System.Serializable]
    public struct RegionName
    {
        public string code;
        public string name;
    }
    public GameObject currentSign;
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
    public RegionName[] regionNames = new RegionName[]
    {
        new RegionName() { code = "asia", name = "Asia" },
        new RegionName() { code = "au", name = "Australia" },
        new RegionName() { code = "cae", name = "Canada, East" },
        new RegionName() { code = "cn", name = "Chinese Mainland" },
        new RegionName() { code = "eu", name = "Europe" },
        new RegionName() { code = "in", name = "India" },
        new RegionName() { code = "jp", name = "Japan" },
        new RegionName() { code = "ru", name = "Russia" },
        new RegionName() { code = "rue", name = "Russia, East" },
        new RegionName() { code = "za", name = "South Africa" },
        new RegionName() { code = "sa", name = "South America" },
        new RegionName() { code = "kr", name = "South Korea" },
        new RegionName() { code = "us", name = "USA, East" },
        new RegionName() { code = "usw", name = "USA, West" },
    };
    public Text textRegionName;
    public bool showCurrentRegion;
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
        var currentRegion = string.Empty;
        if (!string.IsNullOrEmpty(PhotonNetwork.CloudRegion))
            currentRegion = PhotonNetwork.CloudRegion.TrimEnd('/', '*');

        if (showCurrentRegion && SimplePhotonNetworkManager.EnabledRegions.ContainsKey(currentRegion))
        {
            Data = null;
            SetData(SimplePhotonNetworkManager.EnabledRegions[currentRegion]);
        }

        if (Data == null)
            return;

        if (currentSign)
            currentSign.SetActive(currentRegion.Equals(Data.Code));

        if (lowPingSign)
            lowPingSign.SetActive(Data.Ping < highPing);

        if (highPingSign)
            highPingSign.SetActive(Data.Ping >= highPing && Data.Ping < veryHighPing);

        if (veryHighPingSign)
            veryHighPingSign.SetActive(Data.Ping >= veryHighPing);

        if (textPing)
        {
            textPing.text = Data.WasPinged ? Data.Ping.ToString("N0") + "ms" : "N/A";
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
        SimplePhotonNetworkManager.Singleton.Disconnect();
        SimplePhotonNetworkManager.Singleton.ConnectToRegion();
    }
}
