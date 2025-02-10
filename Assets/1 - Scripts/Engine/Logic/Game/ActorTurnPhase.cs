using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class ActorTurnPhase: TurnPhase
{
    private Game _game;
    private Team _team;
    private bool _isComplete = false;
    private float _delayAIActionsTimer = 0f;

    private Actor _selectedActor;

    public override bool CanEndTurn => false;

    public override bool EndTurn => false;

    public override bool EndOnPhasesExhausted => true;


    public Actor SelectedActor
    {
        get => _selectedActor;
    }


    public override void Setup( Game game, Team team )
    {

        _game = game;
        _team = team;
        _isComplete = false;

        SetUpInput();
        SelectActiveActor( null );
        
        for( int i = 0; i < team.MemberCount; ++i )
        {
            var member = team.GetMember( i );
            member.ResetForPhase();
        }
        StartNextActor( game );
    }

    private void SetUpInput()
    {
        if( !_team.IsPlayerTeam )
            return;
        UIManager.Instance.UserControls.SelectMech.AddActivateListener( TrySelectMech ); 
    }

    private void ShutDownInput()
    {
        UIManager.Instance.UserControls.SelectMech.RemoveActivateListener( TrySelectMech );
    }

    private void TrySelectMech( InputActionEvent evt )
    {
        if( evt.Used )
            return;
        if( evt.UserData == null )
            return;
        Actor evtActor = (Actor) evt.UserData;
        if( evtActor == _selectedActor )
            return;

        SelectActiveActor( evtActor );
    }

    private void StartNextActor( Game game )
    {
        //Find first member in team which hasn't performed it's turn actions yet.
        var members = _team.GetMembers();
        var nextMember = members.Where( x => !x.TurnComplete && !x.IsDead() ).FirstOrDefault();

        //Turn phase complete?
        if( nextMember == null )
        {
            //All actors done.
            _isComplete = true;
            SelectActiveActor( null );
            return;
        }

        //Do some AI specific stuff
        if( !_team.IsPlayerTeam )
        {
            //Debug.Log( "Start next actor for phase" );
            _delayAIActionsTimer = DevConfiguration.DELAY_AI_ACTIONS_DURATION;
        }

        SelectActiveActor( nextMember );
    }


    private void SelectActiveActor( Actor actor )
    {
        if( actor != null && actor.TurnComplete )
            return;

        if( _selectedActor != null && _selectedActor != actor )
        {
            _selectedActor.Deselected();
            UIManager.Instance.TerminateActiveRequests( _selectedActor );
            UIManager.Instance.TerminatePending( _selectedActor );
        }

        _selectedActor = actor;
        _selectedActor?.ResetOnSelect();
        _selectedActor?.Selected();
        Events.Instance.Raise( new CurrentActorEvent() { Actor = actor } );
    }


    private bool HasActiveActor()
    {
        return _selectedActor != null;
    }


    public override void Tick( Game game )
    {
        //_isComplete = _team.AllMemberActionsExhausted();

        if( !HasActiveActor() )
        {
            StartNextActor( game );
        }
        else
        {
            RunActorActions();
        }

    }


    private void RunActorActions()
    {
        _delayAIActionsTimer -= Time.deltaTime;
        if( _delayAIActionsTimer > 0 )
            return;
        

        if( _selectedActor.ShouldEndActorTurn() )
        {
            _selectedActor.TurnComplete = true;
            SelectActiveActor( null );
        }

        //Could be null if we just ended above
        if( _selectedActor != null )
            _selectedActor.RunActions( _game );
    }


    public override void TurnEnded()
    {
        _selectedActor?.TurnEnded();
        SelectActiveActor( null );
        ShutDownInput();
    }


    public override bool IsComplete()
    {
        return _isComplete;
    }

}
