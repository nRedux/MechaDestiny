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

    private NavMeshPath _navPath;


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


    public void RunLuaBehavior( TextAsset asset )
    {
        //Dictionary<string, object> props = new Dictionary<string, object> { { "thisMapObject", this } };
        this.LuaBehavior = new LuaBehavior( asset );
    }


    private void CollectActions()
    {
        Actions = GetComponentsInChildren<GfxMapObjectAction>().ToList();
    }


    protected virtual void Awake()
    {
        CollectActions();
    }


    protected virtual void Update()
    {
       

        if( HasValidPath() )
        {
            Move( Data.Speed, TimeManager.Instance.DayData.HoursDelta );
        }
    }


    public void StopMoving()
    {
        //Path = null;
        ActionOnPathComplete = null;
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


    private void Move( float speed, float time )
    {
        if( !HasValidPath() )
            return;
        Vector3 lastPos = Data.Position;
        Vector3 heading = Data.Heading;
        Data.Position = Path.MoveAlong( Data.Position, speed, time, ref heading );
        Data.Heading = heading;

        if( Path.IsComplete() )
        {
            DoPathCompleteAction();
            //Path = null;
        }
    }


    private void DoPathCompleteAction()
    {
        if( string.IsNullOrEmpty( ActionOnPathComplete ) )
            return;

        PathCompleteCallback?.Invoke( ActionOnPathComplete, TargetOnComplete );

        ActionOnPathComplete = null;
        TargetOnComplete = null;
    }


    public bool SetPath( Vector3 destination, float desiredProximity, MapObjectData targetOnComplete, Type actionOnArrive )
    {
        _navPath = _navPath ?? new NavMeshPath();
        this.TargetOnComplete = targetOnComplete;
        this.ActionOnPathComplete = actionOnArrive?.Name;
        if( NavMesh.CalculatePath( transform.position, destination, NavMesh.AllAreas, _navPath ) )
        {
            Path = new TravelPath( _navPath.corners, desiredProximity );
            UpdateHeading();
            return true;
        }

        return false;
    }


    internal void UpdateHeading()
    {
        if( !HasValidPath() )
            return;
        Vector3 heading = Data.Heading;
        Path.MoveAlong( Data.Position, 1, .1f, ref heading );
        Data.Heading = heading;
    }


    public bool HasValidPath()
    {
        return Path != null && !Path.IsComplete();
    }


}
