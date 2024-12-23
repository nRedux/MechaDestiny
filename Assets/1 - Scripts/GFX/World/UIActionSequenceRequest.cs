using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

using SuccessCallback = UIRequestSuccessCallback<System.Collections.Generic.List<ActorAction>>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;

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

    public override void Cleanup()
    {
        UIManager.Instance.HideActionPicker();
        UIManager.Instance.HideActionSequence();
    }


    public override void Run()
    {
        //I don't know why we have the 2 frame delay here.. was right click cancelling and selecting?  I think we were right clicking to initiate
        //this action and the right click immediately cancelled it too. Input conflict. Should change input, or should have input usage handling like
        //"InputEvent" and be able to call myInputEvent.Used() to make it not used by anything else. This would have scheduling concerns though.
        if( !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown( 1 ) && frames > 2 )
            this.Cancel();

        if( Input.GetKeyDown( KeyCode.F ) || _uiWantsFire )
        {
            Succeed( UIManager.Instance.ActionSequence.GetSelectedSequence() );
        }

        frames++;
    }


    public override void Start()
    {
        base.Start();
        UIManager.Instance.ShowActionSequence( _requester, () => _uiWantsFire = true );
        GetNextAction();
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

        //TODO: Actor.Target deciding how so many things function is not good. That property is too open to modification and failure to cleanup (setting null)
        category = _requester.GetInteractionCategory( _requester.Target );

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
