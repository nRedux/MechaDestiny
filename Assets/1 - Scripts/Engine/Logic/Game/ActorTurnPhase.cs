using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ActorTurnPhase: TurnPhase
{
    private const int ACTIVE_ACTOR_START_VALUE = -1;

    private Game _game;
    private Team _team;
    private bool _isComplete = false;
    private float _delayAIActionsTimer = 0f;
    private int _activeActorIndex = ACTIVE_ACTOR_START_VALUE;
    private Actor _activeActor;

    public override bool CanEndTurn => false;

    public override bool EndTurn => false;

    public override bool EndOnPhasesExhausted => true;


    public override void Setup( Game game, Team team )
    {
        _game = game;
        _team = team;
        SetActiveActor( null );
        _isComplete = false;

        for( int i = 0; i < team.MemberCount; ++i )
        {
            var member = team.GetMember( i );
            member.ResetForPhase();
        }
        StartNextActor( game );
    }


    private void StartNextActor( Game game )
    {
        //Find first member in team which hasn't performed it's turn actions yet.
        var members = _team.GetMembers();
        var nextMember = members.Where( x => !x.TurnActionsCompleted && !x.IsDead() ).FirstOrDefault();

        //Turn phase complete?
        if( nextMember == null )
        {
            //All actors done.
            _isComplete = true;
            SetActiveActor( null );
            return;
        }

        //Do some AI specific stuff
        if( !_team.IsPlayerTeam )
        {
            //Debug.Log( "Start next actor for phase" );
            _delayAIActionsTimer = DevConfiguration.DELAY_AI_ACTIONS_DURATION;
        }

        SetActiveActor( nextMember );
    }


    private void SetActiveActor( Actor actor )
    {
        if( _activeActor != null && _activeActor != actor )
        {
            UIManager.Instance.TerminateActiveRequests( _activeActor );
            UIManager.Instance.TerminatePending( _activeActor );
        }

        _activeActor = actor;
        Events.Instance.Raise( new CurrentActorEvent() { Actor = actor } );
    }


    private bool HasActiveActor()
    {
        return _activeActor != null;
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
        _activeActor.RunActions( _game );

        if( _activeActor.EndActorTurn() )
        {
            _activeActor.TurnActionsCompleted = true;
            SetActiveActor( null );
        }
    }

    public override void TurnEnded()
    {
        _activeActor?.TurnEnded();
        SetActiveActor( null );
    }


    public override bool IsComplete()
    {
        return _isComplete;
    }

}
