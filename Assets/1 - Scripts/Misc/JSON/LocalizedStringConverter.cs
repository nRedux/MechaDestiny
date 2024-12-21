using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class LocalizedStringConverter : JsonConverter
{
    public LocalizedStringConverter() : base()
    {

    }


    public override bool CanConvert( Type objectType )
    {
        return typeof( LocalizedString ).IsAssignableFrom( objectType );
    }


    public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
    {
        JObject obj = JObject.Load( reader );
        string table = (string) obj["Table"];
        string entry = (string) obj["Entry"];
        return new LocalizedString( table, entry );
    }


    public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
    {
        var locString = value as LocalizedString;

        string tableName = "Invalid";
        string entryItem = "Invalid";

        try
        {
            TableReference tableReference = locString.TableReference;
            TableEntryReference tableEntryReference = locString.TableEntryReference;
            var stringTable = LocalizationSettings.StringDatabase.GetTable( tableReference );
            tableName = stringTable?.name ?? "invalid";
            var entry = stringTable?.GetEntry( tableEntryReference.KeyId );
            entryItem = entry?.Key ?? "invalid";
        }
        catch
        {

        }

        writer.WriteStartObject();
        writer.WritePropertyName( "Table" );
        writer.WriteValue( locString.TableReference.TableCollectionName );
        writer.WritePropertyName( "Entry" );
        writer.WriteValue( entryItem );
        writer.WriteEndObject();
    }
}
