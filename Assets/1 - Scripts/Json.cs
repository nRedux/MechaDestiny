using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class Json
{

    public static JsonSerializerSettings Settings = new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        ConstructorHandling = ConstructorHandling.Default,
        DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
        Converters = new List<JsonConverter>() { new AssetReferenceConverter(), new LocalizedStringConverter(), new ColorConverter() },

        //ObjectCreationHandling = ObjectCreationHandling.Replace,
        Error = delegate ( object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args )
        {
            var origObj = args.ErrorContext.OriginalObject;
            var member = args.ErrorContext.Member;
            string origObjMessage = origObj != null ? origObj.ToString() + " " + origObj.GetType().Name : "No Value";
            string memberMessage = member != null ? member.ToString() + " " + member.GetType().Name : "No Value";
            Debug.LogError( $"{args.ErrorContext.Error.Message} ----  {args.ErrorContext.Path} --- {origObjMessage} --- {memberMessage} :: Stack Trace Below!" );
            Debug.LogError( args.ErrorContext.Error.StackTrace );
            //args.ErrorContext.Handled = true;
        },
    };

    public static string SerializeObject( object obj )
    {
        return JsonConvert.SerializeObject( obj, Settings );
    }

    public static T DeserializeObject<T>( string json )
    {
        return JsonConvert.DeserializeObject<T>( json, Settings );
    }

    public static T Clone<T>( object obj )
    {
        string json = SerializeObject( obj );
        return DeserializeObject<T>( json );
    }

}
