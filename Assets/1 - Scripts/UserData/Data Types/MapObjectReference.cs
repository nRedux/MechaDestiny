using UnityEngine;

[System.Serializable]
public class MapObjectReference : DataProviderReference<MapObjectAsset, MapObjectData>
{
    public MapObjectReference( string guid ) : base( guid ) { }

}
