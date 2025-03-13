using System.Collections;
using System.Collections.Generic;
using FMOD;
using UnityEngine;


public enum ActionResultStatus
{
    Running,
    Finished
}

public struct StatisticChange
{
    public MechComponentData MechComponent;
    public Statistic Statistic;
    public int Change;
    public int Value;
    public int OldValue;
    public StatChangeTag Tags;

    public bool IsValid { get; private set; }

    public StatisticChange( MechComponentData component, Statistic statistic, int oldValue, int value, StatChangeTag tags = StatChangeTag.None )
    {
        this.MechComponent = component;
        this.Statistic = statistic;
        this.OldValue = oldValue;
        this.Value = value;
        this.Change = value - oldValue;
        this.Tags = tags;
        IsValid = true;
    }

    public void EmitUIUpdateEvent()
    {
        this.Statistic.UIValueUpdate( this );
    }

    public string GetChangeStringified()
    {
        if( ( Tags & StatChangeTag.Miss ) == StatChangeTag.Miss )
            return "Miss";
        else
            return Change.ToString();
    }
}

public abstract class ActionResult
{
    public SmartPoint Target;
    public System.Action OnComplete;

    private List<Statistic> _watchedStatistics = new List<Statistic>();
    private System.Action<StatisticChange> _listeners;
    private List<StatisticChange> _recordedChanges = new List<StatisticChange>();


    public abstract ActionResultStatus Update();
    
    
    public virtual void Start()
    {

    }


    /// <summary>
    /// Eliminate change listeners which have been generated.
    /// </summary>
    public void StopListeningStatChanges()
    {
        _watchedStatistics.Do( x => {
            x.ValueChanged -= _listeners;
        } );
        _watchedStatistics = null;
        _listeners = null;
    }


    /// <summary>
    /// Watch a component for any statistic changes which happen in it.
    /// </summary>
    /// <param name="mechComponent">The component you want to watch</param>
    /// <param name="statistic">The statistic you want to watch</param>
    public void WatchForStatChanges( MechComponentData mechComponent, Statistic statistic )
    {
        if( _watchedStatistics.Contains( statistic ) )
            return;
        _watchedStatistics.Add( statistic );

        //Generate a wrapper which will invoke the statistic changed method with component specific info.
        System.Action<StatisticChange> statChangedWrapper = ( change ) => {
            RecordStatisticChange( mechComponent, change );
        };

        //Track the wrappers we've created
        _listeners += statChangedWrapper;

        //Add listeners which will be invoked if the statistic's value changes
        statistic.ValueChanged += statChangedWrapper;
    }


    /// <summary>
    /// Record a change which happened in a statistic
    /// </summary>
    /// <param name="data">The component which saw the statistic change</param>
    /// <param name="stat"></param>
    /// <param name="oldVal"></param>
    public void RecordStatisticChange( MechComponentData data, StatisticChange change )
    {
        change.MechComponent = data;
        _recordedChanges.Add( change );
    }


    public StatisticChange[] GetChanges()
    {
        var result = _recordedChanges.ToArray();
        return result;
    }

    /// <summary>
    /// Take a change which was recorded from a watched statistic
    /// </summary>
    /// <returns>The recorded statistic change</returns>
    public StatisticChange? TakeChange()
    {

        if( _recordedChanges.Count > 0 )
        {
            var change = _recordedChanges[0];
            change.EmitUIUpdateEvent();
            _recordedChanges.RemoveAt( 0 );
            return change;
        }
        else
            return null;
    }

    public StatisticChange[] TakeChanges()
    {
        var result = _recordedChanges.ToArray();
        result.Do( x => x.EmitUIUpdateEvent() );
        _recordedChanges.Clear();
        return result;
    }

}