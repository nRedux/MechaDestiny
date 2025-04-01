using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEditor.VersionControl;



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
    Event = 0x000002
}


public interface ILuaScriptEvent
{
    public void Execute();
}


[Serializable]
public class TextScript: ILuaScriptEvent
{
    public TextAssetReference ScriptAsset;
    public void Execute()
    {
        LuaBehavior oneShot = new LuaBehavior( ScriptAsset.GetAssetSync() );
    }
}

[Serializable]
public class ObjectScript : ILuaScriptEvent
{
    public GameObjectReference ScriptAsset;
    public void Execute()
    {
        if( !ScriptAsset.RuntimeKeyIsValid() )
        {
            Debug.LogError( $"Object script not set: variable {nameof(ScriptAsset)}" );
            return;
        }

        var asset = ScriptAsset.GetAssetSync();
        UnityEngine.Object.Instantiate( asset );
    }
}


[System.Serializable]
[JsonObject]
public class MapObjectData: DataObject<MapObjectAsset>, IMapEntityData
{
    public GameObjectReference Graphics;

    public Vector3 Position { get; set; }
    public Vector3 Heading { get; set; }

    public MapObjectType Type;

    public float Speed;

    [SerializeReference]
    public ILuaScriptEvent ScriptOnInteract;

    public MapEntityDisplayState DisplayState => throw new NotImplementedException();

    public MapEntityInteractivity Interactivity => throw new NotImplementedException();



    public async Task<GfxMapObject> LoadGraphics()
    {
        return await LoadGraphics( this.Position );
    }

    private async Task<GfxMapObject> LoadGraphics( Vector3 position )
    {
        var mapGfx = Graphics;
        if( !mapGfx.RuntimeKeyIsValid() ) return null;

        var graphicsAsset = await mapGfx.GetAssetAsync();
        if( graphicsAsset == null ) return null;


        var dupe = graphicsAsset.Duplicate( position );
        if( dupe == null )
            return null;
        GfxMapObject mapObjInstance = dupe.GetComponent<GfxMapObject>();
        if( mapObjInstance == null )
        {
            UnityEngine.Object.Destroy( mapObjInstance.gameObject );
            return null;
        }

        mapObjInstance.Initialize( this );
        Position = position;
        mapObjInstance.transform.position = position;

        return mapObjInstance;
    }
}
