using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class OnlyJsonPropertyContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties( System.Type type, MemberSerialization memberSerialization )
    {
        // Retrieve the default list of properties
        var properties = base.CreateProperties( type, memberSerialization );

        // For every property, check if its associated member has a [JsonProperty] attribute.
        // If so, force it to be included even if it has [NonSerialized].
        foreach( var property in properties )
        {
            // Try to get the underlying member using reflection.
            // Depending on your project and naming conventions, you might need
            // to adjust this if the property name doesn’t exactly match the member name.
            var member = type.GetMember( property.UnderlyingName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                             .FirstOrDefault();

            if( member != null )
            {
                // Check for the [JsonProperty] attribute.
                bool hasJsonProperty = member.GetCustomAttribute<JsonPropertyAttribute>() != null;
                if( hasJsonProperty )
                {
                    // Override the default behavior (which might mark it as ignored due to [NonSerialized])
                    property.Ignored = false;
                }
            }
        }

        return properties;
    }
}

public static class Json
{

    public static JsonSerializerSettings Settings = new JsonSerializerSettings()
    {
        TypeNameHandling = TypeNameHandling.Objects,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        ConstructorHandling = ConstructorHandling.Default,
        DefaultValueHandling = DefaultValueHandling.Include,
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Include,
        Converters = new List<JsonConverter>() { new AssetReferenceConverter(), new LocalizedStringConverter(), new ColorConverter() },
        ContractResolver = new OnlyJsonPropertyContractResolver(),
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
