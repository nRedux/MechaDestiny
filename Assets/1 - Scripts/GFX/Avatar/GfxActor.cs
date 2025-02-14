using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;
using Sirenix.OdinInspector;

public class GfxActorAction
{
    public ActionResult SourceAction;
    public int Count;
    public System.Action Complete;
}


[System.Serializable]
public class AnimatorFloat
{

    public string Name;
    public float Value;
    public Animator Animator;


    private int _hash;
    private AnimationCurve _curve = null;
    private Coroutine _coroutine;

    public AnimatorFloat(Animator animator)
    {
        Initialize( animator );
    }

    public AnimatorFloat( Animator animator, AnimationCurve curve )
    {
        Initialize( animator, curve );
    }

    public void Initialize( Animator animator )
    {
        _hash = Animator.StringToHash( Name );
        Animator = animator;
    }

    public void Initialize( Animator animator, AnimationCurve curve )
    {
        _hash = Animator.StringToHash( Name );
        _curve = curve;
        Animator = animator;
    }

    public void Apply( )
    {
        Animator.SetFloat( _hash, Value );
    }

    public void InterpolateValue( float target, float duration )
    {
        CoroutineUtils.EndCoroutine( ref _coroutine );
        float startVal = Value;
        _coroutine = CoroutineUtils.DoInterpolation( duration, f =>
        {
            if( this._curve == null )
                this.Value = Mathf.Lerp( startVal, target, f );
            else
                this.Value = Mathf.Lerp( startVal, target, _curve.Evaluate( f ) );
            Apply();
            return true;
        },
        () => Apply() );
        
    }

}

public class GfxActor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    const string ANIM_FIRED = "fired";
    const string TRIGGER_DEATH = "Trg_Die";

    public System.Action<GfxActor> HoverStarted;
    public System.Action<GfxActor> HoverEnded;

    [TitleGroup( "Gfx Body Components" )]
    public GfxComponent Torso;
    public GfxComponent Legs;
    public GfxComponent LeftArm;
    public GfxComponent RightArm;

    public AwaitableBehavior DeathBehavior;

    [Space]
    public AnimatorFloat AnimatorMoveSpeed;

    [Space]
    public AttackCamera AttackCamera;

    public Actor Actor { get; private set; }

    private Animator _animator;

    [HideInInspector]
    public string LastAnimEvent;

    private StatisticWatcher _mechHealthWatcher = null;

    private bool _destroyed = false;

    private bool _deathSequenceDone = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        AnimatorMoveSpeed.Initialize( _animator );
    }



    public void Initialize( Actor actor )
    {
        Actor = actor;

        if( Torso == null )
        {
            Debug.LogError($"{nameof(Torso)} Not assigned", gameObject );
        }
        else
        {
            _mechHealthWatcher = new StatisticWatcher( Torso.ComponentData.GetStatistic( StatisticType.Health ), OnMechHealthChange );
        }
    }

    private void OnMechHealthChange( StatisticChange change )
    {
        if( !_destroyed && change.Value <= 0 )
        {
            _destroyed = true;
            OnActorKilled();
        }
    }

    private void OnActorKilled()
    {
        //Actor died. Do death animation.
        StartCoroutine( DoDeathSequence() );
    } 

    public void PlayDeathAnimation()
    {
        Debug.Log("Do death animation");
        _animator.SetTrigger( TRIGGER_DEATH );
        Destroy( gameObject, 2f );
    }

    public void SyncPositionToActor( Actor actor )
    {
        transform.position = actor.GetWorldPosition();
    }

    public GfxComponent FindComponent( IEntity data )
    {
        if( Torso.ComponentData == data )
            return Torso;
        if( Legs.ComponentData == data )
            return Legs;
        if( LeftArm.ComponentData == data )
            return LeftArm;
        if( RightArm.ComponentData == data )
            return RightArm;
        return null;
    }


    public void SetTeamColor( Color color )
    {
        var renderer = this.GetComponentsInChildren<Renderer>();
        renderer.Do( x => x.materials.Do( y => y.color = color ) );
    }


    public void OnPointerEnter( PointerEventData eventData )
    {
        HoverStarted?.Invoke( this );
    }


    public void OnPointerExit( PointerEventData eventData )
    {
        HoverEnded?.Invoke( this );
    }


    internal IEnumerator DoDeathSequence()
    {
        if( _deathSequenceDone )
            yield break;
        _deathSequenceDone = true;
        //TODO: This should be checked after the attack completes. Right now it's going to make the sequence look wrong. 
        //AttackHelper.HandleDeadActor( TargetAvatar.Actor );

        if( DeathBehavior != null )
        {
            DeathBehavior.Begin();
            yield return StartCoroutine( DeathBehavior.AwaitComplete() );
        }
        else
        {
            //Fallback
            PlayDeathAnimation();
        }

        yield return new WaitForSeconds( .5f );
    }


    public Vector3 GetLocalVelocity()
    {
        if( _animator == null )
            return Vector3.zero;
        return this.transform.InverseTransformVector(_animator.velocity);
    }

    public void ActorEvent( string evt )
    {
        LastAnimEvent = evt;
    }

    private bool _attackTurnDone = false;
    public void AttackTurnDone( )
    {
        _attackTurnDone = true;
    }

    public void StartAttackTurn( Vector3 target )
    {
        Vector3 delta = target - transform.position;
        Vector3 forward = transform.forward;
        delta.y = 0f;
        forward.y = 0f;
        float angle = Vector3.SignedAngle( forward, delta, Vector3.up );

        _animator.SetFloat( "Attack_Angle", angle );
        _animator.SetTrigger( "Attack_Turn" );
    }


    public void FinishAttackTurn( )
    {
        _animator.SetTrigger( "Attack_Turn_End" );
    }


    public async Task ExecuteAction( AttackActionResult action, System.Action complete )
    {
        //StartCoroutine( ActionActionProcess( action, complete ) );
        await AttackActionSequenceAsync( action, complete );
    }


    private bool AnimationEventMatches( string evt )
    {
        if( LastAnimEvent == null )
            return false;
        var result = evt == LastAnimEvent;
        LastAnimEvent = null;
        return result;
    }

    public async Task AttackActionSequenceAsync( AttackActionResult result, System.Action complete )
    {

        _attackTurnDone = true;
        LastAnimEvent = String.Empty; 

        if( result.DisplayProps.IsSequenceStart )
        {
            _attackTurnDone = false;
            StartAttackTurn( result.Target.Position );
        }

        //TODO: Max time we will wait is one loop through the animation? Can we just wait that long at most?
        while( !_attackTurnDone )
            await Task.Yield();
        
        _attackTurnDone = false;

        string attackParam = result.GetAnimationParam();
        string fireParam = attackParam + "_Fire";


        await TurnTorsoToTarget( result, .4f );
        

        _animator.SetBool( attackParam, true );

        if( result.AttackerWeapon.HasFeatureFlag( (int) WeaponFlags.ShotgunFireMode ) )
        {
            _animator.SetTrigger( fireParam );
            while( !AnimationEventMatches( ANIM_FIRED ) )
                await Task.Yield();

            if( result.DisplayProps.DoArmStart )
                _animator.SetBool( attackParam, false );
            
            for( int i = 0; i < result.Count; i++ )
            {
                CreateShotEffect( result );
            }
        }
        else
        {
            for( int i = 0; i < result.Count; i++ )
            {
                _animator.SetTrigger( fireParam );
                while( !AnimationEventMatches( ANIM_FIRED ) )
                    await Task.Yield();
                CreateShotEffect( result );
            }
        }

        _animator.SetBool( fireParam, false );

        if( result.DisplayProps.DoArmEnd )
        {
            _animator.SetBool( attackParam, false );
        }

        if( result.DisplayProps.IsSequenceEnd )
        {
            FinishAttackTurn();
            await TurnTorsoToNeutral( result, .4f );
            await Task.Delay( 1000 );
        }


        complete?.Invoke();
        Debug.Log("Action ending!");
    }


    private Quaternion _attackerTorsoNeutral;

    public async Task TurnTorsoToTarget( AttackActionResult result, float duration )
    {
        _attackerTorsoNeutral = result.Attacker.Torso.transform.rotation;

        Quaternion start = result.Attacker.Torso.transform.rotation;
        Quaternion end = CalculateEndRotation( result );
        //end = end * Quaternion.AngleAxis( 90, result.Attacker.Torso.transform.right );
        //end = end * Quaternion.AngleAxis( 180, Vector3.up );

        float f = 0;
        while( !Mathf.Approximately(f, duration ) )
        {
            f = Mathf.Clamp( f + Time.deltaTime, 0f, duration );
            float t = f / duration;
            //end = CalculateEndRotation( result );
            result.Attacker.Torso.transform.rotation = Quaternion.Slerp( start, end, t );
            await Task.Yield();
        }
    }

    private Quaternion CalculateEndRotation( AttackActionResult result )
    {
        Quaternion end = Quaternion.LookRotation( result.Target.Position - result.Attacker.transform.position );
        end = Quaternion.AngleAxis( -90, result.Target.Position - result.Attacker.transform.position ) * end;
        end = Quaternion.AngleAxis( 90, Vector3.up ) * end;

        return end;
    }

    public async Task TurnTorsoToNeutral( AttackActionResult result, float duration )
    {
        Quaternion start = result.Attacker.Torso.transform.localRotation;

        float f = 0;
        while( !Mathf.Approximately( f, duration ) )
        {
            f = Mathf.Clamp( f + Time.deltaTime, 0f, duration );
            float t = f / duration;
            result.Attacker.Torso.transform.localRotation = Quaternion.Slerp( start, result.Attacker.Torso.IdentityRotation, t );
            //Debug.Log( f );
            await Task.Yield();
        }
    }

    public void CreateShotEffect( AttackActionResult result )
    {
        result.AttackerWeapon.ModelInstance.CreateShotEffect( result );
    }

    #region CAMERA

    private System.Action _cameraBlendDone = null;
    public void StartAttackCamera( System.Action cameraDone, GfxActor attacker, SmartPoint target )
    {
        if( !AttackCamera )
        {
            cameraDone?.Invoke();
            return;
        }
        _cameraBlendDone = cameraDone;
        AttackCamera.SetTargets( attacker.transform, target);
        AttackCamera.SetActive( true );
    }

    public void StopAttackCamera()
    {
        if( !AttackCamera )
        {
            return;
        }
        AttackCamera.SetActive( false );
        AttackCamera.ClearTempTargets();
    }

    public void AttackCameraRunning( ICinemachineMixer mixer, ICinemachineCamera camera )
    {
        _cameraBlendDone?.Invoke();
        _cameraBlendDone = null;
    }

    #endregion CAMERA


    /// <summary>
    /// Convenience method for finding the position of the torso component in world space.
    /// </summary>
    /// <returns>The torso transform position if Torso is non null, this transform position otherwise</returns>
    public Vector3 GetTorsoPosition()
    {
        return Torso.Opt()?.transform.position ?? transform.position;
    }
}
