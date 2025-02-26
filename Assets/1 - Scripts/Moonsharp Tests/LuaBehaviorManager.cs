using System.Collections.Generic;
using MoonSharp.Interpreter;
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
