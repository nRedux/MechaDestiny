using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class GfxMapObject : MonoBehaviour
{

    public MapObjectData Data;

    [HideInInspector]
    public TravelPath Path;

    public List<GfxMapObjectAction> Actions = new List<GfxMapObjectAction>();

    public GfxMapObjectAction SelectedAction = null;

    public LuaBehavior LuaBehavior { get; private set; }

    public MapObjectData TargetOnComplete = null;
    public string ActionOnPathComplete;

    [SerializeReference]
    public ILuaScriptEvent ScriptOnInteract;
    [NonSerialized]
    public System.Action<string, MapObjectData> PathCompleteCallback;

    


    public virtual void Initialize( IMapEntityData data )
    {
        this.Data = data as MapObjectData;
        this.PathCompleteCallback += this.OnPathCompleted;

        transform.position = this.Data.Position;
        transform.forward = this.Data.Heading;
    }


    private void OnDestroy()
    {
        PathCompleteCallback -= this.OnPathCompleted;
        Data = null;
    }



    protected virtual void Awake()
    {
        CollectActions();
    }


    protected virtual void Update()
    {

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


    public bool HasValidPath()
    {
        return Path != null && !Path.IsComplete();
    }


}
