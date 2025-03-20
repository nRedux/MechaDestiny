using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;


public static class DataHandler
{

    public static System.Action<RunData> OnDataCreated;

    private static JsonSerializerSettings _defaultSerializerSettings;

    private static UserFileData _fileData;
    private static RunData _runData;

    private static IFileLoader _fileLoader = new LocalFileLoader();
    private static IFileWriter _fileWriter = new LocalFileWriter_v1();


    public static RunData RunData 
    {
        get
        {
            if( _runData == null )
            {
                LoadData();
                if( _runData == null )
                    _runData = CreateNewData();
            }
            return _runData;
        }
        private set
        {
            _runData = value;
        }
    }


    public static void Clear()
    {
        _runData = null;
    }


    public static JsonSerializerSettings SerializationSettings
    {
        get
        {
            if( _defaultSerializerSettings == null )
            {
                _defaultSerializerSettings = Json.Settings;
            }
            return _defaultSerializerSettings;
        }
    }


    private static void LoadData()
    {
        string key = typeof( RunData ).Name;

        _fileData = _fileLoader.Load();

        if( _fileData == null )
            return;

        if( string.IsNullOrEmpty( _fileData.RunDataJson ) )
            return;

        _runData = Json.DeserializeObject<RunData>( _fileData.RunDataJson );

        if( _runData != null )
            Debug.Log("Rundata loaded successfully.");
    }


    public static void SaveData( )
    {
        if( RunData == null )
            return;

        _fileWriter.WriteFile( null, RunData );
    }


    public static RunData CreateNewData()
    {
        Debug.Log("Creating new Rundata");
        var result = new RunData();
        OnDataCreated?.Invoke( result );
        return result;
    }

}
