using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.PlayerLoop;


#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu( menuName = "Engine/Create/RunManager")]
public class RunManager : Singleton<RunManager>
{

//    public ScriptGraphAssetReference DefaultCombatEndGraph;
    public RunData RunData {
        get => DataHandler.RunData;
    }

    public MapData MapData { get => RunData.WorldMapData; }

    static RunManager()
    {
        
    }

#if UNITY_EDITOR
    [InitializeOnEnterPlayMode]
    static void OnEnterPlaymodeInEditor( EnterPlayModeOptions options )
    {
        DataHandler.Clear();
    }
#endif

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        DataHandler.OnDataCreated = RunDataCreated;
    }

    public static void RunDataCreated(RunData data)
    {
        data.CompanyData.Employees = GlobalSettings.Instance.GetStarterActorsCollection();
        data.CompanyData.Mechs = data.CompanyData.Employees.Select( x => x.PilotedMech ).ToList();

        GlobalSettings.Instance.TestMechComponentInventory.Do( x =>
        {
            if( x.RuntimeKeyIsValid() )
            {
                var inst = x.GetDataCopySync();

                var canAdd = data.Inventory.CanAddItem( inst );
                if( canAdd == AddStoreItemReason.CanStore )
                    data.Inventory.AddItem( inst );
                else
                    Debug.LogError( canAdd.ToString() );
            }
        } );

        
    }

    public void Update()
    {
        if( Input.GetKeyDown( KeyCode.U ) )
            SaveData();
    }

    public void SetScene( string scene )
    {
        this.RunData.ActiveScene = scene;
    }

    public void SetMapData( MapData mapData )
    {
        this.RunData.WorldMapData = mapData;
        this.RunData.ActiveScene = mapData.Scene;
    }


    public void SaveData()
    {
        DataHandler.SaveData();
    }

    public async void RunCombatEndGraph( EncounterData encounter )
    {
        /*
        bool runDefault = true;

        if( runDefault && DefaultCombatEndGraph.RuntimeKeyIsValid() )
        {
            var combatEndGraph = await DefaultCombatEndGraph.GetAssetAsync();
            combatEndGraph.Run( null );
        }
        */

    }

}
