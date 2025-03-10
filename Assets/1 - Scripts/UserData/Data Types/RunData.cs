using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DoSceneWarmup : GameEvent { }


[System.Serializable]
public class RunData
{

    private bool _isValid = false;

    //What scene are we in right now?
    public string Scene;

    public MapData MapData;
    public CompanyData CompanyData = new CompanyData();
    public MapObjectData Caravan = new MapObjectData();

    private bool _warmupScene = true;

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


    public void SetActiveScene( string scene, bool doInitState )
    {
        this.Scene = scene;
        _warmupScene = doInitState;
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
