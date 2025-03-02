using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;


[System.Flags]
public enum WeaponFlags
{
    NONE = 0,
    ShotgunFireMode = 1 << 1,
    AOE = 1 << 2,
    Melee = 1 << 10,
    ShortRange = 1 << 11,
    MidRange = 1 << 12,
    LongRange = 1 << 13

}

[System.Flags]
public enum StatusFlagsType
{
    NONE = 0,
    Broken = 1 << 1,
}


[System.Serializable]
public class MechComponentData : SimpleEntity<MechComponentAsset>
{

    public GameObjectReference Model;

    public MechComponentType Type;

    [ShowIf(nameof( ShowAsWeapon ) )]
    public WeaponType WeaponType = WeaponType.Melee;

    public AttachmentSlotType AttachmentType;

    public AttachmentSlot[] Attachments;

    public WeaponFlags Flags;

    [HideInInspector]
    [JsonIgnore]
    public GridShape AOEShape;

    public System.Action OnBroken;

    [JsonIgnore]
    public GfxComponent ModelInstance { get; set; }

    [JsonProperty]
    private bool _initialized = false;

    [JsonIgnore]
    public WeaponTypeInfo WeaponTypeInfo
    {
        get
        {
            return GlobalSettings.Instance.WeaponTypes.FirstOrDefault( x => x.Type == WeaponType );
        }
    }


    public bool ShowAsWeapon()
    {
        return Type == MechComponentType.Weapon;
    }


    /// <summary>
    /// First time initialization. Should be called after deserialized.
    /// </summary>
    public void Initialize()
    {
        //We only want to do anything if not initialized previously
        if( _initialized )
            return;

        ClearAllFlags();
        SetFeatureFlags( (int) Flags );

        InitializeAttachments();

        WatchStatistics();
        _initialized = true;
    }


    private void InitializeAttachments()
    {
        Attachments.Do( x =>
        {
            x.Initialize( this );
            this.AddSubEntity( x.Data );
        } );
    }


    public void DestroyAttachments()
    {
        Attachments.Do( x => x.Data.MakeBroken() );
    }


    private void WatchStatistics()
    {
        //Wrap in anonymous method so we can know the type in the callback
        Statistics.Do( x => x.Value.ValueChanged += OnStatisticChanged );
    }

    public bool IsAOE()
    {
        return HasFeatureFlag( (int) WeaponFlags.AOE );
    }


    private void OnStatisticChanged( StatisticChange change )
    {
        if( change.Statistic.Type != StatisticType.Health )
            return;
    }


    public override string GetDisplayName()
    {
        return this.ID;
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
        
        compData.ClearAllFlags();
        compData.SetFeatureFlags( (int) Flags );

        return compData;
    }


    public bool IsBroken()
    {
        var health = GetStatistic( StatisticType.Health );
        if( health == null )
            return false;
        return health.Value <= 0;
    }


    public void MakeBroken()
    {
        var health = GetStatistic( StatisticType.Health );
        if( health == null )
            return;
        health.EmitUIChangeImmediate = true;
        health.SetValue( 0 );
    }


    public override void OnStatisticChanged( StatisticType type, Statistic statistic )
    {
        base.OnStatisticChanged( type, statistic );
    }


    public float CalculateAccuracy( int distance )
    {
        var minAccuracy = GetStatistic( StatisticType.MinAccuracy );
        var maxAccuracy = GetStatistic( StatisticType.MaxAccuracy );
        var range = GetStatistic( StatisticType.Range );

        //We consider distance to be 1 based. 1 = best accuracy
        var calcDistance = distance - 1;

        if( minAccuracy == null || maxAccuracy == null || range == null )
            return 0f;
        if( calcDistance > range.Value || calcDistance < 0 )
            return 0f;

        float distQuotient = 1f - calcDistance / (float)range.Value;
        return Mathf.Lerp( minAccuracy.Value, maxAccuracy.Value, distQuotient );

    }
}
