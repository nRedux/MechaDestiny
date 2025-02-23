using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class MechData: SimpleEntity<MechAsset>
{

    public Color MechColor;

    public MechComponentReference TorsoAsset;
    public MechComponentReference LegsAsset;
    public MechComponentReference LeftArmAsset;
    public MechComponentReference RightArmAsset;

    [HideInInspector]
    public List<MechComponentData> ComponentData;

    public System.Action<MechComponentData> ActiveWeaponChanged;

    [HideInInspector]
    public MechComponentData Torso;
    [HideInInspector]
    public MechComponentData Legs;
    [HideInInspector]
    public MechComponentData LeftArm;
    [HideInInspector]
    public MechComponentData RightArm;

    [JsonProperty]
    private bool _initialized = false;

    [JsonProperty]
    private MechComponentData _activeWeapon = null;

    [JsonIgnore]
    public MechComponentData ActiveWeapon 
    { 
        get
        {
            if( _activeWeapon == null || _activeWeapon.IsBroken() )
            {
                List<IEntity> weps = FindWeaponEntities();
                ActiveWeapon = weps.FirstOrDefault() as MechComponentData;
            }
            return _activeWeapon;
        } 
        set 
        {
            var prevActive = _activeWeapon;
            _activeWeapon = value;
            if( prevActive != value )
            {
                ActiveWeaponChanged?.Invoke( value );
            }
        } 
    }


    public StatisticCollection AccumulateStatistics()
    {
        StatisticCollection accumulatedStats = new StatisticCollection();
        accumulatedStats.Merge( Torso.Statistics );
        accumulatedStats.Merge( Legs.Statistics );
        accumulatedStats.Merge( LeftArm.Statistics );
        accumulatedStats.Merge( RightArm.Statistics );
        return accumulatedStats;
    }


    /// <summary>
    /// First time initialization. Should be called after deserialized.
    /// </summary>
    public void InitializeComponentData()
    {
        //We only want to do anything if not initialized previously
        if( _initialized  )
            return;

        Torso = GetComponentData( TorsoAsset );
        Legs = GetComponentData( LegsAsset );
        LeftArm = GetComponentData( LeftArmAsset );
        RightArm = GetComponentData( RightArmAsset );

        AddSubEntity( Torso );
        AddSubEntity( Legs );
        AddSubEntity( LeftArm );
        AddSubEntity( RightArm );

        var data = new List<MechComponentData>() { Torso, Legs, LeftArm, RightArm };

        ComponentData = data.NonNull().ToList();

        ActiveWeapon = FindWeaponEntities().FirstOrDefault() as MechComponentData;
        _initialized = true;
    }


    /// <summary>
    /// Get a MechComponentData copy out of the asset pointed.
    /// </summary>
    /// <param name="reference">The MechComponentReference pointing at a MechComponent asset in the project.</param>
    /// <returns>A MechComponentData object, or null. If null the reference didn't point to an asset.</returns>
    private MechComponentData GetComponentData( MechComponentReference reference )
    {
        if( !reference.RuntimeKeyIsValid() )
            return null;
        MechComponentData compData = reference.GetDataSync();
        return compData;
    }

    public bool IsTorsoDestroyed()
    {
        var torsoStats = Torso.GetStatistics();
        var health = torsoStats.GetStatistic( StatisticType.Health );
        return health.Value <= 0;
    }

    public override bool IsDead()
    {
        return Torso.IsDead();
    }

    public int GetEvasion()
    {
        if( Legs == null )
            return 0;
        return Legs.GetStatisticValue( StatisticType.Evasion );
    }
}
