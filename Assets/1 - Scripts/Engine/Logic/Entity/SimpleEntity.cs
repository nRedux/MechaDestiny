using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Runtime.Serialization;
using UnityEngine.Android;

public enum EntityFlags
{
    Damagable
}


[System.Serializable]
public class SimpleEntity<TData> : DataObject<TData>, IEntity
{
    [HideInInspector]
    public string ID;

    public StatisticCollection Statistics = new StatisticCollection();

    [HideInInspector]
    public EntityCollection SubEntities = new EntityCollection();

    public StatisticCollection Boosts = new StatisticCollection();

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

    [OnDeserialized]
    void OnDeserialized( StreamingContext c )
    {
        Statistics.SetEntity( this );
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


    public IEntity GetRoot()
    {
        if( _parent == null )
            return this;
        return _parent.GetRoot();
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

    //TODO: Naming is bad. IsDead will return true even if Actor.Die() has yet to be called.
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

    public List<IEntity> FindWeaponEntities( System.Func<MechComponentData, bool> predicate = null )
    {
        List<IEntity> results = new List<IEntity>();
        FindWeaponEntities( this, ref results, predicate );
        return results;
    }

    public bool HasUsableWeapons( )
    {
        List<IEntity> weapons = FindWeaponEntities( c => !c.IsBroken() );
        return weapons.Count > 0;
    }

    public Statistic GetStatistic( StatisticType type )
    {
        return Statistics.GetStatistic( type );
    }

    public int GetStatisticValue( StatisticType type )
    {
        var stat = Statistics.GetStatistic( type );
        //TODO: Should I throw here?
        if( stat == null )
        {
            Debug.LogWarning($"Requests statistic value which doesn't exist in entity. {GetID()}");
            return 0;
        }
        return stat.Value;
    }

    public virtual void OnStatisticChanged( StatisticType type, Statistic statistic )
    {

    }
}
