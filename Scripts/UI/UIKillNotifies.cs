using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIKillNotifies : MonoBehaviour
{
    public UIKillNotifyEntry entryPrefab;
    public Transform container;
    public float entryVisibleDuration = 2f;
    public void Notify(string killerName, string victimName, string weaponName, Texture weaponIcon)
    {
        var newEntry = Instantiate(entryPrefab);
        newEntry.SetData(killerName, victimName, weaponName, weaponIcon);
        newEntry.transform.SetParent(container);
        newEntry.transform.localScale = Vector3.one;
        newEntry.transform.localRotation = Quaternion.identity;
        Destroy(newEntry.gameObject, entryVisibleDuration);
    }
}
