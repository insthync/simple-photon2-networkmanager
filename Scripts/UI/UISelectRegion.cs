using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectRegion : MonoBehaviour
{
    public string[] regions;
    public Text textSelectedRegion;
    private int selectedRegion;

    private void Update()
    {
        if (textSelectedRegion != null)
            textSelectedRegion.text = regions[selectedRegion];
    }

    public void OnSelectRegion(int index)
    {
        selectedRegion = index;
    }

    public void OnClickConnectToRegion()
    {
        SimplePhotonNetworkManager.Singleton.region = regions[selectedRegion];
        SimplePhotonNetworkManager.Singleton.ConnectToRegion();
    }
}
