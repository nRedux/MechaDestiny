using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUIManager : Singleton<MapUIManager>
{
    public UIMapObjectActionView ActionView;

    protected override void Awake()
    {
        base.Awake();
        ActionView.Opt()?.Hide();
        CursorManager.Instance.Opt()?.SetCursorMode( CursorMode.Normal );
    }

}
