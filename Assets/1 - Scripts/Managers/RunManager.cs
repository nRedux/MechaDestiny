using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( menuName = "Engine/Create/RunManager")]
public class RunManager : SingletonScriptableObject<RunManager>
{

    public ScriptGraphAssetReference DefaultCombatEndGraph;
    public RunData RunData { get => DataHandler<RunData>.Data; }
    public MapData MapData { get => RunData.MapData; }

    public void SetScene( string scene, bool doSceneWarmup )
    {
        this.RunData.SetActiveScene( scene, doSceneWarmup );
    }


    public async void RunCombatEndGraph( EncounterData encounter )
    {
        bool runDefault = true;

        if( runDefault && DefaultCombatEndGraph.RuntimeKeyIsValid() )
        {
            var combatEndGraph = await DefaultCombatEndGraph.GetAssetAsync();
            combatEndGraph.Run( null );
        }

    }

}
