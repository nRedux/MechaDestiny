using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechDeath : AwaitableBehavior
{
    private GfxActor _actor;
    public int NumExplosionsMin = 1;
    public int NumExplosionsMax = 3;

    public float InterExplosionDelay = .1f;
    public float AfterExplosionsDelay = .5f;
    public float AfterDeathAnimDelay = .5f;
    public GameObject ExplosionPrefab;


    private void Awake()
    {
        _actor = GetComponentInParent<GfxActor>();
    }


    public override IEnumerator Action()
    {
        int num = Random.Range( NumExplosionsMin, NumExplosionsMax );
        for( int i = 0; i < num; ++i )
        {
            yield return StartCoroutine( InstantiateEffect() );
            yield return new WaitForSeconds( InterExplosionDelay );
        }

        yield return new WaitForSeconds( AfterExplosionsDelay );

        Debug.LogError("Death Anim!");
        _actor.DoDeathAnimation();
        yield return new WaitForSeconds( AfterDeathAnimDelay );
    }


    private IEnumerator InstantiateEffect( )
    {
        if( ExplosionPrefab == null )
        {
            Debug.LogError("No explosion prefab");
            yield break;
        }

        var surfPoint = _actor.Torso.SurfacePoints.Random();
        if( surfPoint == null )
        {
            Debug.LogError("No surface points on torso");
            yield break;
        }

        var instance = Instantiate<GameObject>( ExplosionPrefab );
        instance.transform.position = surfPoint.transform.position;
        instance.transform.rotation = surfPoint.transform.rotation;
    }

}
