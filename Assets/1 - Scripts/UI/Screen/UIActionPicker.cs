using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class UIActionPicker : UISelector<ActorAction, UIActionPickerOption>
{
    
    private bool _pauseMoveAndAttackOverlays = false;
    public bool PauseMoveAttackReqsOptionEnter
    {
        get
        {
            return _pauseMoveAndAttackOverlays;
        }
        set
        {
            _pauseMoveAndAttackOverlays = value;
        }
    }

    private IRequestGroupHandle _pausedRequests;



    protected override void OptionCreated( UIActionPickerOption option )
    {
        option.PointerEnter += OnOptionEnter;
        option.PointerExit += OnOptionExit;
    }

    public void OnOptionEnter()
    {
        if( PauseMoveAttackReqsOptionEnter )
        {
            _pausedRequests = UIManager.Instance.PauseRequests( new System.Type[] { typeof( UIFindMoveTargetRequest ), typeof( UIFindAttackTargetRequest ) } );
        }
    }

    public void OnOptionExit()
    {
        if( _pausedRequests != null )
        {
            _pausedRequests.Undo();
            _pausedRequests = null;
        }
        
    }

}
