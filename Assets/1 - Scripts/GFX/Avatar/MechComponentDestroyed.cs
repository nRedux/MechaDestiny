using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechComponentDestroyed : AwaitableBehavior
{
    private GfxComponent _component;
    public int NumExplosionsMin = 1;
    public int NumExplosionsMax = 3;

    public float InterExplosionDelay = .1f;
    public float AfterExplosionsDelay = .5f;
    public float AfterDeathAnimDelay = .5f;
    public GameObject ExplosionPrefab;


    private void Awake()
    {
        _component = GetComponentInParent<GfxComponent>();
    }


    public override IEnumerator Action()
    {
        if( _component == null )
            yield break;

        int num = Random.Range( NumExplosionsMin, NumExplosionsMax );
        for( int i = 0; i < num; ++i )
        {
            yield return StartCoroutine( InstantiateEffect() );
            yield return new WaitForSeconds( InterExplosionDelay );
        }

        yield return new WaitForSeconds( AfterExplosionsDelay );

        Debug.LogError("Death Anim!", gameObject);
        yield return new WaitForSeconds( AfterDeathAnimDelay );
    }


    private IEnumerator InstantiateEffect( )
    {
        if( ExplosionPrefab == null )
        {
            Debug.LogError("No explosion prefab");
            yield break;
        }

        var surfPoint = _component.SurfacePoints.Random();
        if( surfPoint == null )
        {
            surfPoint = transform;
        }
        var instance = Instantiate<GameObject>( ExplosionPrefab );
        instance.transform.position = surfPoint.position;
        instance.transform.rotation = surfPoint.rotation;
    }

}
