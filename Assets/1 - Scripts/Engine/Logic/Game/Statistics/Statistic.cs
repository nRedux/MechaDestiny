using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Runtime.Serialization;
using static UnityEngine.Rendering.DebugUI;
using System;

public enum StatisticType 
{
    //Vital statistics
    Health = 0x0000,
    AbilityPoints = 0x0010,
    MaxAbilityPoints = 0x0011,

    //Ability statistics
    MinRange = 0x0099,
    Range = 0x0100,
    
    Damage = 0x0110,
    ShotCount = 0x0120,
    FragmentCount = 0x0125,

    MinAccuracy = 0x0130,
    MaxAccuracy = 0x0131,
}

[Flags]
public enum StatChangeTag
{
    None = 0,
    Miss = 1,
}


[HideLabel]
[System.Serializable]
public class Statistic
{
    [JsonProperty]
    [SerializeField]
    private int _value;

    [HideInInspector]
    [SerializeField]
    [JsonProperty]
    private int _startValue;


    [JsonIgnore]
    public System.Action<StatisticChange> ValueChanged;
    [JsonIgnore]
    public System.Action<StatisticChange> OnUIValueUpdate;

    [JsonIgnore]
    [HideInInspector]
    public StatisticType Type;

    [JsonIgnore]
    public int StartValue { get => _startValue; }

    public bool EmitUIChangeImmediate { get; set; }

    public void SetValue( int amount )
    {
        int oldValue = _value;
        _value = amount;
        InvokeChangeEvent( oldValue );
        DoEmitUIChangeImmediate( oldValue );
    }

    public void SetValue( int amount, StatChangeTag tag )
    {
        int oldValue = _value;
        _value = amount;
        InvokeChangeEvent( oldValue, tag );
        DoEmitUIChangeImmediate( oldValue, tag );
    }


    [JsonIgnore]
    public int Value
    {
        get => _value;
        set => _value = value;
    }

    private void InvokeChangeEvent( int oldValue, StatChangeTag tags = StatChangeTag.None )
    {
        if( ValueChanged == null )
            return;
        if( oldValue == _value && tags == StatChangeTag.None )
            return;
        var statChange = new StatisticChange( null, this, oldValue, _value, tags );
        ValueChanged( statChange );
    }


    private void DoEmitUIChangeImmediate( int oldValue, StatChangeTag tags = StatChangeTag.None )
    {
        if( !EmitUIChangeImmediate )
            return;
        EmitUIChangeImmediate = false;
        var statChange = new StatisticChange( null, this, oldValue, _value, tags );
        statChange.EmitUIUpdateEvent();
    }

    [JsonConstructor]
    public Statistic()
    {
        _value = 0;
    }


    public Statistic( int value )
    {
        _value = value;
    }


    public void UIValueUpdate( StatisticChange change )
    {
        OnUIValueUpdate?.Invoke( change );
    }


    [OnDeserialized]
    private void OnDeserialized( StreamingContext streamingContext )
    {
        _startValue = _value;
    }
}
