using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class GfxObjectInteractAction : GfxMapObjectAction
{
    private System.Action _cancelCallback;
    private MapObjActionSelectArgs _selectionArgs;


    public MapObjectData Data
    {
        get
        {
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
    }

    protected virtual void PerformInteraction( object argument )
    {
        if( argument is GfxMapObject gfxObj )
        {
            Data.SetPath( gfxObj.Data.Position, 0f, gfxObj.Data, this.GetType() );
        }
    }

    public override bool WantsActivation( GfxMapObject mapObject, GfxMapObjectAction activeAction )
    {
        //Has objective flag
        if( mapObject != null && ( (int)mapObject.GetData().Type & (int)MapObjectType.Objective) != 0 )
        {
            return true;
        }

        return false;
    }


    public override void Activate()
    {

    }


    public override void Activate( object argument )
    {
 
        PerformInteraction( argument );
    }


    public override object SelectedUpdate( GfxMapObject mapObject )
    {

        if( EventSystem.current.IsPointerOverGameObject() )
            return null;

        if( _selectionArgs.UIClick )
        {
            //Modify so which input we use, Left/Right, is decided at the time of being selected.
            //Same as how we will need a cursor change sometimes, but not other times. 
            if( Input.GetMouseButtonDown( 1 ) )
            {
                _cancelCallback?.Invoke();
            }

            if( Input.GetMouseButtonDown( 0 ) )
            {
                if( mapObject == null )
                    return null;
                return mapObject;
            }
        }
        else
        {
            if( Input.GetMouseButtonDown( 1 ) )
            {
                if( mapObject == null )
                    return null ;
                return mapObject;
            }
        }

        return null;
    }


    public override void SelectedStart( System.Action actionCancels, MapObjActionSelectArgs argument )
    {
        base.SelectedStart( actionCancels, argument );
        CursorManager.Instance.SetCursorMode( CursorMode.Grab );

        _cancelCallback = actionCancels;
        this._selectionArgs = argument;
        /*
        if( _selectionArgs != null )
        {
            if( _selectionArgs.UIClick )
            {
                CursorManager.Instance.SetCursorMode( CursorMode.Grab );
            }
        }
        */
    }

    public override void SelectedStop()
    {
        CursorManager.Instance.SetCursorMode( CursorMode.Normal );
    }

    public override void ObjectHover( GfxMapObject obj )
    {
        if( obj != null && ( (int) obj.GetData().Type & (int) MapObjectType.Objective ) != 0 )
        {
            CursorManager.Instance.SetCursorMode( CursorMode.Grab );
        }
        else
        {
            CursorManager.Instance.SetCursorMode( CursorMode.Normal );
        }
    }

    public override void ActionAfterPathMove( MapObjectData target )
    {
        Debug.Log( "Doing interact action after move!" );


        //RunAction( target.GraphOnInteract );
        RunScript( target.ScriptOnInteract );

    }

    public async void RunAction( ScriptGraphAssetReference reference )
    {
        var thing = reference.LoadAssetAsync<ScriptGraphAsset>();
        await thing.Task;
        VisualScriptingUtility.RunGraph( thing.Task.Result, null );
    }

    public async void RunScript( TextAssetReference reference )
    {
        var thing = reference.LoadAssetAsync<TextAsset>();
        await thing.Task;
        MapObject.RunLuaBehavior( thing.Task.Result );
    }
}
