using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( menuName = "Engine/Create/DayManager" )]
public class TimeManager : SingletonScriptableObject<TimeManager>
{

    public TimeData DayData;

    public static bool IsPlaying
    {
        get => Instance.DayData.IsPlaying();
    }

    protected override void Initialize()
    {
        DayData = DataHandler.RunData.TimeData;

        DayData.DayTick += EncounterManager.Instance.OnDayPassed;
        DayData.HourTick += EncounterManager.Instance.OnHourPassed;
    }


    public void UpdateTime()
    {
        if( !DayData.IsPlaying() )
            DayData.TickTime( 0f );
        else
            DayData.TickTime( Time.deltaTime );
    }


    public void Update()
    {
        UpdateTime();
    }

    public static bool TogglePlaying()
    {
        var dayData = TimeManager.Instance.DayData;
        dayData.SetTimePlaying( !dayData.IsPlaying() );
        return dayData.IsPlaying();
    }

    public static void PauseTime()
    {
        TimeManager.Instance.DayData.SetTimePlaying( false );
    }

    public static void PlayTime()
    {
        TimeManager.Instance.DayData.SetTimePlaying( true );
    }
}
