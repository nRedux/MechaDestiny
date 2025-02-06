using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using SuccessCallback = UIRequestSuccessCallback<bool>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;
using System;

public class UIPickWeaponRequest : UIRequest<bool, bool>
{

    private List<ActorAction> _actions;
    private Actor _actor;

    public UIPickWeaponRequest( Actor requester, SuccessCallback onSuccess, FailureCallback onFailure, CancelCallback onCancel): base( onSuccess, onFailure, onCancel, requester )
    {
        _actor = requester;
    }

    public void SetActions( List<ActorAction> actions )
    {
        this._actions = actions;
    }


    public override void Cleanup()
    {
        ShutdownInput();
        UIManager.Instance.HideWeaponPicker();
    }


    public override void Run()
    {
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
        Cancel();
    }


    public override void Start()
    {
        base.Start();
        SetupInput();
        UIManager.Instance.ShowWeaponPicker( () =>
        {
            UIManager.Instance.HideWeaponPicker();
            Succeed( true );
        } );
    }


    protected override void OnCancelled()
    {
        UIManager.Instance.HideWeaponPicker();
    }

    public static bool CanExecute( Actor actor )
    {
        return actor.HasUsableWeapons();
    }

}
