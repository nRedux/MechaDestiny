using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Game
{

    private List<Team> _teams = new List<Team>();
    private bool _gameOver = false;

    public List<Team> Teams => _teams;

    public int TeamCount => _teams.Count;

    public bool RunGameLogic { get; set; } = true;

    public Board Board { get; private set; }

    public TurnManager TurnManager{ get; private set; }


    public Game( int boardWidth, int boardHeight )
    {
        this.Board = new Board( boardWidth, boardHeight );
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


    public Team GetTeam( int index )
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

    internal void SetWalkability( BoolWindow walkableCells )
    {
        Board.SetWalkability( walkableCells );
    }


    /*
    public List<Actor> GetActorsInRange( int originX, int originY, int range, int teamID )
    {
        var team = Teams[teamID];
        var teamMembers = team.GetMembers();
        bool[,] cellsInRange = new bool[range * 2, range * 2];
        Board.GetCellsManhattan( originX, originY, range, cellsInRange );
        List<Actor> results = new List<Actor>();

        for( int x = 0; x < range * 2; x++ )
        {
            for( int y = 0; y < range * 2; y++ )
            {
                Vector2Int cell = new Vector2Int( x - range, y - range );
                if( cellsInRange[cell.x, cell.y] )
                {
                    var foundActor = teamMembers.Where( x => x.GetBoardPosition() == cell ).FirstOrDefault();
                    if( foundActor != null )
                        results.Add( foundActor );
                }
            }
        }

        return results;
    }
    */

}
