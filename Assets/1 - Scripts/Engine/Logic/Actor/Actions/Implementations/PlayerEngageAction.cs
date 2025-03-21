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
using Unity.VisualScripting;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using System.Linq;
using static UnityEditor.FilePathAttribute;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using System.Runtime.CompilerServices;

[System.Serializable]
public class PlayerEngageAction : AttackAction
{
    public int Damage = 1;

    private Game _game;
    private Actor _actor;
    private UIFindAttackTargetRequest _uiRequest = null;
    private bool _uiWantsToFire = false;
    private ActionSequence _actionSequence;


    public override void Tick()
    {
        if( Input.GetKeyDown( KeyCode.F ) || _uiWantsToFire )
        {
            //Assign sequence and get out of here.
            var uiSequence = UIManager.Instance.ActionSequence.GetSelectedSequence();
            if( uiSequence.Count > 0 )
                _actor.Sequence = uiSequence;
            End();
        }
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
        UsedWeapon = null;
        this.State = ActorActionState.Finished;
        UIManager.Instance.TryEndActionPick();
        UIManager.Instance.HideActionSequence();
        UIManager.Instance.PlayerAttackUI.Opt()?.SetActive( false );
        CancelUIRequests();
        TeardownListeners();
    }


    private void OnWeaponChanged( ActiveWeaponChanged e )
    {
        //CancelUIRequests();
        //Start( _game, _actor );

        PreviewSequenceAction();
    }

    private void PreviewSequenceAction()
    {
        var attack = this._actor.GetActionsOfTypeStrict<PlayerAttackAction>().FirstOrDefault();
        if( attack == null )
            return;
        SmartPoint targetLocation = new SmartPoint( Vector3.zero );
        UIManager.Instance.ActionSequence.PreviewAction( new SequenceAction() { Actor = _actor, Action = attack, Target = targetLocation, UsedWeapon = this._actor.ActiveWeapon } );
    }

    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );
        SetupListeners();
        _game = game;
        _actor = actor;
        _actionSequence = new ActionSequence( actor );
        

        UIManager.Instance.TryPickAction( _actor, ActionPicked, () => { }, ActionCategory.Augment );
        //UIManager.Instance.ShowActionPicker( OnSelect, GetPickableActionCategory() );
        UIManager.Instance.ShowSequenceSelector( actor, _actionSequence, () => _uiWantsToFire = true );
        PreviewSequenceAction();
        UIManager.Instance.PlayerAttackUI.Opt()?.SetActive( true );

        //Gotta subscribe after action sequence UI is shown above becuase otherwise we respond to removal before it does.
        _actionSequence.ActionRemoved += SequenceActionRemoved;

        //Get valid move locations. Notify the UI we need to display a collection of move locations. Wait for UI to return a result. Execute move.
        this.State = ActorActionState.Executing;

        GfxActor attackerAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        //UIManager.Instance.ShowSideAMechInfo( attackerAvatar.Actor, UIManager.MechInfoDisplayMode.Full );

        StartAttackLocationPick();
    }

    private void SequenceActionRemoved( SequenceAction action )
    {
        PreviewSequenceAction();
    }

    private void StartAttackLocationPick()
    {
        GfxActor attackerAvatar = GameEngine.Instance.AvatarManager.GetAvatar( _actor );
        _uiRequest = CreateFindAttackTargetRequest( attackerAvatar );
        //Don't target actors on the same team
        _uiRequest.MarkInvalidTeams( _actor.GetTeamID() );
        UIManager.Instance.RequestUI( _uiRequest, false );
    }

    private void ActionPicked( object result )
    {
        var selected = result as ActorAction;
        
        //This sucker won't work. We need more info for the sequence action.
        UIManager.Instance.ActionSequence.AddSequenceAction( new SequenceAction() { Actor = _actor, Action = selected, Target = null } );
        //Realistically the only thing we should be doing when we pick actions this way is picking augmentations to the existing actual actions.
        
        
        UIManager.Instance.TryPickAction( _actor, ActionPicked, () => { }, ActionCategory.Attack );
        //UIManager.Instance.ShowActionPicker( OnSelect, GetPickableActionCategory() );
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

            var attack = this._actor.GetActionsOfTypeStrict<PlayerAttackAction>().FirstOrDefault();
            if( attack == null )
                return;

            if( attackerAvatar.Actor.ActiveWeapon.IsAOE() )
            {
                var location = (Vector2Int) selectedTarget;

                SmartPoint targetLocation = new SmartPoint( new Vector3( location.x, 0, location.y ) );
                UIManager.Instance.ActionSequence.AddSequenceAction( new SequenceAction() { Actor = _actor, Action = attack, Target = targetLocation, UsedWeapon = this._actor.ActiveWeapon } );
            }
            else
            {
                var actor = (Actor)selectedTarget;
                var targetAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );

                SmartPoint targetLocation = new SmartPoint( actor );
                UIManager.Instance.ActionSequence.AddSequenceAction( new SequenceAction() { Actor = _actor, Action = attack, Target = targetLocation, UsedWeapon = this._actor.ActiveWeapon } );
            }

            PreviewSequenceAction();
            StartAttackLocationPick();
        };

        UIRequestFailureCallback<bool> failure = moveTarget => { End(); _uiRequest = null; };
        UIRequestCancelResult cancel = () => { End(); _uiRequest = null; };
        var request = new UIFindAttackTargetRequest( attackerAvatar.Actor, success, failure, cancel );

        return request;
    }


    public override void TurnEnded()
    {
        End();
    }

    private void CancelUIRequests()
    {
        _uiRequest?.Cancel();
        _uiRequest = null;

        AllowActionSelect = true;
    }

}

