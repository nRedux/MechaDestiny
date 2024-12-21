using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;
using System;

public class AssetReferenceConverter : JsonConverter
{
    public AssetReferenceConverter() : base()
    {

    }

    public override bool CanConvert( Type objectType )
    {
        return typeof( AssetReference ).IsAssignableFrom( objectType );
    }

    public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
    {
        string assetGuid = (string) reader.Value;

        return Activator.CreateInstance( objectType, assetGuid );
    }

    public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
    {
        var assetRef = value as AssetReference;
        writer.WriteValue( assetRef.AssetGUID );
    }
}
