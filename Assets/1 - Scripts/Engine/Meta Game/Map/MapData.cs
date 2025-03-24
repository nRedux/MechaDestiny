using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public interface IMapEntityData
{

}


[JsonObject]
[Serializable]
public class MapData
{
    /// <summary>
    /// The scene which this map data is associated with
    /// </summary>
    private string _scene = "Scene not set";

    /// <summary>
    /// Do we need to do initialization for this data to be properly set up?
    /// </summary>
    private bool _needsInitialization = true;

    /// <summary>
    /// Counter which is used as map entity data IDs
    /// </summary>
    private int _objectIDGenerator = 0;

    private Dictionary<int, IMapEntityData> Objects = new Dictionary<int, IMapEntityData>();


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

    private void Initialize()
    {
        if( _needsInitialization )
        {
            Events.Instance.Raise( new DoSceneWarmup() );
        }
        _needsInitialization = false;
    }

    public void AddMapdata( IMapEntityData data )
    {
        if( data == null )
            return;

        Objects.Add( _objectIDGenerator++, data );
    }

}
