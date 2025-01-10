using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum GfxCellMode
{
    Move,
    Attack
}

public enum GridOverlays
{
    Move,
    AOE
}

public class GfxBoard : SerializedMonoBehaviour
{

    public GridOverlay _generalOverlay;
    public GridOverlay _AOEOverlay;

    public Color AttackCellColor;
    public Color MoveCellColor;

    private Board _board;


    public GridOverlay GeneralOverlay { get => _generalOverlay; }
    
    public GridOverlay AOEOverlay { get => _AOEOverlay; }


    public void SetBoard( Board board )
    {
        this._board = board;
    }


    private void Awake()
    {
        _generalOverlay.Initialize( );
        _AOEOverlay.Initialize();
    }

}
