using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWeaponInfo : MonoBehaviour
{
    [HideInInspector]
    public string ShotCount;
    [HideInInspector]
    public string Damage;

    LocalizedStringsRefresher _locRefresher;

    private void Awake()
    {
        _locRefresher = new LocalizedStringsRefresher( gameObject );
    }

    public void Refresh( MechData mechData )
    {
        if( mechData == null ) return;

        var activeWeapon = mechData.ActiveWeapon;
        ShotCount = activeWeapon.GetStatistic( StatisticType.ShotCount )?.Value.ToString() ?? "0";
        Damage = activeWeapon.GetStatistic( StatisticType.Damage )?.Value.ToString() ?? "0";
        _locRefresher?.RefreshLocalizedStrings();
    }

}
