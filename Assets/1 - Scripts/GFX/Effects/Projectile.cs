using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : ActionEffect, IGfxResult
{

    /// <summary>
    /// The result which created us.
    /// </summary>
    public ActionResult SourceResult;

    private bool _complete = false;
    private StatisticChange _statisticChange;


    public void StruckTarget()
    {
        CoroutineUtils.BeginCoroutine( DoPostHit() );
    }

    public IEnumerator DoPostHit()
    {
        UIDamageNumbers.Instance.CreatePop( this._statisticChange, transform.position );

        yield return CheckDeadTarget();

        Destroy( gameObject );
    }
    

    private IEnumerator CheckDeadTarget()
    {

        yield break;

    }

    private IEnumerator CheckDestroyedComponent()
    {
        yield return GameEngine.Instance.AvatarManager.DoDestroySequenceAllDeadActors();
    }


    public void InterpolateToTarget( Transform target )
    {
        InterpolateToTarget( target.position );
    }

    public void InterpolateToTarget( Vector3 target )
    {
        Vector3 start = transform.position;
        CoroutineUtils.DoInterpolation( .3f, f =>
        {
            transform.position = Vector3.Lerp( start, target, f );
            return true;
        },
        StruckTarget );
    }

    public bool AllCompleted()
    {
        return _complete;
    }

    public override void Run( ActionResult actionResult, Transform firePoint )
    {
        var result = actionResult.TakeChange();
        if( !result.HasValue )
        {
            throw new System.Exception("Hey remember me? Bad thing happened, handle this better.");
        }

        this._statisticChange = result.Value;

        this.SourceResult = actionResult;

        GfxComponent targetComponent = null;// actionResult.Target.FindComponent( _statisticChange.MechComponent );
        Debug.Log( targetComponent.gameObject.name );
        InterpolateToTarget( targetComponent.transform );
    }
}
