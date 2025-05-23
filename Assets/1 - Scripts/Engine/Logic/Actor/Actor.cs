using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public enum SequencePos
{
    SequenceStart,
    Both,
    Start,
    Mid,
    End,
    SequenceEnd
}

public enum ActorStatus
{
    Inactive,
    Active
}

public class ResultDisplayProps
{
    public bool IsSequenceStart;
    public bool DoArmStart;
    public bool DoArmEnd;
    public bool IsSequenceEnd;

    public override string ToString()
    {
        return $"{nameof( ResultDisplayProps )}: {nameof(IsSequenceStart)}:{IsSequenceStart}, {nameof( DoArmStart )}:{DoArmStart}, {nameof( DoArmEnd )}:{DoArmEnd}, {nameof( IsSequenceEnd )}:{IsSequenceEnd}";
    }
}


public enum ActorEvent
{
    Attacked
}

[JsonObject]
[System.Serializable]
public partial class Actor : SimpleEntity<ActorAsset>
{
    private const int MAX_PHASE_ACTIONS = 10;
    [JsonIgnore]
    public ActorAction[] Actions = null;
    public GameObjectReference Avatar;

    /// <summary>
    /// Run all actions automatically - for NPCs
    /// </summary>
    [JsonIgnore]
    public System.Action OnKill;
    public bool RunActionsAutomatically;
    public bool TurnComplete = false;

    private Team _team;
    private ActorActionHandler _actionHandler;
    private bool _diePerformed = false;

    public ActorStatus Status
    {
        get;
        set;
    } = ActorStatus.Active;

    public int SpawnPriority;

    [JsonIgnore]
    public LuaBehavior LuaScript;

    //Actor needs to be able to perform actions such as moving, attacking, executing abilities, etc.
    //Actor needs to be able to track it's statistics and have values be applied
    public Vector2Int Position { get; private set; }
    [JsonIgnore]
    public bool IsPlayer { get; private set; } = true;
    [JsonIgnore]
    public SmartPoint Target { get; set; } = null;
    [JsonIgnore]
    public List<SequenceAction> Sequence { get; set; }

    [JsonIgnore]
    public ActorAction ActiveAction { get => _actionHandler?.ActiveAction; }


    private AIPersonality _personalityInstance;

    [JsonIgnore]
    public AIPersonality AIPersonality
    {
        get
        {
            return _personalityInstance;
        }
        private set
        {
            _personalityInstance = value;
        }
    }

    public void InitializeAIPersonality( AIPersonality source )
    {
        if( source == null ) return;
        AIPersonality = source.Clone();
    }

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
        _actionHandler = isPlayer ? new PlayerActionHandler( this ) : new AIActionHandler( this );
    }

    [JsonIgnore]
    public MechComponentData ActiveWeapon
    {
        get
        {
            return GetMechData().ActiveWeapon;
        }
        set
        {
            GetMechData().ActiveWeapon = value;
        }
    }

    public void InitializeForCombat()
    {
        var asset = GetAssetSync();
        InitializeAIPersonality( asset.AI_Personality );
        Actions = asset.GetActions().Select( x => x.GetData() ).ToArray();
    }

    public MechData GetMechData()
    {
        return GetSubEntities()[0] as MechData;
    }

    public void SetPosition( Vector2Int newPosition, Game game )
    {
        Vector3 curWorldPosition = this.Position.GetWorldPosition();
        Vector3 newWorldPosition = newPosition.GetWorldPosition();

        if( game.Board.IsBlocked( newWorldPosition ) )
        {
            throw new System.Exception("Invalid position request.");
        }

        GameEngine.Instance.Board.Unblock( curWorldPosition );
        GameEngine.Instance.Board.Block( this, newWorldPosition );
        this.Position = newPosition;
    }


    public List<ActionType> GetActionsOfType<ActionType>() where ActionType : ActorAction
    {
        return Actions.Where( x => typeof(ActionType).IsAssignableFrom( x.GetType() ) ).Select( x => x as ActionType ).ToList();
    }

    public List<ActionType> GetActionsOfTypeStrict<ActionType>() where ActionType : ActorAction
    {
        return Actions.Where( x => typeof( ActionType ) == x.GetType() ).Select( x => x as ActionType ).ToList();
    }


    public List<ActionType> GetAIActionsNotOfType<ActionType>() where ActionType : ActorAction
    {
        return Actions.Where( x => !typeof( ActionType ).IsAssignableFrom( x.GetType() ) ).Select( x => x as ActionType ).ToList();
    }

    public void Selected()
    {
        _actionHandler.Selected();
    }

    public void Deselected()
    {
        _actionHandler.Deselected();
    }


    public void ResetForPhase()
    {
        TurnComplete = false;
        _actionHandler.SetupForTurn();
        Actions.Do( x => x.ResetForPhase() );
        DoStatisticsResetForTurn();
    }


    private void DoStatisticsResetForTurn()
    {
        RestoreAbilityPointsForTurn();
        ResetBoostsStatistics();
    }

    public void TriggerEvent( ActorEvent evt, Actor source )
    {
        foreach( var action in Actions )
        {
            if( !action.RespondToEvents )
                continue;

            action.TriggerEvent( this, source, evt );
        }
    }


    /// <summary>
    /// Regain ability points each turn.
    /// </summary>
    public void RestoreAbilityPointsForTurn()
    {
        var ap = GetStatistic( StatisticType.AbilityPoints );
        var maxAP = GetStatisticValue( StatisticType.MaxAbilityPoints );
        ap.SetValue( maxAP );
    }

    public void ResetBoostsStatistics()
    {
        MechData md = this.GetMechData();
        //Fresh copy of base statistics
        Boosts = new StatisticCollection( this.Statistics );
        //Merge in statistics from mech components (not including weapons, etc)
        Boosts.Merge( md.AccumulateStatistics() );
    }


    public void ResetOnSelect()
    {
        _actionHandler.SetupForTurn();
    }


    public void SetTeam( Team team )
    {
        _team = team;
    }


    /// <summary>
    /// Get what type of actions you can use on the target
    /// </summary>
    /// <param name="target">The target actor</param>
    /// <returns>The things you can do</returns>
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
        return this._actionHandler.GetActionOptions( category );
    }

    public bool Die()
    {
        if( _diePerformed )
            return false;
        if( OnKill != null )
            OnKill.Invoke();

        if( this._team == null )
            return true;
        this._team.RemoveMember( this );
        return true;
    }

    public void Kill()
    {
        var mech = GetMechData();
        var health = mech.Torso.GetStatistic( StatisticType.Health );
        if( health == null )
            return;
        health.EmitUIChangeImmediate = true;
        health.SetValue( 0 );
        Die();
    }


    public void Cleanup()
    {
        if( ActiveAction != null )
            ActiveAction.End();
    }


    public void RunActions( Game game )
    {
        _actionHandler.Tick( game );
    }

    public bool CanBeInterrupted()
    {
        return _actionHandler.CanBeInterrupted();
    }


    public void TurnEnded()
    {
        _actionHandler.TurnEnded();
    }


    public bool HasActionsAvailable()
    {
        return _actionHandler.HasActionsAvailable();
    }


    public bool ShouldEndActorTurn()
    {
        if( _team.IsPlayerTeam )
            return _actionHandler.ShouldEndActorTurn();
        else
            return !HasActionsAvailable() && ActiveAction == null;
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

    public List<Actor> MembersAdjacentPriority( int offset )
    {
        if( _team == null )
            return new List<Actor>();
        return _team.MembersAdjacentPriority( this, offset );
    }

    public List<Actor> MembersGreaterPriority( int offset )
    {
        if( _team == null )
            return new List<Actor>();
        return _team.MembersGreaterPriority( this );
    }

    public List<Actor> MembersLowerPriority( int offset )
    {
        if( _team == null )
            return new List<Actor>();
        return _team.MembersLowerPriority( this );
    }
}
