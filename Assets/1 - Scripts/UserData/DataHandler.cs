using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class DataHandler<TData> where TData : class, new ()
{

    private static JsonSerializerSettings _defaultSerializerSettings;

    public static TData Data { get; private set; }

    static DataHandler()
    {
        Data = LoadData();
        if( Data == null )
            Data = CreateNewData();
    }


    public static JsonSerializerSettings SerializationSettings
    {
        get
        {
            if( _defaultSerializerSettings == null )
            {
                _defaultSerializerSettings = GetNewSerializerSettings();
            }
            return _defaultSerializerSettings;
        }
    }


    public static JsonSerializerSettings GetNewSerializerSettings()
    {
        JsonSerializerSettings set = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            MissingMemberHandling = MissingMemberHandling.Error,
            StringEscapeHandling = StringEscapeHandling.Default,
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

        return set;
    }


    private static TData LoadData()
    {
        string key = typeof( TData ).Name;
        string json = PlayerPrefs.GetString( key, string.Empty );
        if( string.IsNullOrEmpty( json ) )
            return null;

        var result = JsonConvert.DeserializeObject<TData>( json, SerializationSettings );
        return result;
    }


    public static TData CreateNewData()
    {
        return new TData();
    }


    public static void SerializeData( )
    {
        if( Data == null )
            return;
        string key = typeof( TData ).Name;
        var json = JsonConvert.SerializeObject( Data, SerializationSettings );
        PlayerPrefs.SetString( key, json );
    }


    public static void SaveData()
    {
        PlayerPrefs.Save();
    }

}
