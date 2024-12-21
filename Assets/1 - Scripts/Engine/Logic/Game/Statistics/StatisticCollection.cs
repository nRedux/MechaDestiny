using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Runtime.Serialization;

[System.Serializable]
public class StatisticCollection: Dictionary<StatisticType, Statistic>, IStatisticSource, ISerializationCallbackReceiver
{
    [JsonProperty]
    [SerializeField]
    private List<StatisticType> _serializedKeys = new List<StatisticType>();
    [JsonProperty]
    [SerializeField]
    private List<Statistic> _serializedValues = new List<Statistic>();

    public bool HasStatistic( StatisticType statistic)
    {
        return ContainsKey( statistic );
    }


    public Statistic GetStatistic( StatisticType statistic )
    {
        if( ContainsKey( statistic ) )
            return this[statistic];
        else
            return null;
    }


    public Statistic AddStatistic( StatisticType statistic, int value )
    {
        Statistic newStatistic = new Statistic( value );
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
}
