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
    private Game _game;
    private Actor _actor;
    private UIFindAttackTargetRequest _uiRequest = null;
    private UIPickWeaponRequest _weaponPickRequest = null;

    public override bool HideAP => true;

    public override void Tick()
    {
        return;
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
        State = ActorActionState.Finished;
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

        if( DisplayProps != null && DisplayProps.IsSequenceStart )
        {
            if( actor.Target?.GfxActor != null )
                UIManager.Instance.ShowSideBMechInfo( actor.Target.GfxActor.Actor, UIManager.MechInfoDisplayMode.Full, true );
        }

        //Get valid move locations. Notify the UI we need to display a collection of move locations. Wait for UI to return a result. Execute move.
        State = ActorActionState.Started;
        GfxActor attackerAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        DoAttack( attackerAvatar );
    }

    private void DoAttack( GfxActor attackerAvatar )
    {
        var selectedTarget = this.Target;
        SpendAP( attackerAvatar.Actor );
        State = ActorActionState.Executing;
        
        AttackActionResult res = new AttackActionResult( attackerAvatar, this.Target, this.UsedWeapon );
        res.DisplayProps = this.DisplayProps;
        res.OnComplete = () =>
        {
            if( res.DisplayProps.IsSequenceEnd )
            {
                UIManager.Instance.HideSideBMechInfo( true );
            }
            _uiRequest = null;
            End();
        };

        AttackHelper.CalculateAttackDamage( res );

        TestKilledTargets( res );

        UIManager.Instance.QueueResult( res );
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

