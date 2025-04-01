using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public interface IMapEntityData
{
    void LoadGraphics();
    MapEntityDisplayState DisplayState { get; }
    MapEntityInteractivity Interactivity { get; }
}

//Visibility
public enum MapEntityDisplayState
{
    NotDiscovered,
    Discovered,
    Visited
}

public enum MapEntityInteractivity
{
    Interactable,
    Complete
}


[JsonObject]
[Serializable]
public class MapData
{
    /// <summary>
    /// The scene which this map data is associated with
    /// </summary>
    [JsonProperty]
    private string _scene = "Scene not set";


    /// <summary>
    /// Do we need to do initialization for this data to be properly set up?
    /// </summary>
    [JsonProperty]
    private bool _needsInitialization = true;


    /// <summary>
    /// Counter which is used as map entity data IDs
    /// </summary>
    [JsonProperty]
    private int _objectIDGenerator = 0;

    
    [JsonProperty]
    private Dictionary<int, IMapEntityData> Objects = new Dictionary<int, IMapEntityData>();


    [JsonProperty]
    private MapObjectData InteractedMapObject;

    public string Scene
    {
        get => _scene;
    }


    public MapData( string scene )
    {
        _needsInitialization = true;
        _scene = scene;
        _objectIDGenerator = 0;
    }

    public void Initialize()
    {
        if( _needsInitialization )
        {
            Events.Instance.Raise( new DoSceneWarmup() );
        }
        _needsInitialization = false;
    }

    public void LoadGraphics()
    {
        Objects.Do( x => x.Value.LoadGraphics() );
    }

    public void AddMapdata( IMapEntityData data )
    {
        if( data == null )
            return;

        Objects.Add( _objectIDGenerator++, data );
    }

}
