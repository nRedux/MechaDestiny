using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityPopulator : MonoBehaviour, ITerrainPointPopulator
{

    public MapObjectReference MapObject;

    private MapObjectAsset _asset;


    public MapObjectData GetMapObjectDataCopy()
    {
        if( !MapObject.RuntimeKeyIsValid() )
            return null;

        return MapObject.GetAssetSync().Opt()?.GetDataCopy();
    }


    public void PopulatePoint( TerrainSamplerPoint point, MapData mapData )
    {
        if( point.AlreadyUsed() )
            return;

        var instance = GetMapObjectDataCopy();
        point.Use();
        instance.Position = point.Position;
        mapData.AddMapdata( instance );
    }

}