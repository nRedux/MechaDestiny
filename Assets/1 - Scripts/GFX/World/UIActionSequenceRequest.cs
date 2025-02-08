using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

using SuccessCallback = UIRequestSuccessCallback<System.Collections.Generic.List<SequenceAction>>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;
using System;

public class UIActionSequenceRequest : UIRequest<List<SequenceAction>, bool>
{
    private int frames = 0;
    private List<ActorAction> _actions;
    private Actor _requester;

    public UIActionSequenceRequest( object requester, SuccessCallback onSuccess, FailureCallback onFailure, CancelCallback onCancel ) : base( onSuccess, onFailure, onCancel, requester )
    {
        _requester = requester as Actor;
    }

    private bool _uiWantsFire = false;


    public override void Start()
    {
        base.Start();
        UIManager.Instance.ShowSequenceSelector( _requester, () => _uiWantsFire = true );
        SetupInput();
        GetNextAction();
    }


    private void SetupInput()
    {
        UIManager.Instance.UserControls.Cancel.AddActivateListener( OnCancelInput );
    }


    private void ShutdownInput()
    {
        UIManager.Instance.UserControls.Cancel.RemoveActivateListener( OnCancelInput );
    }

    private void OnCancelInput( InputActionEvent evt )
    {
        if( evt.Used )
            return;
        evt.Use();
        this.Cancel();
    }


    public override void Cleanup()
    {
        ShutdownInput();
        UIManager.Instance.HideActionPicker();
        UIManager.Instance.HideActionSequence();
    }


    public override void Run()
    {

        if( Input.GetKeyDown( KeyCode.F ) || _uiWantsFire )
        {
            Succeed( UIManager.Instance.ActionSequence.GetSelectedSequence() );
        }

        frames++;
    }


    private void OnSelect( ActorAction selected )
    {
        //Succeed( selected );
        _state = UIRequestState.Running;
        UIManager.Instance.ActionSequence.AddItem( new SequenceAction() { Action = selected } );
        GetNextAction();
    }


    private bool GetNextAction()
    {
        //Check if we can actually pick any more actions.

        ActionCategory category = ActionCategory.Attack;
        UIManager.Instance.ShowActionPicker( OnSelect, category );
        return true;
    }


    protected override void OnCancelled()
    {
        UIManager.Instance.HideActionPicker();
        UIManager.Instance.HideActionSequence();
    }

}
