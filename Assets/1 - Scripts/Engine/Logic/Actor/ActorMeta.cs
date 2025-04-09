using System;
using Newtonsoft.Json;
using UnityEngine;

public enum ActorActivity
{
    Idle,
    Pilot,
}

public class PilotException: System.Exception
{
    public PilotException() : base() { }
    public PilotException(string message) : base(message) { }
    public PilotException( string message, System.Exception innerException ) : base( message, innerException ) { }
}


public partial class Actor
{
    [JsonIgnore]
    [HideInInspector]
    public MechData PilotedMech { get => _pilotedMech; }

    [JsonProperty]
    public ActorActivity Activity { get; private set; } = ActorActivity.Idle;

    [JsonProperty]
    private MechData _pilotedMech = null;


    public void StartPilotingMech( MechData mechData )
    {
        if( mechData == null )
            throw new System.ArgumentNullException( $"Argument {nameof( mechData )} can't be null" );

        if( mechData == _pilotedMech )
            return;

        if( mechData.HasPilot )
        {
            if( mechData.Pilot == this )
                return;
            else
                throw new PilotException( "Can't pilot mech. Mech already has a pilot." );
        }

        StopPilotingMech();

        mechData.SetPilot( this );
        this._pilotedMech = mechData;

        Activity = ActorActivity.Pilot;
    }


    public void StopPilotingMech()
    {
        _pilotedMech?.RemovePilot();
        _pilotedMech = null;
    }
}
