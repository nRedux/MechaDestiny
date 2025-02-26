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

    private void OnEnable()
    {
        
    }

    protected override void Initialize()
    {
        base.Initialize();
        SetupLUAEnv();
    }

    public void SetupLUAEnv()
    {
        UserData.RegisterType<Actor>();
        UserData.RegisterType<SpawnLocation>();
        UserData.RegisterType<ActorStatus>();
    }

    public void RegisterBehavior( LuaBehavior behavior )
    {
        _behaviors.Add( behavior );
    }


}
