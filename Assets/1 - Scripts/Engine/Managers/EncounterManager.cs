using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu( menuName = "Engine/Create/Encounter Manager" )]
public class EncounterManager : SingletonScriptableObject<EncounterManager>
{

    public EncounterState EncounterState;
    bool skipFirstDay = true;

    protected override void Initialize()
    {
        EncounterState = DataHandler.RunData.EncounterState;
    }


    public void AddEncounter( EncounterData newEncounter )
    {
        EncounterState.Encounters.Add( newEncounter );
    }


    public void RemoveEncounter( EncounterData newEncounter )
    {
        EncounterState.Encounters.Remove( newEncounter );
    }


    public void AddRunningEvent( GameObject runner, ScriptGraphAssetReference sourceGraph )
    {
        //_runningGraphs.Add( runner, sourceGraph );
    }


    public void OnHourPassed()
    {
        
    }


    public async void RunEncounter( EncounterData encounter )
    {
        var graphAsset = await encounter.EventGraph.GetAssetAsync( );
        VisualScriptingUtility.RunGraph( graphAsset, () => {

        } );
    }


    public void OnDayPassed()
    {
        //TestEncounters();
    }


    private void TestEncounters()
    {
        if( EncounterState.CanRunMainEvent() )
        {
            var activatedEncounter = CheckActivatedEncounter( DataHandler.RunData.Caravan.Position );
            if( activatedEncounter != null )
            {
                EncounterState.RanMainEncounter( 1 );
                RunEncounter( activatedEncounter );
            }
        }

        EncounterState.DecrementEncounterDelays();
    }


    //See if any encounter activates.
    private EncounterData CheckActivatedEncounter(Vector3 pos)
    {
        var encInrange = GetEncountersInRange( pos );

        foreach( var enc in encInrange )
        {
            if( enc.TestActivation() )
                return enc;
        }
        return null;
    }


    private EncounterWeightedRandom GetRandomSelector( Vector3 position )
    {
        var inRange = FindEncountersInRange( position );
        var unique = inRange.Distinct( new EncounterEventComparer() ).ToList();
        EncounterWeightedRandom bag = new EncounterWeightedRandom();

        unique.Do( x => bag.Add( x.Probability, x ) );
        return bag;
    }

    private List<EncounterData> GetEncountersInRange( Vector3 position )
    {
        var inRange = FindEncountersInRange( position );
        var items = inRange.Distinct( new EncounterEventComparer() ).ToList();
        items.Reverse();
        return items;
    }

    /// <summary>
    /// Find all encounters within range of a point
    /// </summary>
    /// <param name="position">The world position</param>
    public List<EncounterData> FindEncountersInRange( Vector3 position )
    {
        return this.EncounterState.Encounters.GetInRange( position );
    }
}
