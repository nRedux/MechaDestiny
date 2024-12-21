using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;


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
                return Transform.position;
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