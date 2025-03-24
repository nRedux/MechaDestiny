using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityPopulator : TerrainPopulator
{

    public MapObjectReference MapObject;

    private MapObjectAsset _asset;


    public MapObjectAsset Asset
    {
        get
        {
            if( _asset == null )
            {
                if( !MapObject.RuntimeKeyIsValid() )
                    return null;
                _asset = MapObject.GetAssetSync();
            }
            return _asset;
        }
    }


    public MapObjectData MapObjectData
    {
        get
        {
            if( Asset == null )
                return null;
            return Asset.Opt()?.GetDataCopy();
        }
    }


    public override void ProcessSample( Vector3 position, MapData mapData )
    {
        LoadObject( position );
        mapData.AddMapdata( MapObjectData );
    }


    private async void LoadObject( Vector3 position )
    {
        if( Asset == null ) return;

        var data = MapObjectData;
        if( data == null ) return;

        var mapGfx = data.Graphics;
        if( !mapGfx.RuntimeKeyIsValid() ) return;

        var graphicsAsset = await mapGfx.GetAssetAsync();
        if( graphicsAsset == null ) return;


        var dupe = graphicsAsset.Duplicate( position );
        if( dupe == null )
            return;
        GfxMapObject mapObjInstance = dupe.GetComponent<GfxMapObject>();
        if( mapObjInstance == null )
        {
            Destroy( mapObjInstance.gameObject );
            return;
        }

        
        mapObjInstance.Initialize( data );
        data.Position = position;
        mapObjInstance.transform.position = position;
    }


}