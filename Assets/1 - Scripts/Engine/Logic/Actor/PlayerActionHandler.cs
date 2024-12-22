using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
        return _actor.Actions.Where( x => _actor.CanSpendStatistic( StatisticType.AbilityPoints, x.Cost ) && x.AllowedToExecute( _actor ) == CanStartActionResult.Success );
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
            else if( x is List<ActorAction> sequence )
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

        return GetUsableActions().Where( x => x.Category == category ).ToList();//.Where( x => x != _activeAction ).ToList();
    }

}