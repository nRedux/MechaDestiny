using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game
{

    private List<Team> _teams = new List<Team>();
    private bool _gameOver = false;

    public List<Team> Teams => _teams;

    public int TeamCount => _teams.Count;

    public bool RunGameLogic { get; set; } = true;

    public Board Board { get; private set; }

    public TurnManager TurnManager{ get; private set; }


    public Game()
    {
        var pathing = Object.FindFirstObjectByType<AstarPath>();

        if( pathing == null )
            throw new System.Exception( "AStarPath must exist in the scene" );

        this.Board = new Board( pathing );
        this.TurnManager = new TurnManager( this );
    }


    public Game( Game other )
    {
        _teams = new List<Team>();
        for( int i = 0; i < _teams.Count; i++ )
        {
            this._teams.Add( new Team( other._teams[i] ) );
        }

        this.Board = new Board( other.Board );
        this.TurnManager = new TurnManager( other.TurnManager );
        this.TurnManager.SetGameReference( this );
    }


    public void Start()
    {
        TurnManager.StartNextTurn();
    }


    public void Cleanup()
    {
        Teams.Do( x => x.Cleanup() );
    }


    public void AddTeam( Team team )
    {
        team.SetTeamID( TeamCount );
        _teams.Add( team );
    }


    public Team GetTeam( int index  )
    {
        return _teams[index];
    }


    public List<Team> GetOtherTeams( int thisTeam )
    {
        return _teams.Where( x => x.Id != thisTeam ).ToList();
    }


    public Actor GetRandomMemberFromTeam( int teamID )
    {
        return _teams[teamID].GetMembers().Random();
    }


    public Actor GetRandomMemberFromOtherTeams( int thisTeamID )
    {
        var otherTeam = GetOtherTeams(thisTeamID).Random();
        return otherTeam.GetMembers().Where(x => !x.IsDead()).Random();
    }


    public Actor GetRandomMemberFromOtherTeams( int thisTeamID, BoolWindow validLocations )
    {
        var otherTeam = GetOtherTeams( thisTeamID ).Random();
        return otherTeam.GetMembers().Where( x => !x.IsDead() && validLocations.ContainsWorldCoord( x.Position ) && validLocations.GetValueAtWorldCoord( x.Position ) ).Random();
    }


    public Actor GetActor( int thisTeamID )
    {
        var otherTeam = GetOtherTeams( thisTeamID ).Random();
        return otherTeam.GetMembers().Random();
    }

    public void Tick()
    {
        if( !RunGameLogic )
            return;
        if( _gameOver )
            return;

        bool turnFinished = TurnManager.Tick();
        if( turnFinished )
        {
            TurnManager.StartNextTurn();
            Events.Instance.Raise( new GameTurnChangeEvent( this ) );
        }
    }

    public bool CheckGameOver()
    {
        if( _gameOver )
            return true;

        //Super simple game over check!
        //TODO: ANYTHING better than this. Ideally the win condition is checked for elsewhere.
        //I would say this is a place for moonsharp! (or any other scripted solution, we don't want this hard coded).
        Team deadTeam = null;
        bool gameOverResult = TurnManager.HasTeamDied( out deadTeam );
        if( gameOverResult )
        {
            Debug.Log( "Game Over!" );
        }

        _gameOver = _gameOver || gameOverResult;
        if( _gameOver )
        {
            //Need to have this trigger only once a graphics thing says combat end should be checked.
            CoroutineUtils.DoDelay( 2f, () =>
            {
                Events.Instance.Raise( new GameOverEvent { Winner = deadTeam.Id == 0 ? 1 : 0 } );
            } );
        }
        return gameOverResult;
    }

}
