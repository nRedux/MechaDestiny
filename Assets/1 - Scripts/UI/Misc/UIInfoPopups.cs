using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//TODO: Consider making this do pooling, otherwise I could just instantiate these from wherever and not need this class.

[System.Serializable]
public class InfoPopupType
{
    public string Type;
    public UIInfoPop Popup;
}


public class UIInfoPopups : Singleton<UIInfoPopups>
{
    public const string BOOST_POPUP = "boost";

    public UIInfoPop DamagePrefab;

    public InfoPopupType[] Popups;


    protected override void Awake()
    {
        base.Awake();
    }


    public void CreatePop( string type, string message, Vector3 worldPosTarget )
    {
        var pop = Popups.Where( x => x.Type == type ).FirstOrDefault();
        if( pop == null || pop.Popup == null )
            return;

        var popInstance = Instantiate<UIInfoPop>( pop.Popup );
        popInstance.Initialize( this.transform, message, worldPosTarget );
    }


    public void CreatePop( StatisticChange statChange, Vector3 worldPosTarget )
    {
        if( DamagePrefab == null )
            return;
        var pop = Instantiate<UIInfoPop>( DamagePrefab );
        pop.Initialize( this.transform, statChange.GetChangeStringified(), worldPosTarget + Random.insideUnitSphere * .1f );
    }


    public void CreatePop( string message, Vector3 worldPosTarget )
    {
        if( DamagePrefab == null )
            return;
        var pop = Instantiate<UIInfoPop>( DamagePrefab );
        pop.Initialize( this.transform, message, worldPosTarget );
    }

}
