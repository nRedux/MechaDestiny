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
        GameEngine.Instance.GfxBoard.GeneralOverlay.Clear();
        GameEngine.Instance.GfxBoard.GeneralOverlay.UnHighlightCell( _hoveredCell );
        GameEngine.Instance.GfxBoard.AOEOverlay.Clear();
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

        GameEngine.Instance.GfxBoard.GeneralOverlay.SetCellColor( GfxCellMode.Attack );

        float mainTintShift = -.2f;
        GameEngine.Instance.GfxBoard.GeneralOverlay.RenderCells( Cells, true, tintShift: mainTintShift );
        GameEngine.Instance.GfxBoard.AOEOverlay.SetCellColor( GfxCellMode.Attack );
    }


    protected override void OnCancelled()
    {
        GameEngine.Instance.GfxBoard.GeneralOverlay.Clear();
        GameEngine.Instance.GfxBoard.AOEOverlay.Clear();
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
        GameEngine.Instance.GfxBoard.GeneralOverlay.HighlightCell( cell );

        RenderAOE( cell );
    }


    public override void CellHoverEnd( Vector2Int cell )
    {
        GameEngine.Instance.GfxBoard.GeneralOverlay.UnHighlightCell( cell );
        GameEngine.Instance.GfxBoard.AOEOverlay.Clear();
    }


    private void RenderAOE( Vector2Int cell ) 
    {
        if( !_requestingActor.GetMechData().ActiveWeapon.IsAOE() )
            return;

        var shape = _requestingActor.GetMechData().ActiveWeapon.AOEShape;
        BoolWindow win = shape.NewBoolWindow();
        win.MoveCenter( cell );
        GameEngine.Instance.GfxBoard.AOEOverlay.RenderCells( win, false, tintShift: .2f );
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
