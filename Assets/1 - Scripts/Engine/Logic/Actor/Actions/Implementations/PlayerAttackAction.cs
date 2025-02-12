using System.Collections;   
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System;
using JetBrains.Annotations;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Sprites;
using System.Linq;

[System.Serializable]
public class PlayerAttackAction : AttackAction
{
    public int Range = 3;
    public int Damage = 1;


    private ActorActionState _state;
    private Game _game;
    private Actor _actor;
    private UIFindAttackTargetRequest _uiRequest = null;
    private UIPickWeaponRequest _weaponPickRequest = null;

    public override int BoardRange => Range;


    public override ActorActionState State()
    {
        return _state;
    }


    public override void Tick()
    {
        //TryRequestWeaponPick();

        return;
    }


    private void TryRequestWeaponPick()
    {
        if( !UIPickWeaponRequest.CanExecute( _actor ) )
            return;
        if( !Input.GetKeyDown( KeyCode.Q ) )
            return;

        if( _weaponPickRequest != null )
            return;

        _weaponPickRequest = new UIPickWeaponRequest( _actor,
        x =>
        {
            AllowActionSelect = true;
            _weaponPickRequest = null;
        },
        y =>
        {
            AllowActionSelect = true;
            _weaponPickRequest = null;
        },
        () =>
        {
            AllowActionSelect = true;
            _weaponPickRequest = null;
        } );
        AllowActionSelect = false;
        UIManager.Instance.RequestUI( _weaponPickRequest, false );
    }


    private void SetupListeners()
    {
        Events.Instance.AddListener<ActiveWeaponChanged>(OnWeaponChanged);
    }


    private void TeardownListeners()
    {
        Events.Instance.RemoveListener<ActiveWeaponChanged>( OnWeaponChanged );
    }


    public override void End()
    {
        _state = ActorActionState.Finished;
        //UIManager.Instance.PlayerAttackUI.Opt()?.SetActive( false );
        CancelUIRequests();
        TeardownListeners();
    }


    private void OnWeaponChanged( ActiveWeaponChanged e )
    {
        CancelUIRequests();
        Start( _game, _actor );
    }


    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );
        SetupListeners();
        _game = game;
        _actor = actor;

        //UIManager.Instance.PlayerAttackUI.Opt()?.SetActive( true );
        BeginBehavior(game, actor);
    }

    public void BeginBehavior( Game game, Actor actor )
    {
        //Get valid move locations. Notify the UI we need to display a collection of move locations. Wait for UI to return a result. Execute move.
        _state = ActorActionState.Started;
        GfxActor attackerAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        DoAttack( attackerAvatar );
        if( DisplayProps.IsSequenceStart )
        {
            UIManager.Instance.ShowSideAMechInfo( actor, UIManager.MechInfoDisplayMode.Mini );
            if( actor.Target?.GfxActor != null )
                UIManager.Instance.ShowSideBMechInfo( actor.Target.GfxActor.Actor, UIManager.MechInfoDisplayMode.Mini );
        }
        
        /*_uiRequest = CreateFindAttackTargetRequest( attackerAvatar, attackOptions );
        //Don't target actors on the same team
        _uiRequest.MarkInvalidTeams( actor.GetTeamID() );*/
        //UIManager.Instance.RequestUI( _uiRequest );
    }

    private void DoAttack( GfxActor attackerAvatar )
    {
        var selectedTarget = this.Target;
        SpendAP( attackerAvatar.Actor );
        _state = ActorActionState.Executing;
        
        AttackActionResult res = new AttackActionResult( attackerAvatar, this.Target, this.UsedWeapon );
        res.SequencePosition = this.SequencePos;
        res.DisplayProps = this.DisplayProps;
        res.OnComplete = () =>
        {
            if( res.DisplayProps.IsSequenceStart )
            {
                UIManager.Instance.ShowSideAMechInfo( attackerAvatar.Actor, UIManager.MechInfoDisplayMode.Full );
                UIManager.Instance.HideSideBMechInfo();
            }
            _uiRequest = null;
            End();
        };

        AttackHelper.CalculateAttackDamage( res );

        TestKilledTargets( res );

        
        UIManager.Instance.QueueResult( res );
    }

    public class StatisticChangeRootComp : IEqualityComparer<StatisticChange>
    {
        public bool Equals( StatisticChange x, StatisticChange y )
        {
            return x.Statistic.Entity.GetRoot() == y.Statistic.Entity.GetRoot();
        }

        public int GetHashCode( StatisticChange obj )
        {
            return obj.GetHashCode();
        }
    }

    public async void RunAttack( AttackActionResult res )
    {

    }


    public override void TurnEnded()
    {
        End();
    }

    private void CancelUIRequests()
    {
        _weaponPickRequest?.Cancel();
        _uiRequest?.Cancel();
        _uiRequest = null;
        _weaponPickRequest = null;

        AllowActionSelect = true;
    }

}

