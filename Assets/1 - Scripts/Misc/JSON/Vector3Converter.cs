using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;
using System;

public class Vector3Converter : JsonConverter
{
    public Vector3Converter() : base()
    {

    }

    public override bool CanConvert( Type objectType )
    {
        return typeof( Vector3 ).IsAssignableFrom( objectType );
    }

    public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
    {
        float x = (float) reader.Value;
        float y = (float) reader.Value;
        float z = (float) reader.Value;

        return new Vector3(x, y, z);
    }

    public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
    {
        var vector = (Vector3) value;
        writer.WriteValue( vector.x );
        writer.WriteValue( vector.y );
        writer.WriteValue( vector.z );
    }
}
