using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using SuccessCallback = UIRequestSuccessCallback<object>;
using FailureCallback = UIRequestFailureCallback<bool>;
using CancelCallback = UIRequestCancelResult;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using System;

public class UIFindAttackTargetRequest : UIRequest<object, bool>
{
    public BoolWindow Cells;
    private int[] _ignoredTeamIDs;
    private Actor _requestingActor;
    private Vector2Int _hoveredCell = new Vector2Int( -1000, -1000 );


    public UIFindAttackTargetRequest( Actor requester, SuccessCallback onSuccess, FailureCallback onFailure, CancelCallback onCancel): base( onSuccess, onFailure, onCancel, requester )
    {
        _requestingActor = requester;
        this.Cells = CalculateCells( requester );
    }


    /// <summary>
    /// Calculate the cells which will be considered for this request.
    /// </summary>
    private BoolWindow CalculateCells( Actor actor )
    {
        BoolWindow resultCells = null;
        int range = 0;
        //Get the active weapon so we can use it's range
        MechData attackerMechData = actor.GetMechData();
        MechComponentData weapon = attackerMechData.ActiveWeapon;

        //TODO: will have to validate that the assets which define these are correct. Make finding these problems easy!
        range = weapon.GetStatisticValue( StatisticType.Range );
        resultCells = new BoolWindow( range * 2, GameEngine.Instance.Board );
        resultCells.MoveCenter( _requestingActor.Position );
        GameEngine.Instance.Board.GetCellsManhattan( range, resultCells );
        Board.LOS_PruneBoolWindow( resultCells, _requestingActor.Position );

        return resultCells;
    }


    public override void Cleanup()
    {
        ShutdownInput();
        GameEngine.Instance.GfxBoard.GeneralOverlay.Clear();
        GameEngine.Instance.GfxBoard.GeneralOverlay.UnHighlightCell( _hoveredCell );
        GameEngine.Instance.GfxBoard.AOEOverlay.Clear();
        UIManager.Instance.AttackHoverInfo.Opt()?.Hide();
    }


    public override void Run()
    {
        if( _requestingActor.ActiveWeapon.IsAOE() )
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
        SetupInput();
        Events.Instance.AddListener<ActiveWeaponChanged>( OnWeaponChanged );
        SetupGrid();
    }


    private void SetupGrid()
    {
        GameEngine.Instance.GfxBoard.GeneralOverlay.SetCellColor( GfxCellMode.Attack );

        float mainTintShift = -.2f;
        GameEngine.Instance.GfxBoard.GeneralOverlay.RenderCells( Cells, true, tintShift: mainTintShift );
        GameEngine.Instance.GfxBoard.AOEOverlay.SetCellColor( GfxCellMode.Attack );
    }
    private void OnWeaponChanged( ActiveWeaponChanged e )
    {
        this.Cells = CalculateCells( _requestingActor );
        SetupGrid();
    }


    protected override void OnCancelled()
    {
        GameEngine.Instance.GfxBoard.GeneralOverlay.Clear();
        GameEngine.Instance.GfxBoard.AOEOverlay.Clear();
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
        evt.Use();
        this.Cancel();
    }


    private void TryGetUserSelectedCell( )
    {
        if( Input.GetMouseButtonDown( 0 ) )
        {
            Vector2Int result = new Vector2Int();
            var cellUnderMouse = UIManager.Instance.FindAttackableCellUnderMouse( ref result, Cells );
            if( cellUnderMouse )
            {
                //Get all actors at the cell
                //Discard any which should be ignored.
                var actor = GameEngine.Instance.Board.GetActorsAtCell( result ).Where( x => !ShouldIgnoreActor(x) ).FirstOrDefault();
                if( actor != null )
                    Succeed( actor );
            }
        }
    }

    private void TryGetUserSelectedCell_AOE()
    {
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

    public override void CellHoverUpdate( Vector2Int cell )
    {
        if( cell != _hoveredCell )
        {
            CellHoverEnd( _hoveredCell );
            CellHoverStart( cell );
        }
        _hoveredCell = cell;
    }


    private void CellHoverStart( Vector2Int cell )
    {
        _hoveredCell = cell;
        GameEngine.Instance.GfxBoard.GeneralOverlay.HighlightCell( cell );

        RenderAOE( cell );
    }


    private void CellHoverEnd( Vector2Int cell )
    {
        GameEngine.Instance.GfxBoard.GeneralOverlay.UnHighlightCell( cell );
        GameEngine.Instance.GfxBoard.AOEOverlay.Clear();
    }


    private void RenderAOE( Vector2Int cell ) 
    {
        if( !_requestingActor.ActiveWeapon.IsAOE() )
            return;

        var shape = _requestingActor.ActiveWeapon.AOEShape;
        BoolWindow win = shape.NewBoolWindow( GameEngine.Instance.Game.Board );
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
