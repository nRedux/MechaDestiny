using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWeaponInfo : MonoBehaviour
{
    public Image WeaponTypeImage;

    [HideInInspector]
    public string ShotCount;
    [HideInInspector]
    public string Damage;

    LocalizedStringsRefresher _locRefresher;

    private void Awake()
    {
        _locRefresher = new LocalizedStringsRefresher( gameObject );
    }

    public void Refresh( MechComponentData weapon )
    {
        if( weapon == null ) return;

        var wepInfo = weapon.WeaponTypeInfo;

        if( WeaponTypeImage && wepInfo != null )
            WeaponTypeImage.sprite = wepInfo.WeaponTypeSprite;

        ShotCount = weapon.GetStatistic( StatisticType.ShotCount )?.Value.ToString() ?? "0";
        Damage = weapon.GetStatistic( StatisticType.Damage )?.Value.ToString() ?? "0";
        _locRefresher?.RefreshLocalizedStrings();
    }

}
