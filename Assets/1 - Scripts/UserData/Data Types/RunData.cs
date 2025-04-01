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
    public IMapEntityData Caravan = null;
    public StorageContainer Inventory = new StorageContainer();
    public EncounterState EncounterState = new EncounterState();
    public TimeData TimeData = new TimeData();

    public List<IItem> CombatRewards = null;

    //Should be null by default
    public string ActiveScene = null;

    public List<Actor> CombatEnemies;

    public int? CombatMoneyReward = null;

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

    public void ClearRewards()
    {
        CombatMoneyReward = 0;
    }

}
