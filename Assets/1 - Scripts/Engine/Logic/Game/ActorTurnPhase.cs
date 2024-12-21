using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ActorTurnPhase: TurnPhase
{

    private const int ACTIVE_ACTOR_START_VALUE = -1;
    private Game _game;
    private Team _team;
    private bool _isComplete = false;

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
        _activeActorIndex = ACTIVE_ACTOR_START_VALUE;

        for( int i = 0; i < team.MemberCount; ++i )
        {
            var member = team.GetMember( i );
            member.PrepareForPhase();
        }
        StartNextActor( game );
    }

    private void StartNextActor( Game game )
    {
        if( _activeActorIndex + 1 >= _team.MemberCount )
        {
            //All actors done.
            _isComplete = true;
            SetActiveActor( null );
            return;
        }
        //Debug.Log( "Start next actor for phase" );
        SetActiveActor( _team.GetMember( ++_activeActorIndex ) );
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
        _activeActor.RunActions( _game );

        if( !_activeActor.HasActionsAvailable() && _activeActor.ActiveAction == null || _activeActor.ShouldForceEnd() )
            SetActiveActor( null );
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
