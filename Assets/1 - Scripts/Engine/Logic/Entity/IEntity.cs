using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entity is a thing which can be targeted, can have sub entities as well.
/// </summary>
public interface IEntity
{
    public int FeatureFlags { get; }

    public IStatisticSource GetStatistics();

    public Statistic GetStatistic( StatisticType type );
    
    public List<IEntity> GetSubEntities();

    public IEntity GetParent();

    public void SetParent( IEntity entity );

    public string GetID();

    public string GetDisplayName();

    public bool IsDead();
}

public class EntityCollection: List<IEntity>
{

}
