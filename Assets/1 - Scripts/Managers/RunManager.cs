using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu( menuName = "Engine/Create/RunManager")]
public class RunManager : SingletonScriptableObject<RunManager>
{

    public ScriptGraphAssetReference DefaultCombatEndGraph;
    public RunData RunData { 
        get => DataHandler<RunData>.Data; 
    }
    public MapData MapData { get => RunData.MapData; }

    static RunManager()
    {
        
    }

#if UNITY_EDITOR
    [InitializeOnEnterPlayMode]
    static void OnEnterPlaymodeInEditor( EnterPlayModeOptions options )
    {
        DataHandler<RunData>.Clear();
    }
#endif

    private void OnEnable()
    {
        DataHandler<RunData>.OnDataCreated = RunDataCreated;
    }

    public static void RunDataCreated(RunData data)
    {
        data.CompanyData.Employees = GlobalSettings.Instance.GetStarterActorsCollection();
    }

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
