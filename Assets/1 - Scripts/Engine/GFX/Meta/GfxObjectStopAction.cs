using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class GfxObjectStopAction : GfxMapObjectAction
{
   
    private MapObjActionSelectArgs _selectionArgs;
    private bool _requiresActivation = false;
    private GfxMoveableMapObject _moveable = null;

    public MapObjectData MapObjectData { 
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
        _moveable = MapObject as GfxMoveableMapObject;
    }

    public override bool WantsActivation( GfxMapObject mapObject, GfxMapObjectAction activeAction )
    {
        if( _moveable == null )
            return false;
        return Input.GetKeyDown( KeyCode.S );
    }


    public override void Activate()
    {
        
    }


    public override void Activate( object argument )
    {
        try
        {
            _moveable.StopMoving();
        }
        catch { }

    }


    public override object SelectedUpdate( GfxMapObject mapObject )
    {
        return true;
    }


    public override void SelectedStart( System.Action actionCancels, MapObjActionSelectArgs argument )
    {
        base.SelectedStart( actionCancels, argument );
    }

    public override void SelectedStop()
    {
    }

}
