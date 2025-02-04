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

    public override ActorActionState State()
    {
        return _state;
    }


    public override void Tick()
    {

        return;
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

        //Get valid move locations. Notify the UI we need to display a collection of move locations. Wait for UI to return a result. Execute move.
        _state = ActorActionState.Executing;

        GfxActor attackerAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        UIManager.Instance.ShowSideAMechInfo( attackerAvatar.Actor, UIManager.MechInfoDisplayMode.Full );
        _uiRequest = CreateFindAttackTargetRequest( attackerAvatar );
        //Don't target actors on the same team
        _uiRequest.MarkInvalidTeams( actor.GetTeamID() );
        UIManager.Instance.RequestUI( _uiRequest );
    }


    private UIFindAttackTargetRequest CreateFindAttackTargetRequest( GfxActor attackerAvatar )
    {

        UIRequestSuccessCallback<object> success = selectedTarget =>
        {
            UIManager.Instance.PlayerAttackUI.Opt()?.SetActive( false );

            if( attackerAvatar == null )
            {
                Debug.LogError( "Avatar not found for actor." );
                return;
            }

            if( attackerAvatar.Actor.ActiveWeapon.IsAOE() )
            {
                var location = (Vector2Int) selectedTarget;

                this._actor.Target = new SmartPoint( new Vector3( location.x, 0, location.y ) );

                TryPickSequence( attackerAvatar.Actor );
            }
            else
            { 
                var actor = (Actor)selectedTarget;
                var targetAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );

                UIManager.Instance.ShowSideBMechInfo( actor, UIManager.MechInfoDisplayMode.Full );
                this._actor.Target = new SmartPoint( targetAvatar );

                TryPickSequence( attackerAvatar.Actor );
            }


        };

        UIRequestFailureCallback<bool> failure = moveTarget => { End(); _uiRequest = null; };
        UIRequestCancelResult cancel = () => { End(); _uiRequest = null; };
        return new UIFindAttackTargetRequest( attackerAvatar.Actor, success, failure, cancel );
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

