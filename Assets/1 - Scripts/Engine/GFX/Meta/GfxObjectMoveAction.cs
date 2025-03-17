using System;
using System.IO;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class GfxObjectMoveAction : GfxMapObjectAction
{    
    private System.Action _cancelCallback;
    private MapObjActionSelectArgs _selectionArgs;
    private NavMeshPath _path;


    public MapObjectData Data { 
        get {
            try
            {
                return MapObject.Opt()?.GetData();
            }
            catch
            {
                return null;
            }
        }
    }


    public override void Awake()
    {
        base.Awake();
        _path = new NavMeshPath();
    }


    public override bool WantsActivation( GfxMapObject mapObject, GfxMapObjectAction activeAction )
    {
        if( activeAction != null )
            return false;
        return Input.GetMouseButtonDown( 1 );
    }


    public override void Activate()
    {
        
    }


    public override void Activate( object argument )
    {
        try
        {
            Vector3 destination = (Vector3) argument;
            Data.SetPath( destination, 0f, null, null );
        }
        catch( System.Exception ex )
        {
            Debug.LogException( ex );
        }

    }


    public override object SelectedUpdate( GfxMapObject mapObject )
    {

        if( EventSystem.current.IsPointerOverGameObject() )
            return null;

        if( _selectionArgs.UIClick )
        {
            if( Input.GetMouseButtonDown( 1 ) )
            {
                _cancelCallback?.Invoke();
                return null;
            }

            //Modify so which input we use, Left/Right, is decided at the time of being selected.
            //Same as how we will need a cursor change sometimes, but not other times. 
            if( Input.GetMouseButtonDown( 0 ) )
            {
                RaycastHit hit = new RaycastHit();
                if( GameManager.RaycastGroundFromMouse( out hit ) )
                {
                    return hit.point;
                }
            }
        }
        else
        {
            if( Input.GetMouseButtonDown( 1 ) )
            {
                RaycastHit hit = new RaycastHit();
                if( GameManager.RaycastGroundFromMouse( out hit ) )
                {
                    return hit.point;
                }
            }
        }

        return null;
    }


    public override void SelectedStart( System.Action actionCancels, MapObjActionSelectArgs argument )
    {
        base.SelectedStart( actionCancels, argument );
        _cancelCallback = actionCancels;
        this._selectionArgs = argument;
        if( _selectionArgs != null  ) {
            if( _selectionArgs.UIClick )
            {
                CursorManager.Instance.SetCursorMode( CursorMode.Move );
            }
        }
    }

    public override void SelectedStop()
    {
        
    }

}
