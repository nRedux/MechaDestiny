using System.Collections.Generic;
using UnityEngine;

public class BattleInputAction
{
    private System.Action<BattleInputAction> _updateCallback;
    private bool _active;
    private bool _used;

    public bool Active { get => _active; }

    public bool Used { get => _used; }

    public BattleInputAction( System.Action<BattleInputAction> updateCallback )
    {
        if( updateCallback == null )
            throw new System.ArgumentNullException( "update callback cannot be null" );

        this._updateCallback = updateCallback;
    }

    public void Activate()
    {
        _active = true;
    }

    public void Use()
    {
        _used = true;
    }

    public void Update()
    {
        _updateCallback( this );
    }
}

public class BattleInput
{
    public BattleInputAction Cancel = null;

    private List<BattleInputAction> _actions = new List<BattleInputAction>();

    public BattleInput()
    {
        Cancel = new BattleInputAction( UpdateCancel );
    }

    public BattleInputAction NewAction( System.Action<BattleInputAction> update )
    {
        if( update == null )
            throw new System.ArgumentNullException( "update callback cannot be null" );

        var result = new BattleInputAction( update );
        _actions.Add( result );

        return result;
    }

    private void UpdateCancel( BattleInputAction cancel )
    {
        if( Input.GetMouseButtonDown( 1 ) )
            cancel.Activate();
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
