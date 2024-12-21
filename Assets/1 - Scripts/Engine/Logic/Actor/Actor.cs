using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

using System;
using FMOD;

using Debug = UnityEngine.Debug;

public abstract class ActorActionHandler
{
    protected Actor _actor;

    public ActorActionHandler(Actor actor )
    {
        this._actor = actor;
    }

    public abstract bool HasActionsAvailable();
    public abstract void Tick( Game game );
    public abstract void SetupForPhase();
    public abstract bool ShouldForceEndActor();
    public abstract ActorAction ActiveAction { get; }
    public abstract void TurnEnded();

    public abstract List<ActorAction> GetActionOptions( ActionCategory category );
}

public enum SequencePos
{
    Unset,
    Start,
    Mid,
    End,
}

public class PlayerActionHandler : ActorActionHandler
{
    private MoveAction _moveAction = null;
    private ActorAction _activeAction = null;
    private List<ActorAction> _nonMoveActions = new List<ActorAction>();
    private bool _forceEndActor = false;
    private UIPickActionRequest _actionPickRequest = null;
    private UIPickWeaponRequest _weaponPickRequest = null;

    public override ActorAction ActiveAction => _activeAction;

    public PlayerActionHandler( Actor actor ) : base( actor )
    {

    }

    public override bool ShouldForceEndActor()
    {
        return _forceEndActor;
    }

    public override void SetupForPhase()
    {
        _forceEndActor = false;
        _activeAction = null;
        _moveAction = _actor.GetActionsOfType( ActionType.Move ).FirstOrDefault() as MoveAction;
    }


    private IEnumerable<ActorAction> GetUsableActions()
    {
        return _actor.Actions.Where( x => _actor.CanSpendStatistic( StatisticType.AbilityPoints, x.Cost ) && x.AllowedToExecute(_actor) == CanStartActionResult.Success);
    }

    private void TryRequestWeaponPick()
    {
        if( _actionPickRequest != null )
            return;
        if( _weaponPickRequest != null )
            return;
        if( !UIPickWeaponRequest.CanExecute( _actor ) )
            return;
        if( !Input.GetKeyDown( KeyCode.Q ) )
            return;

        _weaponPickRequest = new UIPickWeaponRequest( _actor,
        x =>
        {
            //AllowActionSelect = true;
            _weaponPickRequest = null;
        },
        y =>
        {
            //AllowActionSelect = true;
            _weaponPickRequest = null;
        },
        () =>
        {
            //AllowActionSelect = true;
            _weaponPickRequest = null;
        } );
        //AllowActionSelect = false;
        UIManager.Instance.RequestUI( _weaponPickRequest, false );
    }



    public override bool HasActionsAvailable()
    {
        var abilityPoints = _actor.GetStatistic( StatisticType.AbilityPoints );
        if( abilityPoints == null )
            return false;

        //Can use a non move ability?
        return GetUsableActions().Count() > 0;
    }

    private List<ActorAction> _sequence = null;
    private int _sequenceIndex;
    public override void Tick( Game game )
    {

        TryRequestWeaponPick();

        if( _activeAction != null && _activeAction.State() == ActorActionState.Finished )
        {
            //Restart?
            /*if( _activeAction.AllowedToExecute( _actor ) == CanStartActionResult.Success )
                StartAction( _activeAction, game );
            else*/
            if( _activeAction is PlayerEngageAction && _actor.Target != null )
            {
                _activeAction = null;
                TryPickAction( game );
                return;
            }
            _activeAction = null;
        }


        if( _sequence != null )
        {
            if( _activeAction == null )
            {
                if( _sequenceIndex < _sequence.Count )
                {
                    var nextAction = _sequence[_sequenceIndex];
                    if( nextAction is AttackAction attack )
                    {
                        SequencePos seqPos = SequencePos.Start;
                        if( _sequenceIndex > 0 && _sequenceIndex < _sequence.Count - 1 )
                            seqPos = SequencePos.Mid;
                        else if( _sequenceIndex == _sequence.Count - 1 )
                            seqPos = SequencePos.End;
                        attack.SequencePos = seqPos;
                    }
                    _sequenceIndex++;
                    StartAction( nextAction, game );
                }
                else
                {
                    _actor.Target = null;
                    _sequence = null;
                }
            }
        }

        //Need to check some sort of input to see if we want to perform an action.
        if( _actionPickRequest == null && Input.GetMouseButtonDown( 1 ) )
        {
            TryPickAction( game );
        }




        if( _actionPickRequest != null )
            return;

        if( _activeAction == null )
        {
            if( _moveAction.AllowedToExecute( _actor ) == CanStartActionResult.Success )
                StartAction( _moveAction, game );
        }
        else
        {
            _activeAction.Tick();
        }

        if( !HasActionsAvailable() )
            _forceEndActor = true;

    }


    private bool TryPickAction( Game game )
    {
        if( _weaponPickRequest != null )
            return false;
        //Only allow picking actions if there isn't a currently executing action
        if( _activeAction != null && _activeAction.State() == ActorActionState.Executing )
            return false;

        if( _activeAction != null && !_activeAction.AllowActionSelect )
            return false;

        if( _actionPickRequest != null )
            return false;

        _actionPickRequest = new UIPickActionRequest( _actor, 
        x =>
        {
            _activeAction?.End();
            
            //Player selected to end this actor turn.
            if( x is EndActorAction )
            {
                _forceEndActor = true;
                _actionPickRequest = null;
                return;
            }
            else if( x is List<ActorAction> sequence)
            {
                _activeAction?.End();
                _activeAction = null;
                _sequenceIndex = 0;
                this._sequence = sequence;
                _actionPickRequest = null;
                return;
            }

        /* !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         * TODO: SPLIT SEQUENCE SELECTION INTO IT'S OWN REQUEST!!
         * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! */
        //This is here for the case where engage has selected a target. This is ugly, and not clear.
        if( _actor.Target != null )
        {
            //Begin sequence mode in the request. Reset to running for this action.
            _actionPickRequest.SetStateRunning();
            _actionPickRequest.DoSequenceMode();
            return;
        }

        if( x is ActorAction singleAction )
        {
            StartAction( singleAction, game );
        }


        _actionPickRequest = null;
    },
    y =>
    {
        _actionPickRequest = null;
    },
    () =>
    {
        _activeAction?.End();
        //This is to clear any time there's a cancel... not terrible, but not clearly linked to being in a "Targeting state" where clearing makes sense.
        _actor.Target = null;
        _actionPickRequest = null;
    } );

    UIManager.Instance.RequestUI( _actionPickRequest, false );

    return true;
}


public bool StartAction( ActorAction action, Game game )
{
    if( action == null )
        return false;

    if( action.AllowedToExecute( _actor ) == CanStartActionResult.Success )
    {
        _activeAction = action;
        _activeAction.Start( game, _actor );
        return true;
    }
    return false;
}



public override void TurnEnded()
{
    _actionPickRequest = null;
    UIManager.Instance.TerminateActiveRequests();
    if( _activeAction != null )
        _activeAction.TurnEnded();
    _activeAction = null;
}

public override List<ActorAction> GetActionOptions( ActionCategory category )
{
    if( this._actor.Target != null )
    {
        ActionCategory forcedCategory = _actor.GetInteractionCategory( _actor.Target );
        return GetUsableActions().Where( x => x.Category == forcedCategory ).ToList();//.Where( x => x != _activeAction ).ToList();
    }

    return GetUsableActions().Where(x => x.Category == category ).ToList();//.Where( x => x != _activeAction ).ToList();
}

}


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



[System.Serializable]
public class Actor: SimpleEntity<ActorAsset>
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


public List<ActorAction> GetActionsOfType(ActionType type)
{
    return Actions.Where( x => x.ActionPhase == type ).ToList();
}


public List<ActorAction> GetActionsNotOfType( ActionType type )
{
    return Actions.Where( x =>x.ActionPhase != type ).ToList();
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
        stat.SetValue( Mathf.Max( 0,  stat.Value - amount ) );
        return true;
    }
}
