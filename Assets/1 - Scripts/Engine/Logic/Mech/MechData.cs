using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;
using System;

[System.Serializable]
public class MechData: SimpleEntity<MechAsset>
{
    public GameObjectReference Avatar;

    public Color MechColor;

    public MechComponentReference TorsoAsset;
    public MechComponentReference LegsAsset;
    public MechComponentReference LeftArmAsset;
    public MechComponentReference RightArmAsset;

    [HideInInspector]
    public List<MechComponentData> ComponentData;

    [JsonIgnore]
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
    [NonSerialized]
    private bool _initialized = false;

    [JsonProperty]
    [NonSerialized]
    private Actor _pilot = null;

    [JsonProperty]
    private MechComponentData _activeWeapon = null;


    [JsonIgnore]
    public Actor Pilot { get => _pilot; }


    [JsonIgnore]
    public MechComponentData ActiveWeapon 
    { 
        get
        {
            if( _activeWeapon == null || _activeWeapon.IsBroken() )
            {
                ActiveWeapon = FindFunctionalRangedWeapons().FirstOrDefault() ?? FindFunctionalWeapons().FirstOrDefault();
            }
            return _activeWeapon;
        } 
        set 
        {
            var prevActive = _activeWeapon;
            if( value != null && value.IsBroken() )
            {
                _activeWeapon = null;
                if( prevActive != value )
                    ActiveWeaponChanged?.Invoke( value );
                return;
            }

            _activeWeapon = value;
            if( prevActive != value )
                ActiveWeaponChanged?.Invoke( value );
        }
    }


    public bool HasPilot
    {
        get => _pilot != null;
    }


    public void SetPilot( Actor pilot )
    {
        if( pilot == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( pilot )}' can't be null." );
        _pilot = pilot;
    }

    public void RemovePilot()
    {
        _pilot = null;
    }


    public int GetMoveRange()
    {
        
        if( Legs.IsBroken() )
            return 1;
        else
        {
            //Find max range as either the ap or range of the mech. 1ap per move atm.
            var apRange = Pilot.GetStatisticValue( StatisticType.AbilityPoints );
            int legsRange = Legs.Statistics.GetStatistic( StatisticType.Range ).Value;
            return Mathf.Min( legsRange, apRange );
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

        ActiveWeapon = FindFunctionalRangedWeapons().FirstOrDefault() ?? FindFunctionalWeapons().FirstOrDefault();
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
        MechComponentData compData = reference.GetDataCopySync();
        return compData;
    }

    public bool IsTorsoDestroyed()
    {
        var torsoStats = Torso.GetStatistics();
        var health = torsoStats.GetStatistic( StatisticType.Health );
        return health.Value <= 0;
    }

    public List<MechComponentData> GetAttackTargets()
    {
        var attackTargets = new List<MechComponentData>();
        attackTargets.Add( Torso );
        if( !LeftArm.IsBroken() )
            attackTargets.Add( LeftArm );
        if( !RightArm.IsBroken() )
            attackTargets.Add( RightArm );
        if( !Legs.IsBroken() )
            attackTargets.Add( Legs );
        return attackTargets;
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
