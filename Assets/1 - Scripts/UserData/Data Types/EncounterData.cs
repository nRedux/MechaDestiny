using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;

public class EncounterWeightedItem : WeightedRandomBagItem<EncounterData> { }

public class EncounterWeightedRandom : WeightedRandomBag<EncounterWeightedItem, EncounterData> { }

public class EncounterEventComparer: IEqualityComparer<EncounterData>
{
    public bool Equals(EncounterData a, EncounterData b )
    {
        if( ReferenceEquals( a, b ) )
            return true;
        if( !a.EventGraph.RuntimeKeyIsValid() || b.EventGraph.RuntimeKeyIsValid() )
            return false;
        return a.EventGraph.AssetGUID == b.EventGraph.AssetGUID;
    }

    public int GetHashCode( EncounterData data )
    {
        return data.EventGraph.AssetGUID.GetHashCode();
    }
}

[System.Serializable]
public class EncounterData
{

    public float Probability;
    public ScriptGraphAssetReference EventGraph;

    //Location info.
    [JsonIgnore]
    public SmartPoint Location;

    /// <summary>
    /// How far does this have an effect?
    /// </summary>
    public float Radius;

    public bool WithinAffectRange( Vector3 coordinate )
    {
        Vector3 delta = coordinate - Location.Position;
        return delta.magnitude <= Radius;
    }

    internal bool TestActivation()
    {
        float roll = UnityEngine.Random.value;
        return roll <= Probability;
    }
}


[System.Serializable]
public class EncounterCollection : List<EncounterData>
{

    [JsonConstructor]
    public EncounterCollection() { }


    public EncounterCollection( List<EncounterData> list ) : base( list ) { }


    /// <summary>
    /// Get all that affect a given coordinate
    /// </summary>
    /// <param name="position"></param>
    /// <returns>A new EncounterCollection containing all encounter data that affect a given coordinate</returns>
    public List<EncounterData> GetInRange( Vector3 position )
    {
        var result = this.Where( x => x.WithinAffectRange( position ) ).ToList();
        return new EncounterCollection( result );
    }

}


[System.Serializable]
public class RunningGraphInstance
{
    public ScriptGraphAssetReference Graph;
    public ScriptMachine Instance;
    public Dictionary<string, object> Variables = new Dictionary<string, object>();
}


[System.Serializable]
public class EncounterState
{
    public EncounterCollection Encounters = new EncounterCollection();

    public int MainEventDelay = 0;

    public List<RunningGraphInstance> RunningEncounterGraphs = new List<RunningGraphInstance>();

    public bool CanRunMainEvent()
    {
        return MainEventDelay <= 0;
    }

    public void RanMainEncounter( int delayAfter )
    {
        if( !CanRunMainEvent() )
            return;

        MainEventDelay++;
    }

    public void DecrementEncounterDelays()
    {
        MainEventDelay--;
    }

    public void AddRunningGraph( ScriptGraphAssetReference graphReference, ScriptMachine runningGraph )
    {

    }
}


/*
 * Finding events
 * Collect a list of all encounter potentials.
 * Run probabilities on encounters. 
 * Of all that succeed, rank by priority.
 * Choose random from highest priority.
 */

[CreateAssetMenu( menuName = "Engine/Create/Encounter Data", fileName = "New Encounter Data" )]
public class EncounterDataAsset: ScriptableObject
{
    [HideLabel]
    public EncounterData Data;
}