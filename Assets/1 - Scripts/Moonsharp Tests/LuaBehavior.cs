using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

public class LuaBehaviorException: System.Exception
{
    public LuaBehaviorException() : base() { }
    public LuaBehaviorException( string message) : base(message) { }
    public LuaBehaviorException( string message, System.Exception inner ) : base(message, inner) { }
}

public class LuaBehavior
{

    private const string LUA_ONTURNCHANGE_SIGNATURE = "onTurnChange";
    private const string LUA_UPDATE_SIGNATURE = "update";
    private const string LUA_START_SIGNATURE = "start";
    private const string LUA_AWAKE_SIGNATURE = "awake";
    private const string LUA_COMBATSTART_SIGNATURE = "combatStart";
    private const string LUA_TEAMSCREATED_SIGNATURE = "teamsCreated";
    private const string LUA_THIS_SCRIPT = "thisScript";

    private const string ACTOR_KEY = "thisActor";


    private Script _scriptObject = null;
    private Dictionary<string, object> _methods = new Dictionary<string, object>();
    private List<DynValue> _coroutines = new List<DynValue>();
    private List<DynValue> _deadCoroutines = new List<DynValue>();

    public string ScriptName
    {
        get; private set;

    }


    public LuaBehavior( TextAsset script, Dictionary<string, object> properties = null )
    {
        //Cache this to use in a second, but additionally gets LUA env set up at the right timing.
        var luaManager = LuaBehaviorManager.Instance;
        ScriptName = script.name;
        InitializeScript( script.text, properties );
        
        CacheLuaCalls();
        luaManager.RegisterBehavior( this );
        CallAwake();
    }


    public void SetProperty(string key , object value )
    {
        _scriptObject.Globals[key] = value;
    }


    public void InitializeScript( string script, Dictionary<string, object> properties = null )
    {
        _scriptObject = new Script();
        if( properties == null )
            properties = new Dictionary<string, object>();
        if( properties.ContainsKey( LUA_THIS_SCRIPT ) )
            throw new LuaBehaviorException( $"Invalid script property {LUA_THIS_SCRIPT} in property dictionary" );
        properties.Add( LUA_THIS_SCRIPT, this );
        properties.Do( x => _scriptObject.Globals[x.Key] = x.Value );


        SetupBindings();

        try
        {
            _scriptObject.DoString( script );
        }
        catch( SyntaxErrorException synEx)
        {
            Debug.LogError( $"Lua Error: {synEx.Message}" );
            Debug.LogError( $"Line Number: {synEx.DecoratedMessage}" );
        }
        catch( ScriptRuntimeException ex )
        {
            Debug.LogError( $"Lua Error: {ex.Message}" );
            Debug.LogError( $"Line Number: {ex.DecoratedMessage}" );
        }
    }


    public void AssignActor( Actor actor )
    {
        _scriptObject.Globals[ACTOR_KEY] = actor;
    }


    private void CacheMethod( string method )
    {
        var handle = _scriptObject.Globals[method];
        if( handle == null ) return;
        _methods.Add( method, handle );
    }


    private void CacheLuaCalls()
    {
        CacheMethod( LUA_UPDATE_SIGNATURE );
        CacheMethod( LUA_AWAKE_SIGNATURE );
        CacheMethod( LUA_START_SIGNATURE );
        CacheMethod( LUA_ONTURNCHANGE_SIGNATURE );
        CacheMethod( LUA_COMBATSTART_SIGNATURE );
        CacheMethod( LUA_TEAMSCREATED_SIGNATURE );
    }


    private void SetupBindings()
    {
        _scriptObject.Globals[nameof( Log ).ToLower()] = (System.Action<string>) Log;
        _scriptObject.Globals[nameof( Error ).ToLower()] = (System.Action<string>) Error;
        _scriptObject.Globals[nameof( GetKeyDown ) ] = (System.Func<KeyCode, bool>) GetKeyDown;
        
        _scriptObject.Globals[nameof( ActorStatus )] = typeof( ActorStatus );
        _scriptObject.Globals[nameof( KeyCode )] = typeof( KeyCode );
        _scriptObject.Globals[nameof( UnityEngine.Input )] = typeof( UnityEngine.Input );
        _scriptObject.Globals[nameof( TimeManager )] = typeof( TimeManager );
        _scriptObject.Globals[nameof( MetaGame )] = typeof( MetaGame );
    }


    public void CallUpdate()
    {
        MakeCall( LUA_UPDATE_SIGNATURE );
    }


    public void CallStart()
    {
        MakeCall( LUA_START_SIGNATURE );
    }


    public void CallAwake()
    {
        MakeCall( LUA_AWAKE_SIGNATURE );
    }


    public void CallTeamsCreated()
    {
        MakeCall( LUA_TEAMSCREATED_SIGNATURE );
    }

    public void CallCombatStart()
    {
        MakeCall( LUA_COMBATSTART_SIGNATURE );
    }
    
    public void CallTurnChanged( int turn )
    {
        MakeCall( LUA_ONTURNCHANGE_SIGNATURE, turn );
    }


    private void MakeCall( string method )
    {
        object handle = null;
        if( _methods.TryGetValue( method, out handle ) )
        {
            try
            {
                _scriptObject.Call( handle );
            }
            catch( ScriptRuntimeException ex )
            {
                Debug.LogError( $"Lua Error: {ex.Message}" );
                Debug.LogError( $"Line Number: {ex.DecoratedMessage}" );
            }
        }
    }

    public void CreateCoroutine( DynValue function )
    {
        var coroutine = _scriptObject.CreateCoroutine( function );
        _coroutines.Add( coroutine );
    }

    public void ExecuteCoroutines()
    {
        _deadCoroutines = new List<DynValue>();
        _coroutines.Do( x => {
            if( x.Coroutine.State == CoroutineState.Suspended ||
                x.Coroutine.State == CoroutineState.NotStarted )
                x.Coroutine.Resume();
            if( x.Coroutine.State == CoroutineState.Dead )
                _deadCoroutines.Add( x );
        } );

        _deadCoroutines.Do( x => _coroutines.Remove( x ) );
    }


    private void MakeCall( string method, params object[] args )
    {
        object handle = null;
        if( _methods.TryGetValue( method, out handle ) )
        {
            try
            {
                _scriptObject.Call( handle, args );
            }
            catch( ScriptRuntimeException ex )
            {
                Debug.LogError( $"Lua Error: {ex.Message}" );
                Debug.LogError( $"Line Number: {ex.DecoratedMessage}" );
            }
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

    public bool GetKeyDown( KeyCode key )
    {
        return Input.GetKeyDown( key );
    }

}
