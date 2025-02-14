using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;


public class SmartPointException: System.Exception
{
    public SmartPointException() : base() { }
    public SmartPointException( string message ) : base( message ) { }
}


[System.Serializable]
public class SmartPoint
{
    
    public Vector3 Coordinate;

    private Transform _transform = null;

    [JsonProperty]
    private bool _hasTransform = false;

    
    [JsonIgnore]
    public Transform Transform {
        get => _transform;

        set
        {
            _hasTransform = value != null;
            _transform = value;
        }
    }

    public GfxActor GfxActor
    {
        get;
        private set;
    }


    public Actor Actor
    {
        get;
        private set;
    }

    public static implicit operator GfxActor( SmartPoint sp ) => sp.GfxActor;

    public override string ToString()
    {
        if( Transform != null )
            return $"Transform: {Transform.name}, Position: {Position}";
        else
            return $"Transform: None, Position: {Position}";
    }

    
    public Vector3 Position
    {
        get
        {
            if( HasTransform )
            {
                if( Transform == null )
                    return Coordinate;
                Coordinate = Transform.position;
                return Coordinate = Transform.position;
            }
            else
                return Coordinate;
        }
    }

    public bool HasTransform
    {
        get
        {
            return _hasTransform;
        }
    }

    [JsonConstructor]
    public SmartPoint() { }

    public SmartPoint( Vector3 position )
    {
        this.Coordinate = position;
        this.Transform = null;
    }

    public SmartPoint( Transform target )
    {
        this.Transform = target;
        if( target != null )
            this.Coordinate = target.position;
    }

    public SmartPoint( Actor target )
    {
        if( target == null )
            throw new SmartPointException( $"Argument {nameof( target )} cannot be null." );

        this.Actor = target;
        this.GfxActor = GameEngine.Instance.AvatarManager.GetAvatar( target );
        if( this.GfxActor == null )
            throw new SmartPointException( $"{nameof(SmartPoint)} No avatar found for actor in argument {nameof(target)}" );

        this.Transform = this.GfxActor.transform;
        this.Coordinate = this.GfxActor.transform.position;
    }


    [OnDeserialized]
    public void OnDeserialized( StreamingContext context )
    {
        if( HasTransform ) 
        {
            GameObject obj = new GameObject("SmartPoint Transform");
            this.Transform = obj.transform;
            this.Transform.position = this.Coordinate;
        }
    }

    [OnSerializing]
    public void OnSerializing(StreamingContext context )
    {
        if( HasTransform )
            Coordinate = Transform.position;
    }
}