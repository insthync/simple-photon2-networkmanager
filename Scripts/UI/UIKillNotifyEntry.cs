using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIKillNotifyEntry : MonoBehaviour
{
    [Tooltip("{0} = KillerName, {1} = VictimName, {2} = WeaponName")]
    public string fullKillTextFormat = "{1} was killed by {0}'s {2}";
    public Text fullKillText;
    public Text killerNameText;
    public Text victimNameText;
    public Text weaponNameText;
    public RawImage weaponIconRawImage;

    public void SetData(string killerName, string victimName, string weaponName, Texture weaponIcon)
    {
        if (fullKillText != null)
            fullKillText.text = string.Format(fullKillTextFormat, killerName, victimName, weaponName);

        if (killerNameText != null)
            killerNameText.text = killerName;

        if (victimNameText != null)
            victimNameText.text = victimName;

        if (weaponNameText != null)
            weaponNameText.text = weaponName;

        if (weaponIconRawImage != null)
            weaponIconRawImage.texture = weaponIcon;
    }

    public void Clear()
    {
        if (fullKillText != null)
            fullKillText.text = "";
        if (killerNameText != null)
            killerNameText.text = "";
        if (victimNameText != null)
            victimNameText.text = "";
        if (weaponNameText != null)
            weaponNameText.text = "";
        if (weaponIconRawImage != null)
            weaponIconRawImage.texture = null;
    }
}
