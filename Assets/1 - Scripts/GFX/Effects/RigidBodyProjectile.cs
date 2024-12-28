using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;



public class RigidBodyProjectile : ActionEffect, IGfxResult
{
    public float MinVelocity, MaxVelocity;
    public GameObject MuzzleFlash;
    public ActionResult ActionResult;

    public EffectSurfaceEvent OnHitSurface;

    private Rigidbody _rigidBody = null;
    private StatisticChange[] _statisticChanges;

    /// <summary>
    /// The location we're aiming to hit.
    /// </summary>
    SmartPoint _targetPoint;


    public bool AllCompleted()
    {
        return false;
    }


    public override void Run( AttackActionResult actionResult, Transform firePoint )
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
    private void TakeChange( AttackActionResult actionResult )
    {
        if( actionResult.AttackerWeapon.IsAOE() )
        {
            this._statisticChanges = actionResult.TakeChanges();
        }
        else
        {
            var change = actionResult.TakeChange();
            this._statisticChanges = new StatisticChange[] { change.Value };
        }
    }


    /// <summary>
    /// Align ourselves so we fire at the intended target location.
    /// </summary>
    /// <param name="actionResult">Action result we're visually presenting for.</param>
    private void AimAtTarget( ActionResult actionResult )
    {
        Vector3 toTarget = Vector3.zero;
        if( ActionResult.Target.GfxActor != null )
        {
            var targetComponent = ActionResult.Target.GfxActor.FindComponent( _statisticChanges[0].Statistic.Entity as MechComponentData );
            toTarget = targetComponent.transform.position - transform.position;
        }
        else
        {
            toTarget = ActionResult.Target.Position - transform.position; 
        }

        transform.forward = toTarget;

        //Expects to be faced the right direction already
        _rigidBody.AddForce( toTarget.normalized * Random.Range( MinVelocity, MaxVelocity ), ForceMode.VelocityChange );
    }


    private void OnDestroy()
    {
        //UIDamageNumbers.Instance.CreatePop( this._statisticChange.Change, transform.position 
        //CoroutineUtils.BeginCoroutine( CheckDeadTarget() );

        _statisticChanges.Do( x =>
        {
            var root = x.Statistic.Entity.GetRoot();
            var actor = root as Actor;
            var avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );

            var targetComponent = avatar.FindComponent( x.Statistic.Entity ); 
            UIDamageNumbers.Instance.CreatePop( x, targetComponent?.transform.position ?? transform.position );
        } );

        OnHitSurface.Invoke( EffectSurfaceType.Metal );
    }


}
