using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class GfxObjectStopAction : GfxMapObjectAction
{
   
    private MapObjActionSelectArgs _selectionArgs;
    private bool _requiresActivation = false;

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


    public override bool WantsActivation( GfxMapObject mapObject, GfxMapObjectAction activeAction )
    {
        return Input.GetKeyDown( KeyCode.S );
    }


    public override void Activate()
    {
        
    }


    public override void Activate( object argument )
    {
        try
        {
            MapObject.StopMoving();
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
