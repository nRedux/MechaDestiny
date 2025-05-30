using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.Android;

[System.Serializable]
public class StatisticCollection: Dictionary<StatisticType, Statistic>, IStatisticSource, ISerializationCallbackReceiver
{
    [JsonProperty]
    private IEntity _entity;
    [JsonProperty]
    [SerializeField]
    private List<StatisticType> _serializedKeys = new List<StatisticType>();
    [JsonProperty]
    [SerializeField]
    private List<Statistic> _serializedValues = new List<Statistic>();

    public IEntity Entity { get => _entity; }

    public StatisticCollection() : base() { }

    public StatisticCollection( StatisticCollection other ) : base( other ) { }

    public bool HasStatistic( StatisticType statistic)
    {
        return ContainsKey( statistic );
    }


    public Statistic GetStatistic( StatisticType statistic, bool createIfMissing = false )
    {
        if( ContainsKey( statistic ) )
            return this[statistic];
        else if( createIfMissing )
            return AddStatistic( statistic, 0 );
        else
            return null;
    }


    public Statistic AddStatistic( StatisticType statistic, int value )
    {
        Statistic newStatistic = new Statistic( statistic, value );
        Add( statistic, newStatistic );
        return newStatistic;
    }


    [OnDeserialized]
    public void OnJsonDeserialized( StreamingContext context )
    {
        this.Do( x => x.Value.Type = x.Key );
    }


    public void OnBeforeSerialize()
    {
        _serializedKeys.Clear();
        _serializedValues.Clear();
        foreach( var pair in this )
        {
            _serializedKeys.Add( pair.Key );
            _serializedValues.Add( pair.Value ); 
        }
    }


    public void OnAfterDeserialize()
    {
        this.Clear();

        if( _serializedKeys.Count != _serializedValues.Count )
            throw new System.Exception( string.Format( "there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable." ) );

        for( int i = 0; i < _serializedKeys.Count; i++ )
            this.Add( _serializedKeys[i], _serializedValues[i] );
    }


    internal void SetEntity( IEntity entity )
    {
        _entity = entity;
        this.Do( x => x.Value.Entity = entity );
    }


    /// <summary>
    /// Merge other collection into this collection. Creates stats in this from other if they don't exist in this. Adds values from other if they do exist in this.
    /// Does nothing with callbacks or events from other.
    /// </summary>
    /// <param name="other">The other statistic collection</param>
    public void Merge( StatisticCollection other )
    {
        foreach( var stat in other )
        {
            if( this.ContainsKey(stat.Key) )
            {
                //Merge into our value
                this[stat.Key].Value += stat.Value.Value;
            }
            else
            {
                //Add, we don't have it
                AddStatistic( stat.Key, stat.Value.Value );
            }
        }
    }
}
