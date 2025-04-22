using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.UIElements;

public class EntitySeriesPopulator : TerrainPopulator
{

    public MapObjectReference[] MapObjects;

    private MapObjectAsset[] _assets;

    private int _nextIndex = 0;


    /// <summary>
    /// Stores a copy of the entries in MapObjects which are valid addressable references.
    /// </summary>
    private MapObjectReference[] _validMapObjectRefs = null;

    private IEnumerator<MapObjectReference> _mapObjEnumerator;


    /// <summary>
    /// Cache valid entries in MapObjects for use during population
    /// </summary>
    private void InitializeEnumerator()
    {
        if( _mapObjEnumerator != null && _mapObjEnumerator.Current != null )
            return;
        _mapObjEnumerator = MapObjects.Where( x => x.RuntimeKeyIsValid() ).GetEnumerator();
    }


    /// <summary>
    /// Get an asset from our map object references to use for population
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MapObjectAsset GetAssetAtIndex( int index )
    {
        InitializeEnumerator();

        if( _assets == null )
            _assets = new MapObjectAsset[_validMapObjectRefs.Length];

        //Load the asset if needed
        if( _assets[index] == null )
        {
            if( !_validMapObjectRefs[index].RuntimeKeyIsValid() )
                return null;
            _assets[index] = _validMapObjectRefs[index].GetAssetSync();
        }
        return _assets[index];
    }


    public MapObjectAsset GetNextMapObject()
    {
        InitializeEnumerator();
        var current = _mapObjEnumerator.Current;
        return current.GetAssetSync();
    }


    public MapObjectData GetMapObjectdata()
    {
        var asset = GetNextMapObject();
        if( asset == null )
            return null;
        return asset.Opt()?.GetDataCopy();
    }


    public override void ProcessSample( Vector3 position, MapData mapData )
    {
        var instance = GetMapObjectdata();

        if( instance == null )
        instance.Position = position;
        mapData.AddMapdata( instance );
    }


}