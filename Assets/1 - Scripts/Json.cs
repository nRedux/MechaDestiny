using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class Json
{

    private static JsonSerializerSettings _settings = new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        ConstructorHandling = ConstructorHandling.Default,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = new List<JsonConverter>() { new AssetReferenceConverter(), new LocalizedStringConverter(), new ColorConverter() }
    };

    public static string SerializeObject( object obj )
    {
        return JsonConvert.SerializeObject( obj, _settings );
    }

    public static T DeserializeObject<T>( string json )
    {
        return JsonConvert.DeserializeObject<T>( json, _settings );
    }

    public static T Clone<T>( object obj )
    {
        string json = SerializeObject( obj );
        return DeserializeObject<T>( json );
    }

}
