using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using Newtonsoft.Json;
using System.Linq;

public class TurnManager
{
    private int _turnNumber = -1;
    private int _activePhaseIndex = -1;
    private Game _game = null;

    private AIPlayerHandler _aiPlayerHandler = null;

    public Team ActiveTeam => _game.GetTeam( _turnNumber % _game.TeamCount );
    public Team NextTeam => _game.GetTeam( (_turnNumber + 1) % _game.TeamCount );

    public int TurnNumber { get { return _turnNumber; } }


    [JsonConstructor]
    public TurnManager()
    {

    }


    public TurnManager( TurnManager other )
    {
        this._game = other._game;
        this._activePhaseIndex = other._activePhaseIndex;
        this._turnNumber = other._turnNumber;
    }


    //TODO: This is a really rough way of asking "victory or defeat achieved"!
    //It's simple, but needs replacement. It lets us know if game is over. First team (in an assumed 2 team battle) to lose it's people, loses the game.
    public bool HasTeamDied( out Team deadTeam )
    {
        deadTeam = null;
        var deadMatch = _game.Teams.Where( x => !x.HasLivingMembers() ).FirstOrDefault();
        if( deadMatch != null )
            deadTeam = deadMatch;
        return deadTeam != null;
    }


    public void SetGameReference(Game game )
    {
        this._game = game;
    }


    public TurnManager( Game game )
    {
        this._game = game;
    }


    public void StartNextTurn()
    {
        _activePhaseIndex = -1;
        _turnNumber++;

        StartNextPhase();

        if( LuaBehaviorManager.Instance != null )
            LuaBehaviorManager.Instance.OnTurnChanged( this );

        if( !ActiveTeam.IsPlayerTeam )
            _aiPlayerHandler = new AIPlayerHandler( _game );
    }


    public void EndTurn()
    {
        if( _turnNumber < 0 )
            return;
        if( ActiveTeam == null )
            return;
        if( _activePhaseIndex < 0 || _activePhaseIndex >= ActiveTeam.TurnPhases.Length )
            return;
        ActiveTeam.TurnPhases[_activePhaseIndex].TurnEnded();
    }


    public bool Tick()
    {
        bool turnFinished = false;
        if( ActiveTeam.IsPlayerTeam )
        {
            TickActivePhase();
            turnFinished = ShouldEndTurn();
        }
        else
        {
            TickActivePhase();
            turnFinished = ShouldEndTurn();
        }
        return turnFinished;
    }


    private bool PhasesExhausted()
    {
        return _activePhaseIndex + 1 >= ActiveTeam.TurnPhases.Length;
    }

    private bool ActivePhaseComplete()
    {
        //No reason to advance the phase?
        if( _activePhaseIndex < 0 )
            return false;

        return ActiveTeam.TurnPhases[_activePhaseIndex].IsComplete();
    }

    private bool HasMorePhases()
    {
        //No reason to advance the phase?
        if( _activePhaseIndex < 0 )
            return false;

        return _activePhaseIndex + 1 < ActiveTeam.TurnPhases.Length;
    }

    private void StartNextPhase()
    {
        //Increment phase index
        _activePhaseIndex++;
        //Debug.Log("Starting new turn");
        ActiveTeam.TurnPhases[_activePhaseIndex].Setup( _game, ActiveTeam );
    }


    private bool ShouldEndTurn()
    {
        if( ActivePhaseWantsEndTurn() )
            return true;

        return ActivePhaseComplete() && 
            ( EndTurnFromPhaseExhaustion() || EndTurnPhaseRequested() );
    }

    private bool EndTurnFromPhaseExhaustion()
    {
        return PhasesExhausted() && ActiveTeam.TurnPhases[_activePhaseIndex].EndOnPhasesExhausted;
    }

    private bool EndTurnPhaseRequested()
    {
        return ActiveTeam.TurnPhases[_activePhaseIndex].CanEndTurn &&
            ActiveTeam.TurnPhases[_activePhaseIndex].EndTurn;
    }

    public TurnPhase GetActivePhase()
    {
        return ActiveTeam.TurnPhases[_activePhaseIndex];
    }

    private void TickActivePhase()
    {
        ActiveTeam.TurnPhases[_activePhaseIndex].Tick( _game );
        if( ActivePhaseComplete() )
        {
            if( !PhasesExhausted() )
                StartNextPhase();
        }
    }

    private bool ActivePhaseWantsEndTurn()
    {
        if( _activePhaseIndex >= ActiveTeam.TurnPhases.Length )
            return false;
        if( !ActiveTeam.TurnPhases[_activePhaseIndex].CanEndTurn )
            return false;
        return ActiveTeam.TurnPhases[_activePhaseIndex].EndTurn;
    }

}
