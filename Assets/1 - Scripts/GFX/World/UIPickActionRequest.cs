using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using SuccessCallback = UIRequestSuccessCallback<object>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.EventSystems;

public class UIPickActionRequest : UIRequest<object, bool>
{
    private int frames = 0;
    private List<ActorAction> _actions;
    private Actor _requester;

    private bool _inSequenceMode = false;

    public UIPickActionRequest( object requester, SuccessCallback onSuccess, FailureCallback onFailure, CancelCallback onCancel ) : base( onSuccess, onFailure, onCancel, requester )
    {
        _requester = requester as Actor;
    }

    private bool _uiWantsFire = false;

    public void DoSequenceMode()
    {
        if( !_inSequenceMode )
            UIManager.Instance.ShowActionSequence( _requester, () => _uiWantsFire = true );
        _inSequenceMode = true;
    }

    public void SetActions( List<ActorAction> actions )
    {
        this._actions = actions;
    }


    public override void Cleanup()
    {
        UIManager.Instance.HideActionPicker();
    }


    public override void Run()
    {
        if( !EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown( 1 ) && frames > 2 )
            this.Cancel();

        if( _inSequenceMode && ( Input.GetKeyDown( KeyCode.F ) || _uiWantsFire ) )
        {
            Succeed( UIManager.Instance.ActionSequence.GetSelectedSequence() );
            UIManager.Instance.HideActionSequence();
        }

        frames++;
    }


    public override void Start()
    {
        base.Start();
        GetNextAction();
    }


    private void OnSelect( ActorAction selected )
    {
        if( !_inSequenceMode )
            Succeed( selected );

        UIManager.Instance.ActionSequence.AddItem( selected );
        //When do we return success on full sequence? (Should be based on other input?
        GetNextAction();
    }


    private bool GetNextAction()
    {
        //Check if we can actually pick any more actions.

        ActionCategory category = ActionCategory.Control; 

        if( _inSequenceMode )
            category = _requester.GetInteractionCategory( _requester.Target );

        UIManager.Instance.ShowActionPicker( OnSelect, category );
        return true;
    }


    protected override void OnCancelled()
    {
        UIManager.Instance.HideActionPicker();
        UIManager.Instance.HideActionSequence();
    }

}
