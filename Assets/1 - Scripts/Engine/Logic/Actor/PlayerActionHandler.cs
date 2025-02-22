using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UI;


public class ActionSequence
{
    private const int INFINITE_COST = int.MaxValue;

    public System.Action<SequenceAction> ActionAdded;
    public System.Action<SequenceAction> ActionRemoved;

    private Actor _actor;
    private List<SequenceAction> _actions = new List<SequenceAction>();
    private int _availableActionPoints;

    private int FullSequenceCost => _actions?.Select( x => x.GetCost() ).Sum() ?? INFINITE_COST;
    private int AvailableActionPoints => _availableActionPoints - FullSequenceCost;

    public ActionSequence( Actor actor )
    {
        if( actor == null )
            throw new System.ArgumentNullException( $"{nameof( actor )} argument cannot be null." );
        _actor = actor;
        _availableActionPoints = actor.GetStatisticValue( StatisticType.AbilityPoints );
    }

    public void ClearListeners()
    {
        ActionAdded = null;
        ActionRemoved = null;
    }

    public bool AddAction( SequenceAction action )
    {
        if( action == null )
            return false;
        if( _actions.Contains( action ) )
            return false;

        _actions.Add( action );
        ActionAdded?.Invoke( action );
        return true;
    }

    public bool RemoveAction( SequenceAction action )
    {
        if( action == null )
            return false;
        if( _actions.Remove( action ) )
        {
            ActionRemoved?.Invoke( action );
            return true;
        }
        return false;
    }


#if USE_ACTION_BLOCKS
    private int GetBlockCostForAction( ActorAction action )
    {
        if( action == null )
            return 1;
        return Mathf.Max( action.BlocksUsed, 1 );
    }
#endif

    public bool CanAddSequenceAction( SequenceAction action )
    {
        if( action == null )
            return false;

        bool canAffordAP = AvailableActionPoints - action.GetCost() >= 0;

#if USE_ACTION_BLOCKS
        bool canAffordBlocks = _maxActionBlocks - ( BlocksUsed + GetBlockCostForAction( action.Action ) ) >= 0;
        return canAffordAP && canAffordBlocks;
#else
        return canAffordAP;
#endif
    }

}


public class SequenceAction
{
    public Actor Actor;
    public ActorAction Action;
    public SmartPoint Target;
    public MechComponentData UsedWeapon;

    public SequenceAction() { }

    public int GetCost()
    {
        int result = 1;
        if( Action == null )
            return 1;

        if( UsedWeapon != null )
        {
            result = Mathf.Max( 1, UsedWeapon.GetStatisticValue( StatisticType.AbilityPointCost ) );
            return result;
        }

        result = Mathf.Max( Action.APCost, 1 );
        return result;
    }
}


public class PlayerActionHandler : ActorActionHandler
{
    private MoveAction _moveAction = null;
    private ActorAction _activeAction = null;
    /// <summary>
    /// If true, we will end the turn for the current actor.
    /// </summary>
    private bool _forceEndActorTurn = false;
    private UIPickActionRequest _actionPickRequest = null;
    private List<SequenceAction> _sequence = null;
    private int _sequenceIndex = 0;
    private bool _firstBaseAction = true;

    public override ActorAction ActiveAction => _activeAction;


    public PlayerActionHandler( Actor actor ) : base( actor )
    {

    }


    public override bool ShouldEndActorTurn()
    {
        return _forceEndActorTurn;
    }


    public void SetupInput()
    {
        UIManager.Instance.UserControls.Cancel.AddActivateListener( OnCancelInput );
    }


    public void ShutdownInput()
    {
        UIManager.Instance.UserControls.Cancel.RemoveActivateListener( OnCancelInput );
    }


    private void OnCancelInput( InputActionEvent evt )
    {
        if( evt.Used )
            return;
        evt.Use();

        if( _canRightClickActionPick )
        {
            TryPickAction();
        }
    }


    public override void SetupForTurn()
    {
        _forceEndActorTurn = false;
        _activeAction = null;
        _moveAction = _actor.GetActionsOfType<MoveAction>().FirstOrDefault();
    }


    private IEnumerable<ActorAction> GetUsableActions()
    {
        return _actor.Actions.Where( x => _actor.CanSpendStatistic( StatisticType.AbilityPoints, x.APCost ) && x.AllowedToExecute( _actor ) == CanStartActionResult.Success );
    }


    public override bool HasActionsAvailable()
    {
        var abilityPoints = _actor.GetStatistic( StatisticType.AbilityPoints );
        if( abilityPoints == null )
            return false;

        //Can use a non move ability?
        return GetUsableActions().Count() > 0;
    }

    public override void Tick( Game game )
    {

        UIManager.Instance.TryPickWeapon( this._actor );

        if( _activeAction != null && _activeAction.State == ActorActionState.Finished )
            _activeAction = null;

        PerformActionSequence( game );


        if( _actionPickRequest != null )
            return;

        TickActiveOrSelectDefault( game );

        //Force the actor to end it's turn?
        if( !HasActionsAvailable() )
            _forceEndActorTurn = true;

    }


    private bool _canRightClickActionPick = false;

    /// <summary>
    /// If we have an active action, run it's tick. If not, try to select default action
    /// </summary>
    /// <param name="game">The game state instance</param>
    private void TickActiveOrSelectDefault( Game game )
    {
        if( _activeAction == null && !_pickingAction )
        {
            if( _moveAction.AllowedToExecute( _actor ) == CanStartActionResult.Success )
                ExecuteAction( _moveAction, game );

            //Selected default.
            //FORMALIZE DEFAULT ACTION.
            _canRightClickActionPick = true;

        }
        else if( _activeAction != null )
        {
            _activeAction.Tick();
        }
    }

    public void BeginDefaultActionState()
    {

    }


    MechComponentData PreviousWeapon( int index )
    {
        if( _sequence == null )
            return null;

        for( int i = index-1; i >= 0; i-- )
        {
            if( _sequence[i].Action is PlayerAttackAction attack )
            {
                return _sequence[i].UsedWeapon;
            }
        }

        return null;
    }

    MechComponentData NextWeapon( int index )
    {
        if( _sequence == null )
            return null;

        for( int i = index + 1; i < _sequence.Count; i++ )
        {
            if( _sequence[i].Action is PlayerAttackAction attack )
            {
                return _sequence[i].UsedWeapon;
            }
        }

        return null;
    }

    private void PerformActionSequence( Game game )
    {
        //Take sequence from actor, if one exists, and we're not executing one.
        if( this._sequence == null && this._actor.Sequence != null )
        {
            //Reset indexing
            this._sequenceIndex = 0;
            //Take
            this._sequence = this._actor.Sequence;
            //Clear from actor.
            this._actor.Sequence = null;

            //Nuke anything which was going on.
            _activeAction?.End();
            _activeAction = null;
            _sequenceIndex = 0;
            _firstBaseAction = true;
        }

        if( this._sequence == null )
            return;

        if( _activeAction == null )
        {
            if( _sequenceIndex < _sequence.Count )
            {
                var nextAction = _sequence[_sequenceIndex];

                _actor.Target = nextAction.Target;

                //Determine if in sequence
                //Determine position within sequence

                if( nextAction.Action is PlayerAttackAction attack )
                {
                    MechComponentData thisWeapon = nextAction.UsedWeapon;
                    MechComponentData prevWeapon = PreviousWeapon( _sequenceIndex );
                    MechComponentData nextWeapon = NextWeapon( _sequenceIndex );

                    SequencePos seqPos = SequencePos.Start;
                    ResultDisplayProps displayProps = new ResultDisplayProps();

                    if( nextWeapon != thisWeapon )
                    {
                        displayProps.DoArmEnd = true;
                    }
                    if( prevWeapon != thisWeapon )
                    {
                        displayProps.DoArmStart = true;
                    }
                    if( prevWeapon == null )
                    {
                        displayProps.IsSequenceStart = true;
                    }
                    if( nextWeapon == null )
                        displayProps.IsSequenceEnd = true;


                    attack.DisplayProps = displayProps;
                    attack.SequencePos = seqPos;

                    //If this kills the target, break out of the sequence so we don't fire on something dead.
                    attack.OnKillTarget = () =>
                    {
                        if( nextAction.Action is AttackAction endSeqNow )
                        {
                            //We want to end arm activity even if it was previously not desired.
                            //displayProps.DoArmStuff = true;
                        }
                    };


                    //Give attack info about this action in the sequence.
                    attack.Target = nextAction.Target;
                    attack.UsedWeapon = nextAction.UsedWeapon;

                    Debug.Log( "DP: " +  attack.DisplayProps.ToString() );
                }

                _sequenceIndex++;

                _canRightClickActionPick = false;
                 ExecuteAction( nextAction.Action, game );
            }
            else
            {
                _sequence = null;
            }
        }

        return;
    }


    public void ActionPickSuccess( object x )
    {
        //Set to default state if picked was the default (move) action. Otherwise, not default state.
        _canRightClickActionPick = x == _moveAction;

        Game game = GameEngine.Instance.Game;
        _activeAction?.End();
        _pickingAction = false;
        //Player selected to end this actor turn.
        if( x is EndActorAction )
        {
            _forceEndActorTurn = true;
            return;
        }
        else if( x is List<SequenceAction> sequence )
        {
            this._sequence = sequence;
            return;
        }

        if( x is ActorAction singleAction )
        {
            _activeAction = singleAction;
            ExecuteAction( singleAction, game );
        }
    }

    public bool _pickingAction = false;

    public void ActionPickCancel( )
    {
        _pickingAction = false;
        //This is to kill the active move action which is active....
        _activeAction?.End();
        //This is to clear any time there's a cancel... not terrible, but not clearly linked to being in a "Targeting state" where clearing makes sense.
    }

    private bool TryPickAction( )
    {
        //Only allow picking actions if there isn't a currently executing action
        if( _activeAction != null && _activeAction.State == ActorActionState.Executing )
            return false;

        if( _activeAction != null && !_activeAction.AllowActionSelect )
            return false;

        _pickingAction = true;
        UIManager.Instance.TryPickAction( _actor, ActionPickSuccess, ActionPickCancel, ActionCategory.Control );

        return true;
    }


    public bool ExecuteAction( ActorAction action, Game game )
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
        //Might be a sign of a problem
        _actionPickRequest = null;
        UIManager.Instance.TerminateActiveRequests();
        if( _activeAction != null )
            _activeAction.TurnEnded();
        _activeAction = null;
    }


    public override List<ActorAction> GetActionOptions( ActionCategory category )
    {
        return GetUsableActions().Where( x => x.Category == category ).ToList();//.Where( x => x != _activeAction ).ToList();
    }


    public override void Selected()
    {
        _canRightClickActionPick = false;
        SetupInput();
    }


    public override void Deselected()
    {
        _canRightClickActionPick = false;
        ShutdownInput();
    }

}