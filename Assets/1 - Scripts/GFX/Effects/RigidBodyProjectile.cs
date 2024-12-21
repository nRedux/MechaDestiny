using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class RigidBodyProjectile : ActionEffect, IGfxResult
{
    public float MinVelocity, MaxVelocity;
    public GameObject MuzzleFlash;
    public ActionResult ActionResult;

    public EffectSurfaceEvent OnHitSurface;

    private Rigidbody _rigidBody = null;
    private StatisticChange _statisticChange;

    /// <summary>
    /// The location we're aiming to hit.
    /// </summary>
    SmartPoint _targetPoint;


    public bool AllCompleted()
    {
        return false;
    }


    public override void Run( ActionResult actionResult, Transform firePoint )
    {
        SetupRigidBody();

        this.ActionResult = actionResult;
        
        TakeChange( actionResult );

        AimAtTarget( actionResult );

        SpawnMuzzleFlash( firePoint );
    }


    private void SpawnMuzzleFlash( Transform firePoint )
    {
        if( MuzzleFlash )
        {
            Instantiate( MuzzleFlash, firePoint.position, firePoint.rotation );
        }
    }


    /// <summary>
    /// Get our ref to the RigidBody initialized.
    /// </summary>
    private void SetupRigidBody()
    {
        _rigidBody = GetComponent<Rigidbody>();
        if( _rigidBody == null )
        {
            Debug.LogError( "No RigidBody on RigidBodyProjectile" );
            return;
        }
    }


    /// <summary>
    /// Take change from action result, get everything we need for engine state.
    /// </summary>
    /// <param name="actionResult">The action result we're visualizing for</param>
    private void TakeChange( ActionResult actionResult )
    {
        var result = actionResult.TakeChange();
        if( !result.HasValue )
        {
            throw new System.Exception( "Hey remember me? Bad thing happened, handle this better." );
        }

        this._statisticChange = result.Value;
    }


    /// <summary>
    /// Align ourselves so we fire at the intended target location.
    /// </summary>
    /// <param name="actionResult">Action result we're visually presenting for.</param>
    private void AimAtTarget( ActionResult actionResult )
    {
        var targetComponent = actionResult.TargetAvatar.FindComponent( _statisticChange.MechComponent );
        _targetPoint = new SmartPoint( targetComponent.transform );

        Vector3 toTarget = targetComponent.transform.position - transform.position;
        transform.forward = toTarget;

        //Expects to be faced the right direction already
        _rigidBody.AddForce( toTarget.normalized * Random.Range( MinVelocity, MaxVelocity ), ForceMode.VelocityChange );
    }


    private void OnDestroy()
    {
        //UIDamageNumbers.Instance.CreatePop( this._statisticChange.Change, transform.position 
        CoroutineUtils.BeginCoroutine( CheckDeadTarget() );

        UIDamageNumbers.Instance.CreatePop( this._statisticChange, transform.position );

        OnHitSurface.Invoke( EffectSurfaceType.Metal );
    }


    private IEnumerator CheckDeadTarget()
    {
        if( ActionResult.MechData == null )
            yield break;

        if( ActionResult.MechData.Torso == null )
            yield break;//TODO: Bad mech data setup. But need to validate that elsewhere. VALIDATE IT!!!

        if( ActionResult.MechData.IsTorsoDestroyed() )
        {
            //TODO: Should also be doing a fallback in the action itself if there are no graphic things like projectile to do this.
            //Dead mech! Run death procedure.
            yield return ActionResult.TargetAvatar.DoDeathSequence();
        }
    }

}
