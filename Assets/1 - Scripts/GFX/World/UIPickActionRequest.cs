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
using System;

public class UIPickActionRequest : UIRequest<object, bool>
{
    private int frames = 0;
    private List<ActorAction> _actions;
    private Actor _requester;
    private ActionCategory _category;

    public UIPickActionRequest( object requester, SuccessCallback onSuccess, FailureCallback onFailure, CancelCallback onCancel, ActionCategory category ) : base( onSuccess, onFailure, onCancel, requester )
    {
        _requester = requester as Actor;
        _category = category;
    }

    private bool _uiWantsFire = false;


    public void SetActions( List<ActorAction> actions )
    {
        this._actions = actions;
    }


    public override void Cleanup()
    {
        ShutdownInput();
        UIManager.Instance.HideActionPicker();
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
        if( UIManager.Instance.IsPointerOverUI() )
            return;
        evt.Use();
        Cancel();
    }


    public override void Run()
    {
    }


    public override void Start()
    {
        base.Start();
        SetupInput();
        GetNextAction();
    }


    private void OnSelect( ActorAction selected )
    {
        Succeed( selected );
    }


    private bool GetNextAction()
    {
        //Check if we can actually pick any more actions.

        UIManager.Instance.ShowActionPicker( OnSelect, _category );
        return true;
    }

    protected override void OnCancelled()
    {
        UIManager.Instance.HideActionPicker();
        UIManager.Instance.HideActionSequence();
    }


    public override void OnPaused()
    {
        base.OnPaused();
        UIManager.Instance.ActionPicker.Hide();
    }

    public override void OnResumed()
    {
        base.OnResumed();
        UIManager.Instance.ActionPicker.Show();
    }

}
