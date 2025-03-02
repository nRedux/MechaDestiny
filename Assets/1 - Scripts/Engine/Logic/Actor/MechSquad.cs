using Newtonsoft.Json;
using UnityEngine;


public class MechSquadException : System.Exception
{
    public MechSquadException() : base() { }
    public MechSquadException( string message ) : base( message ) { }
    public MechSquadException( string message, System.Exception innerException ) : base( message, innerException ) { }
}


[JsonObject]
public class MechSquad
{

    private ActorCollection _actors;


    public void AddMember( Actor actor )
    {
        if( actor == null )
            throw new System.ArgumentNullException( $"Argument {nameof( actor )} cannot be null." );
        if( actor.PilotedMech == null )
            throw new MechSquadException( "Actor can't be added to squad unless it is piloting a mech" );
        if( _actors.ContainsActor( actor ) )
            return;
        _actors.Add( actor );
    }


    public bool RemoveMember( Actor actor )
    {
        return _actors.Remove( actor );
    }

}
