using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapObjectActionComparer : IEqualityComparer<GfxMapObjectAction>
{
    public bool Equals( GfxMapObjectAction x, GfxMapObjectAction y )
    {
        return x.GetType() == y.GetType();
    }

    public int GetHashCode( GfxMapObjectAction obj )
    {
        return obj.GetHashCode();
    }
}

public class MapObjActionSelectArgs
{
    public bool UIClick = false;
}


public abstract class GfxMapObjectAction : MonoBehaviour
{
    public Sprite BtnImage;

    public bool Disable;

    public GfxMapObject MapObject { get; private set; } = null;

    public MapObjActionSelectArgs SelectArgs;


    public virtual void Awake()
    {
        MapObject = GetComponentInParent<GfxMapObject>();
    }

    public virtual bool CanActivate()
    {
        return !Disable;
    }

    public virtual bool WantsActivation( GfxMapObject hovered, GfxMapObjectAction activeAction )
    {
        return false;
    }

    public virtual void SelectedStart( System.Action actionCancels, MapObjActionSelectArgs argument )
    {
        SelectArgs = argument;
    }


    public abstract object SelectedUpdate( GfxMapObject hovered );


    public abstract void Activate();


    public abstract void Activate( object argument );

    public abstract void SelectedStop();

    public virtual void ObjectHover( GfxMapObject obj )
    {

    }

    public virtual void ActionAfterPathMove( MapObjectData target )
    {

    }
}
