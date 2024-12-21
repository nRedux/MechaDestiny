using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


public enum TimeFunction
{
    Pause,
    Play
}
public class ControlTime : Unit
{
    [DoNotSerialize]
    public ControlInput Enter;

    [DoNotSerialize]
    public ControlOutput Exit;

    [DoNotSerialize]
    public ValueInput Function;


    protected override void Definition()
    {

        Function = ValueInput<TimeFunction>( nameof( Function ), TimeFunction.Pause );

        Exit = ControlOutput( nameof( Exit ) );
        Enter = ControlInput( nameof( Enter ), ( flow ) =>
        {
            TimeFunction function = flow.GetValue<TimeFunction>( Function );

            switch( function )
            {
                case TimeFunction.Pause:
                    TimeManager.PauseTime();
                    break;

                case TimeFunction.Play:
                    TimeManager.PlayTime();
                    break;
            }
            return Exit;
        } );

    }


}
