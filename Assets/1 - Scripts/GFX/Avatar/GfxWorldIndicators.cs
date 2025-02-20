using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class InvalidIndicatorTypeException : System.Exception
{
    public InvalidIndicatorTypeException() : base() { }
    public InvalidIndicatorTypeException( string message ) : base( message ) { }
    public InvalidIndicatorTypeException( string message, System.Exception innerException ) : base( message, innerException ) { }
}


public class WorldIndicatorException : System.Exception
{
    public WorldIndicatorException() : base() { }
    public WorldIndicatorException( string message ) : base( message ) { }
    public WorldIndicatorException( string message, System.Exception innerException ) : base( message, innerException ) { }
}


public class WorldIndicatorInvalidLocationException : System.Exception
{
    public WorldIndicatorInvalidLocationException() : base() { }
    public WorldIndicatorInvalidLocationException( string message ) : base( message ) { }
    public WorldIndicatorInvalidLocationException( string message, System.Exception innerException ) : base( message, innerException ) { }
}


[System.Serializable]
public class GfxWorldIndicators
{
    public const string ATTACK_INDICATOR = "attack_indicator";

    public GfxWorldIndicator AttackIndicator;

    private Dictionary<Vector2Int, GfxWorldIndicator> _cellIndicators = new Dictionary<Vector2Int, GfxWorldIndicator>();
    private Dictionary<Actor, GfxWorldIndicator> _actorIndicators = new Dictionary<Actor, GfxWorldIndicator>();


    private GfxWorldIndicator FindIndicator(string indicatorType)
    {
        switch( indicatorType )
        {
            case ATTACK_INDICATOR:
                return AttackIndicator;
            default:
                throw new InvalidIndicatorTypeException( $"{indicatorType} is not a valid indicator type" );
        }
    }

    public GfxWorldIndicator TryCreateIndicatorOnCell( string indicatorType, Vector2Int cell )
    {
        try
        {
            return CreateIndicatorOnCell( indicatorType, cell );
        }
        catch( WorldIndicatorInvalidLocationException )
        {
            //Just catch it, nothing happens
            return null;
        }
    }


    public GfxWorldIndicator TryCreateIndicatorOnActor( string indicatorType, Actor actor )
    {
        try
        {
            return CreateIndicatorOnActor( indicatorType, actor );
        }
        catch( WorldIndicatorInvalidLocationException )
        {
            //Just catch it, nothing happens
            return null;
        }
    }


    public GfxWorldIndicator TryCreateIndicatorOnSmartPos( string indicatorType, SmartPoint point )
    {
        try
        {
            //We're going to try to find an actor, either in the smartpoint or at the location of the smartpoint.
            Actor actor = point.Actor;
            if( actor == null )
                actor = GameEngine.Instance.Board.GetActorAtCell( point.Position.ToVector2Int() );

            if( actor != null )
            {
                return TryCreateIndicatorOnActor( GfxWorldIndicators.ATTACK_INDICATOR, actor );
            }
            else
            {
                return TryCreateIndicatorOnCell( GfxWorldIndicators.ATTACK_INDICATOR, point.Position.ToVector2Int() );
            }
        }
        catch( WorldIndicatorInvalidLocationException )
        {
            //Just catch it, nothing happens
            return null;
        }
    }


    public GfxWorldIndicator CreateIndicatorOnCell( string indicatorType, Vector2Int cell )
    {
        GfxWorldIndicator indicatorPrefab = FindIndicator( indicatorType );

        if( _cellIndicators.ContainsKey( cell ) )
            throw new WorldIndicatorInvalidLocationException( $"Cannot create cell indicator, one already exists. Cell: {cell.ToString()}" );

        var actorAtCell = GameEngine.Instance.Board.GetActorAtCell( cell );
        if( actorAtCell != null )
            throw new WorldIndicatorInvalidLocationException( $"Cannot create a cell indicator on a cell which is occupied. Cell: {cell.ToString()}" );

        var instance = GameObject.Instantiate<GfxWorldIndicator>( indicatorPrefab );
        instance.transform.position = cell.GetWorldPosition();
        
        _cellIndicators.Add( cell, instance );

        return instance;
    }


    public GfxWorldIndicator CreateIndicatorOnActor( string indicatorType, Actor actor )
    {
        GfxWorldIndicator indicatorPrefab = FindIndicator( indicatorType );

        if( actor == null )
            throw new System.ArgumentNullException( $"{nameof( actor )} argument cannot be null." );

        if( _actorIndicators.ContainsKey( actor ) )
            throw new WorldIndicatorInvalidLocationException( $"Cannot create indicator for actor, one already exists. Actor position: {actor.Position.ToString()}" );

        GfxActor avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        if( avatar == null )
            throw new WorldIndicatorException( $"No avatar found for actor." );

        var instance = GameObject.Instantiate<GfxWorldIndicator>( indicatorPrefab );

        //Need to sort out how to attach it properly
        instance.transform.SetParent( avatar.transform );
        //Will need to adjust it's height
        instance.transform.localPosition = Vector3.up * avatar.Bounds.size.y;

        _actorIndicators.Add( actor, instance );
        return instance;
    }


    public void DestroyActorIndicators( bool afterHide )
    {
        if( afterHide )
            _actorIndicators.Do( x => x.Value.Hide( true ) );
        else
            _actorIndicators.Do( x => GameObject.Destroy( x.Value ) );
        _actorIndicators.Clear();
    }


    public bool DestroyIndicator(SmartPoint point, bool afterHide)
    {
        GfxWorldIndicator indicator = null;

        //We're going to try to find an actor, either in the smartpoint or at the location of the smartpoint.
        Actor actor = point.Actor;
        if( actor == null )
            actor = GameEngine.Instance.Board.GetActorAtCell( point.Position.ToVector2Int() );


        //Try find indicator
        if( actor != null && _actorIndicators.TryGetValue( actor, out indicator ) )
            _actorIndicators.Remove( actor );
        else if( _cellIndicators.TryGetValue( point.Position.ToVector2Int(), out indicator ) )
            _cellIndicators.Remove( point.Position.ToVector2Int() );

        //If exists, destroy
        if( indicator != null )
        {
            if( afterHide )
                indicator.Hide( true );
            else
                GameObject.Destroy( indicator.gameObject );
            return true;
        }

        return false;
    }


    public void DestroyCellIndicators( bool afterHide )
    {
        if( afterHide )
            _cellIndicators.Do( x => x.Value.Hide( true ) );
        else
            _cellIndicators.Do( x => GameObject.Destroy( x.Value ) );
        _cellIndicators.Clear();
    }

    public void DestroyAllIndicators( bool afterHide )
    {
        DestroyActorIndicators( afterHide );
        DestroyCellIndicators( afterHide );
    }
}

