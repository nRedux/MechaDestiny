using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputActionEvent
{

    private bool _used;

    public object UserData
    {
        get; private set;
    }

    public InputActionEvent( object data )
    {
        _used = false;
        UserData = data;
    }

    public bool Used
    {
        get => _used;
    }

    public void Use()
    {
        _used = true;
    }
}


public delegate void InputActionEventDelegate( InputActionEvent evt );
public delegate void InputActionDelegate( InputAction evt );


public class InputAction
{
    private InputActionDelegate _updateCallback;
    private bool _active;

    private InputActionEvent _event;

    public bool Active { get => _active; }

    public bool Used { get => _event.Used; }

    private InputActionEventDelegate _onActivate;

    public void AddActivateListener( InputActionEventDelegate callback )
    {
        if( _onActivate == callback )
            return;

        //We want things added later to be called first.
        //Cache the previous delegate and all of it's calls.
        var prev = _onActivate;
        //Set the new callback as the base (front of the list)
        _onActivate = callback;
        //Add everything previously added back in.
        _onActivate += prev;
    }

    public void RemoveActivateListener( InputActionEventDelegate callback )
    {
        _onActivate -= callback;
    }

    public InputAction( InputActionDelegate updateCallback )
    {
        if( updateCallback == null )
            throw new System.ArgumentNullException( "update callback cannot be null" );

        this._updateCallback = updateCallback;
    }

    public void Activate( object data = null )
    {
        _active = true;
        _event = new InputActionEvent( data );
        _onActivate?.Invoke( _event );
    }

    public void Update()
    {
        _updateCallback( this );
    }
}

public class InputActions
{
    public InputAction Interact = null;
    public InputAction Cancel = null;

    private List<InputAction> _actions = new List<InputAction>();

    public InputActions()
    {
        Cancel = NewAction( UpdateCancel );
        Interact = NewAction( UpdateInteract );
    }

   
    public InputAction NewAction( InputActionDelegate update )
    {
        if( update == null )
            throw new System.ArgumentNullException( "update callback cannot be null" );

        var result = new InputAction( update );
        _actions.Add( result );

        return result;
    }


    private void UpdateCancel( InputAction cancel )
    {
        if( Input.GetMouseButtonDown( 1 ) && !UIManager.Instance.IsPointerOverUI() )
            cancel.Activate();
    }


    private void UpdateInteract( InputAction interact )
    {
        if( Input.GetMouseButtonDown( 0 ) )
            interact.Activate();
    }


    public void Update()
    {
        for( int x = 0; x < _actions.Count; x++ )
        {
            if( _actions[x] == null )
                continue;
            _actions[x].Update();
        }
    }
}
