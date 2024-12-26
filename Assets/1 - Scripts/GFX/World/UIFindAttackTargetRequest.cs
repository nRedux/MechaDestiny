using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using SuccessCallback = UIRequestSuccessCallback<object>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;
using UnityEditor.Localization.Plugins.XLIFF.V12;

public class UIFindAttackTargetRequest : UIRequest<object, bool>
{
    public BoolWindow Cells;
    private int[] _ignoredTeamIDs;
    private Actor _requestingActor;
    private Vector2Int _hoveredCell;
    public UIFindAttackTargetRequest( Actor requester, BoolWindow cells, SuccessCallback onSuccess, FailureCallback onFailure, CancelCallback onCancel): base( onSuccess, onFailure, onCancel, requester )
    {
        _requestingActor = requester;
        this.Cells = cells;
    }


    public override void Cleanup()
    {
        GameEngine.Instance.GfxBoard.ClearMoveOverlay();
        GameEngine.Instance.GfxBoard.UnHighlightCell( _hoveredCell );
        UIManager.Instance.AttackHoverInfo.Opt()?.Hide();
    }


    public override void Run()
    {
        if( _requestingActor.GetMechData().ActiveWeapon.IsAOE() )
        {
            TryGetUserSelectedCell_AOE();
        }
        else
        {
            TryGetUserSelectedCell( );
        }
    }


    public override void Start()
    {
        base.Start();

        GameEngine.Instance.GfxBoard.SetCellColor( GfxCellMode.Attack );
        GameEngine.Instance.GfxBoard.RenderCells( Cells, true );
    }


    protected override void OnCancelled()
    {
        GameEngine.Instance.GfxBoard.ClearMoveOverlay();
    }


    private void TryGetUserSelectedCell( )
    {
        if( Input.GetMouseButtonDown( 1 ) )
            this.Cancel();

        if( Input.GetMouseButtonDown( 0 ) )
        {
            Vector2Int result = new Vector2Int();
            var cellUnderMouse = UIManager.Instance.FindAttackableCellUnderMouse( ref result, Cells );
            if( cellUnderMouse )
            {
                //Get all actors at the cell
                //Discard any which should be ignored.
                var actor = UIManager.Instance.GetActorsAtCell( result ).Where( x => !ShouldIgnoreActor(x) ).FirstOrDefault();
                if( actor != null )
                    Succeed( actor );
            }
        }
    }

    private void TryGetUserSelectedCell_AOE()
    {
        if( Input.GetMouseButtonDown( 1 ) )
            this.Cancel();

        if( Input.GetMouseButtonDown( 0 ) )
        {
            Vector2Int result = new Vector2Int();
            var cellUnderMouse = UIManager.Instance.FindAttackableCellUnderMouse( ref result, Cells );
            if( cellUnderMouse )
            {
                Succeed( result );
            }
        }
    }

    /// <summary>
    /// Marks teams which should be invalid selections.
    /// </summary>
    /// <param name="teamIDs">Team IDs which should be rejected</param>
    public void MarkInvalidTeams( params int[] teamIDs )
    {
        _ignoredTeamIDs = teamIDs;
    }


    private bool ShouldIgnoreActor( Actor actor )
    {
        if( _ignoredTeamIDs == null )
            return false;
        return _ignoredTeamIDs.Count( x => x == actor.GetTeamID() ) > 0;
    }

    public override void CellHoverStart( Vector2Int cell )
    {
        _hoveredCell = cell;
        GameEngine.Instance.GfxBoard.HighlightCell( cell );
    }

    public override void CellHoverEnd( Vector2Int cell )
    {
        GameEngine.Instance.GfxBoard.UnHighlightCell( cell );
    }

    public override void ActorHoverStart( Actor hoveredActor )
    {
        if( ShouldIgnoreActor( hoveredActor ) )
            return;
        UIManager.Instance.ShowSideBMechInfo( hoveredActor, UIManager.MechInfoDisplayMode.Full );
        UIManager.Instance.AttackHoverInfo.Opt()?.Show( _requestingActor, hoveredActor );
    }

    public override void ActorHoverEnd( Actor hoveredActor )
    {
        UIManager.Instance.HideSideBMechInfo( );
        UIManager.Instance.AttackHoverInfo.Opt()?.Hide();
    }

}
