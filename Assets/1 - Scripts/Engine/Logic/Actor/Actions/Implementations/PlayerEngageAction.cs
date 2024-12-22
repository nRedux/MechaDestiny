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

[System.Serializable]
public class PlayerEngageAction : AttackAction
{
    public int Range = 3;
    public int Damage = 1;

    private ActorActionState _state;
    private Game _game;
    private Actor _actor;
    private UIFindAttackTargetRequest _uiRequest = null;
    private UIPickWeaponRequest _weaponPickRequest = null;

    public override int BoardRange => Range;
    public override ActionType ActionPhase => ActionType.Attack;


    public override ActorActionState State()
    {
        return _state;
    }


    public override void Tick()
    {
        TryRequestWeaponPick();

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
        Events.Instance.AddListener<ActiveWeaponChanged>( OnWeaponChanged );
    }


    private void TeardownListeners()
    {
        Events.Instance.RemoveListener<ActiveWeaponChanged>( OnWeaponChanged );
    }


    public override void End()
    {
        _state = ActorActionState.Finished;
        UIManager.Instance.PlayerAttackUI.Opt()?.SetActive( false );
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

        UIManager.Instance.PlayerAttackUI.Opt()?.SetActive( true );
        BeginBehavior( game, actor );
    }


    public void BeginBehavior( Game game, Actor actor )
    {
        //Get valid move locations. Notify the UI we need to display a collection of move locations. Wait for UI to return a result. Execute move.
        _state = ActorActionState.Started;

        var attackerMechEntity = actor.GetSubEntities()[0];
        var attackerMechData = attackerMechEntity as MechData;
        var weapon = attackerMechData.ActiveWeapon;

        //TODO: will have to validate that the assets which define these are correct. Make finding these problems easy!
        var range = weapon.GetStatistic( StatisticType.Range );

        var attackOptions = new BoolWindow( range.Value * 2 );
        attackOptions.MoveCenter( actor.Position );
        _game.Board.GetCellsManhattan( range.Value, attackOptions );
        Board.LOS_PruneBoolWindow( attackOptions, actor.Position );


        GfxActor attackerAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        UIManager.Instance.ShowSideAMechInfo( attackerAvatar.Actor, UIManager.MechInfoDisplayMode.Full );
        _uiRequest = CreateFindAttackTargetRequest( attackerAvatar, attackOptions );
        //Don't target actors on the same team
        _uiRequest.MarkInvalidTeams( actor.GetTeamID() );
        UIManager.Instance.RequestUI( _uiRequest );
    }


    private UIFindAttackTargetRequest CreateFindAttackTargetRequest( GfxActor attackerAvatar, BoolWindow attackOptions )
    {

        UIRequestSuccessCallback<Actor> success = targetActor =>
        {
            UIManager.Instance.PlayerAttackUI.Opt()?.SetActive( false );

            if( attackerAvatar == null )
            {
                Debug.LogError( "Avatar not found for actor." );
                return;
            }

            var targetAvatar = GameEngine.Instance.AvatarManager.GetAvatar( targetActor );

            UIManager.Instance.ShowSideBMechInfo( targetActor, UIManager.MechInfoDisplayMode.Full );
            this._actor.Target = targetActor;

            TryPickSequence( attackerAvatar.Actor);
        };

        UIRequestFailureCallback<bool> failure = moveTarget => { End(); _uiRequest = null; };
        UIRequestCancelResult cancel = () => { End(); _uiRequest = null; };
        return new UIFindAttackTargetRequest( attackerAvatar.Actor, attackOptions, success, failure, cancel );
    }

    UIActionSequenceRequest _sequencePickRequest;

    private void TryPickSequence( Actor attacker )
    {
        _sequencePickRequest = new UIActionSequenceRequest( _actor,
        x =>
        {
            _sequencePickRequest = null;
            this._actor.Sequence = x;
            End();
        },
        y =>
        {
            End();
            _sequencePickRequest = null;
        },
        () =>
        {
            End();
            _sequencePickRequest = null;
        } );

        UIManager.Instance.RequestUI( _sequencePickRequest, true );
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

