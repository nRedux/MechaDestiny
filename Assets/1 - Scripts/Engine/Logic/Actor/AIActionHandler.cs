using System.Collections.Generic;
using System;
using UnityEngine;


public class AIActionHandler : ActorActionHandler
{

    private ActorAction _activeAction;
    private List<ActorAction> _phaseActions = new List<ActorAction>();

    public override ActorAction ActiveAction => _activeAction;

    public AIActionHandler( Actor actor ) : base( actor )
    {

    }

    public override bool ShouldForceEndActor()
    {
        return false;
    }

    public override void SetupForPhase()
    {
        _phaseActions = new List<ActorAction>();
        int numActions = _actor.Actions.Length;

        ActionType[] type = new ActionType[] { ActionType.Move, ActionType.Attack };
        int numTypes = type.Length;
        for( int i = 0; i < numActions; i++ )
        {
            //Cycle through requested
            for( int p = 0; p < numTypes; p++ )
            {
                if( _actor.Actions[i].ActionPhase == type[p] )
                {
                    _phaseActions.Add( _actor.Actions[i] );
                }
            }
        }
    }


    public override bool HasActionsAvailable()
    {
        return _phaseActions.Count > 0;
    }


    public override void Tick( Game game )
    {
        StartActionIfNoneActive( game );

        if( _activeAction != null )
        {
            _activeAction.Tick();

            if( _activeAction.State() == ActorActionState.Finished )
            {
                //Why isn't it ending itself if it's completed?
                _activeAction.End();
                _activeAction = null;
            }
        }
    }


    public void StartActionIfNoneActive( Game game )
    {
        if( _activeAction != null )
            return;
        StartNextAction( game );
    }


    public void StartNextAction( Game game )
    {
        _activeAction = GetNextAction();
        if( _activeAction != null )
            _activeAction.Start( game, _actor );
    }


    private ActorAction GetNextAction()
    {
        if( HasActionsAvailable() )
        {
            var result = _phaseActions.First();
            _phaseActions.RemoveAt( 0 );
            return result;
        }

        return null;
    }

    public override void TurnEnded()
    {
        if( _activeAction != null )
            _activeAction.TurnEnded();
    }

    public override List<ActorAction> GetActionOptions( ActionCategory category )
    {
        throw new NotImplementedException();
    }
}