using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class MapUIManager : Singleton<MapUIManager>
{
    public UIMapObjectActionView ActionView;
    public UIMechSelector MechSelector;

    protected override void Awake()
    {
        base.Awake();
        ActionView.Opt()?.Hide();
        CursorManager.Instance.Opt()?.SetCursorMode( CursorMode.Normal );
    }

}
