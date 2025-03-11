using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;


[System.Serializable]
public class IntUnityEvent : UnityEvent<int> { }

[System.Serializable]
public class FloatUnityEvent : UnityEvent<float> { }

[System.Serializable]
public class StringUnityEvent : UnityEvent<string> { }


public class StatisticWatcher
{
    [System.NonSerialized]
    public Statistic _statistic;


    private System.Action<StatisticChange> _onChange = null;

    public StatisticWatcher( Statistic statistic, System.Action<StatisticChange> onChange )
    {
        if( onChange == null )
            return;
        if( statistic == null )
            return;

        _onChange = onChange;
        this._statistic = statistic;
        this._statistic.OnUIValueUpdate += onChange;
    }


    public void Stop()
    {
        if( this._statistic == null || _onChange == null )
            return;
        this._statistic.OnUIValueUpdate -= _onChange;
    }


    protected virtual void StatisticChanged( StatisticChange stat )
    {

    }
}


public class UIStatisticListener : UIEntityElement
{

    [Tooltip("What statistic we display values for.")]
    public StatisticType StatisticType;

    [System.NonSerialized]
    public Statistic _statistic;

    public int Value;

    public IntUnityEvent ValueChanged = new IntUnityEvent();
    public FloatUnityEvent NormalizedValueChanged = new FloatUnityEvent();
    public StringUnityEvent StringValueChanged = new StringUnityEvent();


    protected override void OnDestroy()
    {
        UnbindStatistic();
    }


    public override void OnEntityAssigned( IEntity entity )
    {
        base.OnEntityAssigned( entity );
        var stats = entity.GetStatistics();
        var stat = stats.GetStatistic( StatisticType );
        WatchStatistic( stat );
    }


    private void WatchStatistic(Statistic statistic)
    {
        //Unbind previous statistic
        UnbindStatistic();
        if( statistic == null )
        {
            Debug.LogError($"Statistic {StatisticType} not present in watched entity.", gameObject);
            return;
        }
        Value = statistic.Value;
        this._statistic = statistic;
        this._statistic.OnUIValueUpdate += StatisticChanged;

        StatisticChanged( new StatisticChange(null, _statistic, _statistic.Value, _statistic.Value));
    }


    private void UnbindStatistic( )
    {
        if( this._statistic == null )   
            return;
        this._statistic.OnUIValueUpdate -= StatisticChanged;
    }


    private void Update()
    {
        if( Input.GetKeyDown( KeyCode.Alpha1) )
        {
            StringValueChanged.Invoke( _statistic.Value.ToString() );
            ValueChanged.Invoke( _statistic.Value );
            NormalizedValueChanged.Invoke( _statistic.Value / (float) _statistic.StartValue );
        }
    }


    protected virtual void StatisticChanged( StatisticChange stat )
    {
        Value = stat.Value;
        StringValueChanged.Invoke( stat.Value.ToString() );
        ValueChanged.Invoke( stat.Value );
        NormalizedValueChanged.Invoke( stat.Value / (float) stat.Statistic.StartValue );
    }


}
