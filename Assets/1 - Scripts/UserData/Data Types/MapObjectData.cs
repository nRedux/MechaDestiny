using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;



public class TravelPath
{

    private int _nextPoint = 1;
    private bool _pathComplete = false;
    private float _desiredProximity = .1f;
    public TravelPath() { }


    public TravelPath( Vector3[] points )
    {
        this.Points = points;
        this._desiredProximity = 0f;
    }


    public TravelPath( Vector3[] points, float desiredProximity )
    {
        this.Points = points;
        this._desiredProximity = desiredProximity;
    }

    public Vector3[] Points;

    public Vector3 MoveAlong( Vector3 position, float speed, float time, ref Vector3 heading )
    {
        if( _pathComplete )
            return position;

        Vector3 nextPoint = Points[_nextPoint];

        Vector3 toNext = nextPoint - position;
        Vector3 move = toNext.normalized * speed * time;

        float desiredMove = move.magnitude;
        
        Vector3 actualMove = Vector3.ClampMagnitude( move, toNext.magnitude );

        //Move toward new location
        position += actualMove;

        Vector3 end = this.Points[this.Points.Length - 1];
        Vector3 toEnd = end - position;
        if( toEnd.magnitude < _desiredProximity )
        {
            _pathComplete = true;
            return position;
        }

        //Reached end of path, or moving to next node?
        if( toNext.magnitude < .05f )
        {
            if( _nextPoint >= Points.Length - 1 )// Reached end.
            {
                _pathComplete = true;
                return position;
            }
            else//Increment point, move distance we couldn't move toward next point;
            {
                _nextPoint++;
            }
        }
        else
            heading = toNext.normalized;

        return position;
    } 

    public bool IsComplete()
    {
        return _pathComplete;
    }
}


[Flags]
public enum MapObjectType
{
    None = 0,
    Unit = 0x000001,
    Objective = 0x000002
}

[System.Serializable]
[JsonObject]
public class MapObjectData: DataObject<MapObjectAsset>
{
    public GameObjectReference Graphics;
    public Vector3 Position;
    public Vector3 Heading;

    public MapObjectType Type;

    public float Speed;

    [HideInInspector]
    public TravelPath Path;

    public MapObjectData TargetOnComplete = null;
    public string ActionOnPathComplete;

    public ScriptGraphAssetReference GraphOnInteract;
    public TextAssetReference ScriptOnInteract;

    private NavMeshPath _navPath;

    [NonSerialized]
    public System.Action<string, MapObjectData> PathCompleteCallback;

    public bool SetPath( Vector3 destination, float desiredProximity, MapObjectData targetOnComplete, Type actionOnArrive )
    {
        _navPath = _navPath ?? new NavMeshPath();
        this.TargetOnComplete = targetOnComplete;
        this.ActionOnPathComplete = actionOnArrive?.Name;
        if( NavMesh.CalculatePath( Position, destination, NavMesh.AllAreas, _navPath ) )
        {
            Path = new TravelPath( _navPath.corners, desiredProximity );
            UpdateHeading();
            return true;
        }

        return false;
    }

    public void AddToMap()
    {
        RunManager.Instance.MapData.Objects.Add( this );
    }
    
    public bool HasValidPath()
    {
        return Path != null && !Path.IsComplete();
    }

    public void StopMoving()
    {
        Path = null;
        ActionOnPathComplete = null;
    }

    private void Move( float speed, float time )
    {
        if( !HasValidPath() )
            return;
        Vector3 lastPos = Position;
        Position = Path.MoveAlong( Position, speed, time, ref Heading );

        if( Path.IsComplete() )
        {
            DoPathCompleteAction();
            Path = null;
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

    public void Tick( float time )
    {
        if( HasValidPath() )
        {
            Move( this.Speed, time );
        }
    }

    internal void UpdateHeading()
    {
        if( !HasValidPath() )
            return;
        Path.MoveAlong( Position, 1, .1f, ref Heading );
    }
}
