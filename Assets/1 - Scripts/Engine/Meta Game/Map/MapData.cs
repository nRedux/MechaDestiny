using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;


[JsonObject]
[Serializable]
public class MapData
{
    public List<MapObjectData> Objects = new List<MapObjectData>();


    public IEnumerable<MapObjectData> GetObjectsOfType( MapObjectType type )
    {
        return Objects.Where( x => x.Type == type );
    }


}
