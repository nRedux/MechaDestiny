using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GfxMapObject : MonoBehaviour
{

    public MapObjectData Data;

    public List<GfxMapObjectAction> Actions = new List<GfxMapObjectAction>();

    public GfxMapObjectAction SelectedAction = null;

    public void Initialize( MapObjectData data )
    {
        this.Data = data;
        this.Data.PathCompleteCallback += this.OnPathCompleted;
    }

    private void OnDestroy()
    {
        this.Data.PathCompleteCallback -= this.OnPathCompleted;
        this.Data = null;
    }

    private void OnPathCompleted( string actionType, MapObjectData target )
    {
        var action = this.Actions.Where( x => x.GetType().Name == actionType ).FirstOrDefault();
        action.Opt()?.ActionAfterPathMove( target );
    }

    public virtual void OnSelected() 
    {
        var selected = GetComponents<GfxObjectSelected>();
        selected.Do( x => x.OnSelected() );
    }

    public virtual void OnDeselected() 
    {
        var selected = GetComponents<GfxObjectSelected>();
        selected.Do( x => x.OnDeselected() );
    }

    public virtual MapObjectData GetData()
    {
        return Data;
    }

    private void CollectActions()
    {
        Actions = GetComponentsInChildren<GfxMapObjectAction>().ToList();
    }

    protected virtual void Awake()
    {
        CollectActions();
    }

    public void SelectAction<T>(System.Action onCancelled) where T: GfxMapObjectAction
    {
        var action = Actions.Where( x => x.GetType().IsAssignableFrom( typeof( T ) ) ).FirstOrDefault();
        if( action == null )
        {
            SelectedAction = null;
            return;
        }
        SelectedAction = action;
    }


}