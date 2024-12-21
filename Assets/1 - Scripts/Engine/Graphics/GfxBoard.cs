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

public class GfxBoard : SerializedMonoBehaviour
{

    public GridOverlay _moveOverlay;
    public GameEngine GameEngine => _gameEngine;

    public Material HighlightMaterial;
    public Color AttackCellColor;
    public Color MoveCellColor;

    private Board _board;
    private GridStarNode[,] _renderNodes = new GridStarNode[30, 30];
    private GameEngine _gameEngine;

    public void SetBoard( Board board )
    {
        this._board = board;
    }


    private void Awake()
    {
        _moveOverlay.Initialize( HighlightMaterial );
    }

    public void SetCellColor( GfxCellMode mode )
    {
        switch( mode )
        {
            case GfxCellMode.Move:
                _moveOverlay.SetGridColor( MoveCellColor );
                break;
            case GfxCellMode.Attack:
                _moveOverlay.SetGridColor( AttackCellColor );
                break;
        }
    }


    public void RenderCells( BoolWindow cells )
    {
        _moveOverlay.ReturnAllCells();
        cells.Do( iter => {
            if( iter.value == false )
                return;
            var renderCell = _moveOverlay.GetCell( cells.GetWorldPosition( iter.local.x, iter.local.y ) );
            renderCell.SetActive( true );
        } );
    }


    public void HighlightCell( Vector2 location )
    {
        var cell = _moveOverlay.GetCellAtLocation( location );
        _moveOverlay.HighlightCell( cell );
    }

    public void UnHighlightCell( Vector2 location )
    {
        var cell = _moveOverlay.GetCellAtLocation( location );
        _moveOverlay.UnHighlightCell( cell );
    }


    public void ClearMoveOverlay()
    {
        _moveOverlay.ReturnAllCells();
    }

    internal Stack<GridStarNode> GetPath( Vector2Int start, Vector2Int end )
    {
        return _board.Map.GetPath( start, end );
    }

}
