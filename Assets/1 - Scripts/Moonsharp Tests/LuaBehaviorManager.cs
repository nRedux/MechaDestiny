using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

public enum LuaEvent
{
    ActorDamage = 0x000010,
    ActorDied = 0x000010
}

[CreateAssetMenu(fileName = "Lua Behavior Manager", menuName = "Engine/Managers/LuaBehaviorManager")]

public class LuaBehaviorManager : SingletonScriptableObject<LuaBehaviorManager>
{

    private List<LuaBehavior> _behaviors = new List<LuaBehavior>();

    private Dictionary<string, object> _superGlobals = new Dictionary<string, object>();

    public void SetSuperGlobal( string key, object value )
    {
        _superGlobals.Add( key, value );
    }

    private void OnEnable()
    {
        
    }

    protected override void Initialize()
    {
        base.Initialize();
        SetupLUAEnv();
    }

    public void OnTurnChanged(TurnManager mgr )
    {
        _behaviors.Do( x =>
        {
            if( x == null )
                return;
            x.CallTurnChanged( mgr.TurnNumber / 2 );
        } );
    }

    public void OnTeamsCreated()
    {
        _behaviors.Do( x =>
        {
            if( x == null )
                return;
            x.CallTeamsCreated();
        } );
    }

    public void SetupLUAEnv()
    {
        UserData.RegisterType<Actor>();
        UserData.RegisterType<SpawnLocation>();
        UserData.RegisterType<Team>();
        UserData.RegisterType<Dictionary<string, object>>();

        UserData.RegisterType<List<Actor>>();
        UserData.RegisterType<UnityEngine.Input>();

        UserData.RegisterType<ActorStatus>();
        UserData.RegisterType<KeyCode>();

        //Script.DefaultOptions.ScriptLoader = new UnityAssetsScriptLoader("LUA Common");
        Dictionary<string, string> scripts = new Dictionary<string, string>();
        object[] result = Resources.LoadAll( "MoonSharp/Scripts", typeof( TextAsset ) );
        foreach( TextAsset ta in result.OfType<TextAsset>() )
        {
            scripts.Add( ta.name, ta.text );
        }

        ( (ScriptLoaderBase) Script.DefaultOptions.ScriptLoader ).ModulePaths = new string[] { "Lua/?", "Lua/?.txt" };

    }

    public void RegisterBehavior( LuaBehavior behavior )
    {
        _behaviors.Add( behavior );
        behavior.SetProperty( "_sG", _superGlobals );
    }

    public void Update()
    {
        _behaviors.Do( x => x.CallUpdate() );
    }


}
