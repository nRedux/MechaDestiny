using UnityEngine;
using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

public class ColorConverter : JsonConverter
{
    public ColorConverter() : base()
    {

    }

    public override bool CanConvert( Type objectType )
    {
        return typeof( Color ).IsAssignableFrom( objectType );
    }

    public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
    {
        JObject obj = JObject.Load( reader );
        float r = (float) obj["r"];
        float g = (float) obj["g"];
        float b = (float) obj["b"];
        float a = (float) obj["a"];

        return new Color( r, g, b, a );
    }

    public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
    {
        var color = (Color) value;

        writer.WriteStartObject();
        writer.WritePropertyName( "r" );
        writer.WriteValue( color.r );
        writer.WritePropertyName( "g" );
        writer.WriteValue( color.g );
        writer.WritePropertyName( "b" );
        writer.WriteValue( color.b );
        writer.WritePropertyName( "a" );
        writer.WriteValue( color.a );
        writer.WriteEndObject();
    }
}
