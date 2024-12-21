using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using SuccessCallback = UIRequestSuccessCallback<bool>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;

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
        UIManager.Instance.HideWeaponPicker();
    }


    public override void Run()
    {
        if( Input.GetMouseButtonDown( 1 ) )
            this.Cancel();
    }


    public override void Start()
    {
        base.Start();
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
