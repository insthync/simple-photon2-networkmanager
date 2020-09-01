using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRegionList : UIBase
{
    public UIRegionEntry entryPrefab;
    public GameObject noEntryObject;
    public Transform regionListContainer;

    private void OnEnable()
    {
        FillRegionList();
        SimplePhotonNetworkManager.onRegionListReceived += OnRegionListReceived;
    }

    private void OnDisable()
    {
        SimplePhotonNetworkManager.onRegionListReceived -= OnRegionListReceived;
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        FillRegionList();
    }

    public void FillRegionList()
    {
        if (entryPrefab == null || regionListContainer == null)
            return;
        for (var i = regionListContainer.childCount - 1; i >= 0; --i)
        {
            var child = regionListContainer.GetChild(i);
            Destroy(child.gameObject);
        }
        foreach (var data in SimplePhotonNetworkManager.EnabledRegions.Values)
        {
            var newEntry = Instantiate(entryPrefab, regionListContainer);
            newEntry.SetData(data);
            newEntry.gameObject.SetActive(true);
        }
        if (noEntryObject != null)
            noEntryObject.SetActive(regionListContainer.childCount <= 0);
    }
}
