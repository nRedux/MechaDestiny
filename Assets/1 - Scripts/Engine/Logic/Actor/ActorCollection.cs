using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEngine;

[JsonObject]
public class ActorCollectionEntry: IEquatable<ActorCollectionEntry>
{

    [JsonProperty]
    private Actor _actor;


    [JsonIgnore]
    public Actor Actor
    {
        get => _actor; 
    }


    public ActorCollectionEntry( Actor actor )
    {
        if( actor == null )
            throw new System.ArgumentNullException( $"Argument '{nameof(actor)}' cannot be null" );
        this._actor = actor;
    }


    public static explicit operator Actor(ActorCollectionEntry entry)
    {
        return entry.Actor;
    }


    public static bool operator ==( Actor actor, ActorCollectionEntry entry )
    {
        return ActorEquals( actor, entry );
    }

    
    public static bool operator !=( Actor actor, ActorCollectionEntry entry )
    {
        return !( actor == entry );
    }


    public static bool operator ==( ActorCollectionEntry entry, Actor actor )
    {
        return ActorEquals( actor, entry );
    }


    public static bool operator !=( ActorCollectionEntry entry, Actor actor )
    {
        return !( actor == entry );
    }


    public static bool operator ==( ActorCollectionEntry lhs, ActorCollectionEntry rhs)
    {
        if( ReferenceEquals( lhs, rhs ) )
            return true;
        if( ReferenceEquals( lhs, null ) )
            return false;
        if( ReferenceEquals( rhs, null ) )
            return false;
        return lhs.Equals( rhs );
    }


    public static bool operator !=( ActorCollectionEntry lhs, ActorCollectionEntry rhs )
    {
        return !( lhs == rhs );
    }


    private static bool ActorEquals( Actor actor, ActorCollectionEntry entry )
    {
        if( ReferenceEquals( actor, null ) )
            return false;
        if( ReferenceEquals( entry, null ) )
            return false;
        if( entry.Actor == null )
            return false;
        return actor == entry.Actor;
    }


    public override bool Equals( object obj )
    {
        if( ReferenceEquals( obj, this ) )
            return true;
        if( obj is ActorCollectionEntry entry )
            return entry.Actor == this.Actor;
        if( obj is Actor actor )
            return actor == this;
        return false;
    }


    public override int GetHashCode()
    {
        return Actor.GetHashCode();
    }


    public bool Equals( ActorCollectionEntry other )
    {
        if( ReferenceEquals(other, (ActorCollectionEntry)null ) )
            return false;
        return this.Actor == other.Actor;
    }
}


[JsonObject]
public class ActorCollection: IEnumerable<ActorCollectionEntry>
{

    [JsonProperty]
    private List<ActorCollectionEntry> _entries = new List<ActorCollectionEntry>();


    public ActorCollectionEntry Add( Actor actor )
    {
        if( actor == null )
            throw new System.NullReferenceException( $"Argument '{nameof( actor )}' cannot be null" );

        ActorCollectionEntry newEntry = new ActorCollectionEntry( actor );
        if( _entries.Contains( newEntry ) )
            return null;
        _entries.Add( newEntry );
        return newEntry;
    }


    public bool Remove( Actor actor )
    {
        ActorCollectionEntry toBeRemoved = new ActorCollectionEntry( actor );
        return _entries.Remove( toBeRemoved );
    }


    public bool ContainsActor( Actor actor )
    {
        foreach( var entry in _entries )
        {
            if( entry.Actor == actor )
                return true;
        }
        return false;
    }


    public void Add( ActorCollectionEntry toAdd )
    {
        if( toAdd == default( ActorCollectionEntry ) )
            throw new System.NullReferenceException( $"Argument '{nameof( toAdd )}' cannot be null" );

        if( _entries.Contains( toAdd ) )
            return;
        _entries.Add( toAdd );
    }


    public void Remove( ActorCollectionEntry toBeRemoved )
    {
        _entries.Remove( toBeRemoved );
    }


    public IEnumerator<ActorCollectionEntry> GetEnumerator()
    {
        return _entries.GetEnumerator();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator) GetEnumerator();
    }

}
