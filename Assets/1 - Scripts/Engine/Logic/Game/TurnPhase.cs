using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;

public abstract class TurnPhase
{
    public abstract bool EndOnPhasesExhausted { get; }
    public abstract bool CanEndTurn { get; }
    public abstract bool EndTurn { get; } 

    public abstract void Setup( Game game, Team team );

    public abstract void Tick( Game game );
    public abstract void TurnEnded();

    public abstract bool IsComplete();
}



public class PlayerPhase : ActorTurnPhase, IGameEventListener
{

    public override bool CanEndTurn => true;
    public override bool EndTurn => _endTurn;

    private bool _endTurn = false;

    public PlayerPhase( )
    {
    }


    public PlayerPhase( GameEventBase endEvent )
    {
        endEvent.AddListener( this );
    }


    public override void Setup( Game game, Team team )
    {
        base.Setup( game, team );
        _endTurn = false;
    }

    public void OnEventRaised()
    {
        _endTurn = true;
    }
}


public class AITurnPhase : ActorTurnPhase
{

    public override bool CanEndTurn => true;

    public override bool EndTurn => false;

    public override bool EndOnPhasesExhausted => true;


    public AITurnPhase()
    {
    }


    public override void Setup( Game game, Team team )
    {
        base.Setup( game, team );
    }

}