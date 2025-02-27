using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Team
{
    public TurnPhase[] TurnPhases;
    public bool IsPlayerTeam = false;

    private List<Actor> _members = new List<Actor>();
    private int _teamID;

    public int MemberCount => _members.Count;
    public int Id => _teamID;


    public Team(Team other )
    {
        this.TurnPhases = other.TurnPhases;
        this.IsPlayerTeam = other.IsPlayerTeam;
        this._members = new List<Actor>();
        for( int i = 0; i < other._members.Count; i++ )
        {
            this._members[i] = new Actor( other._members[i] );
        }
        this._teamID = other._teamID;
    }


    public Team( bool isPlayerTeam )
    {
        this.IsPlayerTeam = isPlayerTeam;
    }


    public void SetTeamID( int id )
    {
        _teamID = id;
    }


    public void AddMember( Actor actor )
    {
        if( actor == null )
            return;
        actor.SetIsPlayer( this.IsPlayerTeam );
        actor.SetTeam( this );
        _members.Add( actor );
    }


    public void RemoveMember( Actor actor )
    {
        actor.SetTeam( null );
        _members.Remove( actor );
    }

    public void Cleanup()
    {
        _members.Do( x => x.Cleanup() );
    }

    internal void TurnEnded()
    {
        
    }


    public Actor GetMember( int index )
    {
        return _members[index];
    }


    public List<Actor> GetMembers()
    {
        return _members;
    }


    public bool AllMemberActionsExhausted()
    {
        return false;
    }


    public void SetPhases()
    {

    }


    public void SetTurnPhases( TurnPhase[] phases )
    {
        TurnPhases = phases;
    }


    public bool HasLivingMembers()
    {
        int livingCount = this._members.Count( x => !x.IsDead() );
        return livingCount != 0;
    }

    public List<Actor> MembersAdjacentPriority( Actor actor, int offset )
    {
        return _members.Where( x => x.SpawnPriority == actor.SpawnPriority + offset ).ToList();
    }

    public List<Actor> MembersGreaterPriority( Actor actor )
    {
        return _members.Where( x => x.SpawnPriority > actor.SpawnPriority ).ToList();
    }

    public List<Actor> MembersLowerPriority( Actor actor )
    {
        return _members.Where( x => x.SpawnPriority < actor.SpawnPriority ).ToList();
    }

}
