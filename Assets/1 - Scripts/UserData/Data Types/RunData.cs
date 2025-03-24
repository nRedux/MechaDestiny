using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;


public class DoSceneWarmup : GameEvent { }


[System.Serializable]
public class RunData
{

    private bool _isValid = false;

    //What scene are we in right now?
    public string Scene;

    public MapData WorldMapData;
    public CompanyData CompanyData = new CompanyData();
    public MapObjectData Caravan = null;
    public StorageContainer Inventory = new StorageContainer();
    public EncounterState EncounterState = new EncounterState();
    public TimeData TimeData = new TimeData();

    //Should be null by default
    public string ActiveScene = null;

    [JsonProperty]
    private bool _warmupScene = true;

    /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
     * Need to make this be used and make sure lua scripts are able to set it. I'd also love
     *!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
    public List<Actor> CombatEnemies;

    public string WorldMapScene;

    public RunData()
    {
        _isValid = false;
    }

    public void Validate( MapObjectData caravanData )
    {
        Caravan = caravanData;
        _isValid = true;
    }

    public bool IsValid( )
    {
        return _isValid;
    }


    public void SetActiveScene( string scene )
    {
        this.Scene = scene;
    }

    public bool SceneNeedsWarmup()
    {
        return _warmupScene;
    }

    public void DoSceneWarmup()
    {
        if( !_warmupScene )
            return;
        _warmupScene = false;
        Events.Instance.Raise( new DoSceneWarmup() );
    }

}
