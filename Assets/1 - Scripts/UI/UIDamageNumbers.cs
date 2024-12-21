using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDamageNumbers : Singleton<UIDamageNumbers>
{

    public UIDamagePop DamagePrefab;
    private RectTransform _rectTransform;


    protected override void Awake()
    {
        base.Awake();
        _rectTransform = transform as RectTransform;
    }


    public void CreatePop( StatisticChange statChange, Vector3 worldPos )
    {
        if( DamagePrefab == null )
            return;

        var pop = Instantiate<UIDamagePop>( DamagePrefab );
        pop.transform.SetParent( this.transform, false );
        pop.Initialize( statChange.GetChangeStringified() );
        var rt = pop.transform as RectTransform;
        rt.SetCanvasScreenPosition( _rectTransform, worldPos + Random.insideUnitSphere * .1f );
        pop.DoRise();
    }

}
