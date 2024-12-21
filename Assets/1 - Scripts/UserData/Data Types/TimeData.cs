using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class TimeData
{
    public float TimeSpeed = 1f;
    public float DayLength = 10f;
    public float WorldTime = 0f;
    private bool _playing = true;

    private float _lastWorldTime;
    private int _hoursCounter, _daysCounter;

    [JsonIgnore]
    public System.Action DayTick;

    [JsonIgnore]
    public System.Action HourTick;

    public float Days
    {
        get
        {
            return WorldTime / DayLength;
        }
    }

    public float Hours
    {
        get
        {
            return WorldTime / (DayLength / 24);
        }
    }

    public float HoursDelta
    {
        get
        {
            return DeltaTime / ( DayLength / 24 );
        }
    }


    public float DeltaTime
    {
        get => WorldTime - _lastWorldTime;
    }


    public void TickTime( float deltaTime )
    {
        _lastWorldTime = WorldTime;
        WorldTime += deltaTime * TimeSpeed;

        if( (int)Days != _daysCounter )
        {
            _daysCounter = (int)Days;
            DayTick?.Invoke();
        }

        if( (int)Hours != _hoursCounter )
        {
            _hoursCounter = (int) Hours;
            HourTick?.Invoke();
        }
    }

    public bool IsPlaying()
    {
        return _playing;
    }
    
    public void SetTimePlaying( bool playing )
    {
        Debug.Log( $"Set time playing: {playing}" );
        this._playing = playing;

        Events.Instance.Raise( new TimeModeChanged() { IsPlaying = this._playing } );
    }

}
