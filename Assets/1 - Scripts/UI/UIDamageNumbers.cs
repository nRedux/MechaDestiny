using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDamageNumbers : Singleton<UIDamageNumbers>
{

    public UIDamagePop DamagePrefab;


    protected override void Awake()
    {
        base.Awake();
    }


    public void CreatePop( StatisticChange statChange, Vector3 worldPosTarget )
    {
        if( DamagePrefab == null )
            return;
        var pop = Instantiate<UIDamagePop>( DamagePrefab );
        pop.Initialize( this.transform, statChange.GetChangeStringified(), worldPosTarget + Random.insideUnitSphere * .1f );
    }

    public void CreatePop( string message, Vector3 worldPosTarget )
    {
        if( DamagePrefab == null )
            return;
        var pop = Instantiate<UIDamagePop>( DamagePrefab );
        pop.Initialize( this.transform, message, worldPosTarget );
    }

}
