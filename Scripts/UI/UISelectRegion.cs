using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectRegion : MonoBehaviour
{
    public SimplePhotonNetworkManager networkManager;
    public Dropdown dropdown;

    public void OnSelectRegion(int index)
    {
        networkManager.region = dropdown.options[index].text;
    }
}
