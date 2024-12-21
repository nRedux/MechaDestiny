using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;
using Unity.VisualScripting;

public enum EntityFlags
{
    Damagable
}

[System.Serializable]
public class SimpleEntity<TData> : DataObject<TData>, IEntity
{
    [HideInInspector]
    public string ID;

    public StatisticCollection Statistics;

    [HideInInspector]
    public EntityCollection SubEntities = new EntityCollection();

    public int FeatureFlags => _entityFlags;

    public int StatusFlags => _statusFlags;


    private int _statusFlags = 0;
    private int _entityFlags = 0;
    private IEntity _parent;

    public void ClearAllFlags()
    {
        _entityFlags = 0;
        _statusFlags = 0;
    }

    public void SetFeatureFlags( int flags )
    {
        SetFlags( ref _entityFlags, flags );
    }

    public void UnsetFeatureFlags( int flags )
    {
        UnsetFlags( ref _entityFlags, flags );
    }

    public bool HasFeatureFlags( int flags )
    {
        return HasFlags( _entityFlags, flags );
    }

    public bool HasFeatureFlag( int flag )
    {
        return HasFlag( _entityFlags, flag );
    }

    public bool NotHasFeatureFlag( int flag )
    {
        return NotHasFlag( _entityFlags, flag );
    }

    public void SetStatusFlags( int flags )
    {
        SetFlags( ref _statusFlags, flags );
    }

    public void UnsetStatusFlags( int flags )
    {
        UnsetFlags( ref _statusFlags, flags );
    }

    public bool HasStatusFlags( int flags )
    {
        return HasFlags( _statusFlags, flags );
    }

    public bool HasStatusFlag( int flag )
    {
        return HasFlag( _statusFlags, flag );
    }

    public bool NotHasStatusFlag( int flag )
    {
        return NotHasFlag( _statusFlags, flag );
    }


    public IEntity GetParent()
    {
        return _parent;
    }

    public void SetParent( IEntity parent )
    {
        _parent = parent;
    }


    public string GetID()
    {
        return ID;
    }


    public IStatisticSource GetStatistics()
    {
        return Statistics;
    }


    public virtual string GetDisplayName()
    {
        return "SimpleEntity";
    }


    public List<IEntity> GetSubEntities()
    {
        return SubEntities;
    }


    public bool CanAddSubEntity( IEntity entity )
    {
        return !this.SubEntities.Contains( entity );
    }


    public void AddSubEntity( IEntity entity )
    {
        entity.SetParent( this );
        this.SubEntities.Add( entity );
    }

    public virtual bool IsDead()
    {
        var health = this.Statistics.GetStatistic( StatisticType.Health );
        if( health == null )
            return false;//TODO: We should make this obvious to whomever is testing that this entity can't die! Something needs to happen to make it obvious without wasted time!
        return health.Value <= 0;
    }

    public MechComponentData FindWeaponEntity( IEntity entity )
    {
        if( entity is MechComponentData component && component.Type == MechComponentType.Weapon )
        {
            return component;
        }

        var subEntities = entity.GetSubEntities();
        foreach( var subEnt in subEntities ) {
            //Only accept non broken components
            var res = FindWeaponEntity( subEnt );
            if( res == null || res.IsBroken() )
                continue;
            return res;
        }

        return null;
    }

    public void FindWeaponEntities( IEntity entity, ref List<IEntity> results, System.Func<MechComponentData, bool> predicate = null )
    {
        if( entity is MechComponentData component && component.Type == MechComponentType.Weapon )
        {
            if( predicate?.Invoke( component ) ?? true )
                results.Add( entity );
        }

        var subEntities = entity.GetSubEntities();
        foreach( var subEnt in subEntities )
        {
            FindWeaponEntities( subEnt, ref results );
        }
    }

    public bool HasUsableWeapons( )
    {
        List<IEntity> weapons = new List<IEntity>();
        FindWeaponEntities( this, ref weapons, c => !c.IsBroken() );
        return weapons.Count > 0;
    }

    public Statistic GetStatistic( StatisticType type )
    {
        return Statistics.GetStatistic( type );
    }

    public virtual void OnStatisticChanged( StatisticType type, Statistic statistic )
    {

    }
}
