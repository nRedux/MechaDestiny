using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;



public class LuaBehavior
{
    public List<SpawnLocation> SpawnLocations;

    private Script _scriptObject = null;

    private object _luaUpdate;
    private object _luaStart;

    public string ScriptName
    {
        get; private set;
    }


    const string ACTOR_KEY = "thisActor";

    const string LUA_UPDATE_SIGNATURE = "update";
    const string LUA_START_SIGNATURE = "start";


    public LuaBehavior( TextAsset script, Dictionary<string, object> properties = null )
    {
        //Cache this to use in a second, but additionally gets LUA env set up at the right timing.
        var luaManager = LuaBehaviorManager.Instance;
        ScriptName = script.name;
        InitializeScript( script.text );
        if( properties != null )
            properties.Do( x => _scriptObject.Globals[x.Key] = x.Value );
        CacheLuaCalls();
        luaManager.RegisterBehavior( this );
        //CallStart();
    }

    public void InitializeScript( string script )
    {
        _scriptObject = new Script();
        SetupBindings();
        _scriptObject.DoString( script );

        /*
        Table table = new Table( _scriptObject );
        for( int i = 0; i < SpawnLocations.Count; i++ )
        {
            if( SpawnLocations[i] == null )
                continue;
            table[i] = SpawnLocations[i];
        }
        */
    }

    public void AssignActor( Actor actor )
    {
        _scriptObject.Globals[ACTOR_KEY] = actor;
    }

    private void CacheLuaCalls()
    {
        _luaUpdate = _scriptObject.Globals[LUA_UPDATE_SIGNATURE];
        _luaStart = _scriptObject.Globals[LUA_START_SIGNATURE];
    }

    private void SetupBindings()
    {
        _scriptObject.Globals[nameof( Log ).ToLower()] = (System.Action<string>) Log;
        _scriptObject.Globals[nameof( Error ).ToLower()] = (System.Action<string>) Error;
        _scriptObject.Globals[nameof( ActorStatus )] = typeof( ActorStatus );
    }

    public void CallUpdate()
    {
        MakeCall( _luaUpdate );
    }

    public void CallStart()
    {
        MakeCall( _luaStart );
    }

    private void MakeCall( object method )
    {
        if( method == null )
            return;
        try
        {
            _scriptObject.Call( method );
        }
        catch( ScriptRuntimeException ex )
        {
            Debug.LogError( $"Lua Error: {ex.Message}" );
            Debug.LogError( $"Line Number: {ex.DecoratedMessage}" );
        }

    }

    public void Log( string message )
    {
        Debug.Log( $"Msg({ScriptName}): {message}" );
    }

    public void Error( string message )
    {
        Debug.LogError( $"Error({ScriptName}): {message}" );
    }
}
