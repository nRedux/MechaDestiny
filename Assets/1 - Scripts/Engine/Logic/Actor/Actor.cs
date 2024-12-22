using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

using System;
using FMOD;

using Debug = UnityEngine.Debug;

public enum SequencePos
{
    Unset,
    Start,
    Mid,
    End,
}


[System.Serializable]
public class Actor : SimpleEntity<ActorAsset>
{
    private const int MAX_PHASE_ACTIONS = 10;

    public ActorAction[] Actions = null;
    public GameObjectReference Avatar;
    public ActorAction ActiveAction { get => ActionHandler?.ActiveAction; }

    /// <summary>
    /// Run all actions automatically - for NPCs
    /// </summary>
    public bool RunActionsAutomatically;
    public System.Action OnKill;


    private Team _team;
    private ActorActionHandler ActionHandler;


    //Actor needs to be able to perform actions such as moving, attacking, executing abilities, etc.
    //Actor needs to be able to track it's statistics and have values be applied
    public Vector2Int Position { get; private set; }
    public bool IsPlayer { get; private set; } = true;
    public Actor Target { get; set; }


    [JsonConstructor]
    public Actor() { }


    public Actor( Actor other )
    {
        //May need deep copy?
        Actions = new ActorAction[other.Actions.Length];
        for( int i = 0; i < other.Actions.Length; i++ )
        {
            Actions[i] = other.Actions[i];
        }

        //SetPosition( other.Position );
    }


    public void SetIsPlayer( bool isPlayer )
    {
        this.IsPlayer = isPlayer;
        ActionHandler = isPlayer ? new PlayerActionHandler( this ) : new AIActionHandler( this );
    }


    public MechData GetMechData()
    {
        return GetSubEntities()[0] as MechData;
    }

    public void SetPosition( Vector2Int position, Game game )
    {
        if( !game.Board.CanActorOccupyCell( position ) )
            return;//TODO: How is the calling code supposed to know this failed???

        game.Board.SetActorOccupiesCell( Position, false );
        game.Board.SetActorOccupiesCell( position, true );
        this.Position = position;
    }


    public List<ActorAction> GetActionsOfType( ActionType type )
    {
        return Actions.Where( x => x.ActionPhase == type ).ToList();
    }


    public List<ActorAction> GetActionsNotOfType( ActionType type )
    {
        return Actions.Where( x => x.ActionPhase != type ).ToList();
    }


    /*
     * Players should be able perform actions continuously until their action points are exhausted.
     */

    public void PrepareForPhase()
    {
        ActionHandler.SetupForPhase();
    }


    public void SetTeam( Team team )
    {
        _team = team;
    }


    public ActionCategory GetInteractionCategory( Actor target )
    {
        if( target == null )
            return ActionCategory.NONE;
        return GetTeamID() == target.GetTeamID() ? ActionCategory.Support : ActionCategory.Attack;
    }


    public int GetTeamID()
    {
        if( _team == null )
            return -1;
        return _team.Id;
    }


    public List<ActorAction> GetActionOptions( ActionCategory category )
    {
        return this.ActionHandler.GetActionOptions( category );
    }

    public void Die()
    {
        if( OnKill != null )
            OnKill.Invoke();

        if( this._team == null )
            return;
        this._team.RemoveMember( this );
    }


    public void Cleanup()
    {
        if( ActiveAction != null )
            ActiveAction.End();
    }


    public void RunActions( Game game )
    {
        ActionHandler.Tick( game );
    }


    public void TurnEnded()
    {
        ActionHandler.TurnEnded();
    }


    public bool HasActionsAvailable()
    {
        return ActionHandler.HasActionsAvailable();
    }


    public bool ShouldForceEnd()
    {
        return ActionHandler.ShouldForceEndActor();
    }


    public Vector3 GetWorldPosition()
    {
        return new Vector3( Position.x + .5f, 0, Position.y + .5f );
    }


    public override bool IsDead()
    {
        var subEnts = GetSubEntities();
        if( subEnts.Count == 0 )
            return false;

        //first sub entity of actor should be mech, or pilot, or whatever. We check that entity for death.
        return subEnts[0].IsDead();
    }


    /// <summary>
    /// Tests if a statistic can have a given amount deducted as in the case of a transaction. Statistic must have a value greater than 0.
    /// </summary>
    /// <param name="statistic"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool CanSpendStatistic( StatisticType statistic, int amount )
    {
        var ap = GetStatistic( statistic )?.Value ?? 0;
        return ap - amount >= 0;
    }


    /// <summary>
    /// Spend an amount of a statistic.
    /// </summary>
    /// <param name="statistic">The statistic to spend from</param>
    /// <param name="amount">The amount to spend</param>
    /// <returns></returns>
    public bool SpendStatistic( StatisticType statistic, int amount, bool updateUI = true )
    {
        if( !CanSpendStatistic( statistic, amount ) )
            return false;
        var stat = GetStatistic( statistic );
        stat.EmitUIChangeImmediate |= updateUI;
        stat.SetValue( Mathf.Max( 0, stat.Value - amount ) );
        return true;
    }
}
