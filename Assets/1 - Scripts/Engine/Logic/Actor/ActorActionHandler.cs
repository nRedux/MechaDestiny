using System;
using System.Collections.Generic;
using UnityEngine;


public abstract class ActorActionHandler
{
    protected Actor _actor;

    public ActorActionHandler( Actor actor )
    {
        this._actor = actor;
    }

    public abstract bool HasActionsAvailable();
    public abstract void Tick( Game game );
    public abstract bool CanBeInterrupted();
    public abstract void SetupForTurn();
    public abstract bool ShouldEndActorTurn();
    public abstract ActorAction ActiveAction { get; }
    public abstract void TurnEnded();

    public abstract List<ActorAction> GetActionOptions( ActionCategory category );

    public abstract void Selected();

    public abstract void Deselected();
}
