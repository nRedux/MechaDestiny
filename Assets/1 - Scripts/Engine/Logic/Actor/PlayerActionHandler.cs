using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;


public class SequenceAction
{
    public ActorAction Action;
    public SmartPoint Target;
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
        return _actor.Actions.Where( x => _actor.CanSpendStatistic( StatisticType.AbilityPoints, x.Cost ) && x.AllowedToExecute( _actor ) == CanStartActionResult.Success );
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

        if( _activeAction != null && _activeAction.State() == ActorActionState.Finished )
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
        }

        if( this._sequence == null )
            return;

        if( _activeAction == null )
        {
            if( _sequenceIndex < _sequence.Count )
            {
                var nextAction = _sequence[_sequenceIndex];
                _actor.Target = nextAction.Target;
                if( nextAction.Action is PlayerAttackAction attack )
                {

                    SequencePos seqPos = SequencePos.Start;
                    if( _sequenceIndex > 0 && _sequenceIndex < _sequence.Count - 1 )
                        seqPos = SequencePos.Mid;
                    else if( _sequenceIndex == _sequence.Count - 1 )
                        seqPos = SequencePos.End;
                    if( _sequence.Count == 1 )
                        seqPos = SequencePos.All;
                    attack.SequencePos = seqPos;

                    //If this kills the target, break out of the sequence so we don't fire on something dead.
                    attack.OnKillTarget = () =>
                    {
                        if( nextAction.Action is AttackAction endSeqNow )
                            endSeqNow.SequencePos = SequencePos.End;
                        _sequence = null;
                    };
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
        if( _activeAction != null && _activeAction.State() == ActorActionState.Executing )
            return false;

        if( _activeAction != null && !_activeAction.AllowActionSelect )
            return false;

        _pickingAction = true;
        UIManager.Instance.TryPickAction( _actor, ActionPickSuccess, ActionPickCancel );

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