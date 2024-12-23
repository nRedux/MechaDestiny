using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SuccessCallback = UIRequestSuccessCallback<UnityEngine.Vector2Int>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;
using Unity.VisualScripting;

public class UIFindMoveTargetRequest : UIRequest<Vector2Int, bool>
{

    public BoolWindow Cells;
    public System.Action<(bool hover, int cost, Vector2Int location)> OnCellHover;

    private bool _goodHover = false;
    private Actor _actor;
    private Vector2Int _hoveredCell;

    public UIFindMoveTargetRequest( Actor requester, BoolWindow cells, SuccessCallback onSuccess, FailureCallback onFailure = null, CancelCallback onCancel = null ): base( onSuccess, onFailure, onCancel, requester )
    {
        this._actor = requester;
        this.Cells = cells;
    }


    public override void Cleanup()
    {
        GameEngine.Instance.GfxBoard.UnHighlightCell( _hoveredCell );
        GameEngine.Instance.GfxBoard.ClearMoveOverlay();
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


    public override void Start()
    {
        base.Start();

        GameEngine.Instance.GfxBoard.SetCellColor( GfxCellMode.Move );
        GameEngine.Instance.GfxBoard.RenderCells( Cells, true );
    }


    protected override void OnCancelled()
    {
    }


    public override void CellHoverStart( Vector2Int cell )
    {
        int? distance = GameEngine.Instance.Board.GetDistance( _actor.Position, cell );
        OnCellHover?.Invoke( (hover: true, cost: distance ?? 0, location: cell) );
        _goodHover = true;
        _hoveredCell = cell;
        GameEngine.Instance.GfxBoard.HighlightCell( cell );
    }

    

    public override void CellHoverEnd( Vector2Int cell )
    {
        InvokeCellHoverEnd();
        _goodHover = false;
        GameEngine.Instance.GfxBoard.UnHighlightCell( cell );
    }


    private void TryGetUserSelectedCell( )
    {
        if( Input.GetMouseButtonDown( 1 ) )
            this.Cancel();

        if( Input.GetMouseButtonDown( 0 ) )
        {
            if( _goodHover )
                Succeed( _hoveredCell );
        }
    }
}
