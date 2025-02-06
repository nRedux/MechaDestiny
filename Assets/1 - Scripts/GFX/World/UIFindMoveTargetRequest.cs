using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SuccessCallback = UIRequestSuccessCallback<UnityEngine.Vector2Int>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;
using Unity.VisualScripting;
using static VariablePoissonSampling;
using System;

public class UIFindMoveTargetRequest : UIRequest<Vector2Int, bool>
{

    public BoolWindow MoveOptionCells;
    public System.Action<(bool hover, int cost, Vector2Int location)> OnCellHover;

    private bool _goodHover = false;
    private Actor _actor;
    private Vector2Int _hoveredCell;

    public UIFindMoveTargetRequest( Actor requester, BoolWindow cells, SuccessCallback onSuccess, FailureCallback onFailure = null, CancelCallback onCancel = null ): base( onSuccess, onFailure, onCancel, requester )
    {
        this._actor = requester;
        this.MoveOptionCells = cells;
    }


    public override void Cleanup()
    {
        GameEngine.Instance.GfxBoard.GeneralOverlay.UnHighlightCell( _hoveredCell );
        GameEngine.Instance.GfxBoard.GeneralOverlay.Clear();
        ShutdownInput();
        _goodHover = false;
        //Send callback for "hover ended"
        InvokeCellHoverEnd();
    }


    public override void Run()
    {
        TryGetUserSelectedCell( );
    }


    private void InvokeCellHoverEnd()
    {
        OnCellHover?.Invoke( (hover: false, cost: 0, location: Vector2Int.zero) );
    }


    private void SetupInput()
    {
        UIManager.Instance.UserControls.Cancel.AddActivateListener( OnCancelInput );
    }


    private void ShutdownInput()
    {
        UIManager.Instance.UserControls.Cancel.RemoveActivateListener( OnCancelInput );
    }

    private void OnCancelInput( InputActionEvent evt )
    {
        if( evt.Used )
            return;
        //evt.Use();
        Cancel();
    }

    public override void Start()
    {
        base.Start();
        SetupInput();
        GameEngine.Instance.GfxBoard.GeneralOverlay.SetCellColor( GfxCellMode.Move );
        GameEngine.Instance.GfxBoard.GeneralOverlay.RenderCells( MoveOptionCells, true );
    }


    protected override void OnCancelled()
    {
    }


    public override void CellHoverStart( Vector2Int cell )
    {
        //Is the cell within our movable cells?
        if( !MoveOptionCells.ContainsWorldCell( cell ) )
            return;
        //Is the cell part of the move options?
        Vector2Int local = MoveOptionCells.WorldToLocalCell( cell );
        if( !MoveOptionCells[local] )
            return;

        int? distance = GameEngine.Instance.Board.GetMovePathDistance( _actor.Position, cell, _actor );
        OnCellHover?.Invoke( (hover: true, cost: distance ?? -1, location: cell) );
        _goodHover = true;
        _hoveredCell = cell;
        GameEngine.Instance.GfxBoard.GeneralOverlay.HighlightCell( cell );
    }

    

    public override void CellHoverEnd( Vector2Int cell )
    {
        InvokeCellHoverEnd();
        _goodHover = false;
        GameEngine.Instance.GfxBoard.GeneralOverlay.UnHighlightCell( cell );
    }


    private void TryGetUserSelectedCell( )
    {

        if( Input.GetMouseButtonDown( 0 ) )
        {
            if( _goodHover )
                Succeed( _hoveredCell );
        }
    }
}
