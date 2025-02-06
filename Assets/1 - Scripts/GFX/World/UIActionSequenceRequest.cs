using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

using SuccessCallback = UIRequestSuccessCallback<System.Collections.Generic.List<ActorAction>>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;
using System;

public class UIActionSequenceRequest : UIRequest<List<ActorAction>, bool>
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
        UIManager.Instance.ShowActionSequence( _requester, () => _uiWantsFire = true );
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

        UIManager.Instance.ActionSequence.AddItem( selected );
        //When do we return success on full sequence? (Should be based on other input?
        GetNextAction();
    }


    private bool GetNextAction()
    {
        //Check if we can actually pick any more actions.

        ActionCategory category = ActionCategory.Control;

        if( _requester.Target?.GfxActor == null )
        {
            category = ActionCategory.Attack;
        }
        else
        {
            //TODO: Actor.Target deciding how so many things function is not good. That property is too open to modification and failure to cleanup (setting null)
            category = _requester.GetInteractionCategory( _requester.Target?.GfxActor.Actor );
        }

        UIManager.Instance.ShowActionPicker( OnSelect, category );
        return true;
    }


    protected override void OnCancelled()
    {
        _requester.Target = null;
        UIManager.Instance.HideActionPicker();
        UIManager.Instance.HideActionSequence();
    }

}
