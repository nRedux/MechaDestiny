using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.SocialPlatforms;

public class Board
{
    public GridStar Map = new GridStar( 10, 10 );

    public int Width => Map.Width;

    public int Height => Map.Height;

    private FloatWindow _scratchBoard;


    public Board( Board other )
    {
        this.Map = new GridStar( other.Map );
        _scratchBoard = new FloatWindow( other.Width, other.Height );
    }


    public Board( int width, int height )
    {
        Map = new GridStar( width, height );
        _scratchBoard = new FloatWindow( width, height );
    }



    /// <summary>
    /// Filters cells in a bool window by iterating through all cells. A ray is cast from a source position
    /// to each cell in the window which has a value of true. If the raycast hits something, we assume there is no line of sight.
    /// </summary>
    /// <param name="windowToPrune">The bool window of potential target locations.</param>
    /// <param name="losSource">The source of the line of sight query.</param>
    public static void LOS_PruneBoolWindow( BoolWindow windowToPrune, Vector2Int losSource )
    {
        Vector3 srcWorldPosition = losSource.WorldPosition() + Vector3.up;

        windowToPrune.Do( iter =>
        {
            if( !iter.value )
                return;
            if( LOS_CanSeeTo( losSource , iter.world ) )
                windowToPrune[iter.local] = false;
        } );
    }


    /// <summary>
    /// Check if we can raycast from a source tile to a destination tile
    /// </summary>
    /// <param name="source">The source tile.</param>
    /// <param name="dest">The destination tile</param>
    /// <returns>True if there isn't an object blocking the raycast. False if we are blocked.</returns>
    public static bool LOS_CanSeeTo( Vector2Int source, Vector2Int dest )
    {
        Vector3 srcWorldPosition = source.WorldPosition() + Vector3.up;
        Vector3 worldPos = dest.WorldPosition() + Vector3.up;
        Vector3 toCell = worldPos - srcWorldPosition;
        Ray r = new Ray( srcWorldPosition, toCell.normalized );
        return Physics.Raycast( r, toCell.magnitude, 1 << (int) Layers.Environment );
    }


    /// <summary>
    /// Get a utility field which is a gradient toward enemy positions.
    /// </summary>
    /// <param name="game">The game instance</param>
    /// <param name="actor">The actor which needs utility toward enemies</param>
    /// <returns>The utility field.</returns>
    public FloatWindow GetEnemyProximityUtility( Game game, Actor actor )
    {
        var otherTeams = game.GetOtherTeams( actor.GetTeamID() );
        ClearScratchBoard();

        otherTeams.Do( team =>
        {
            team.GetMembers().Do( member =>
            {
                _scratchBoard.Do( cell =>
                {
                    //How far to the other actor at this cell?
                    int gradient = cell.world.ManhattanDistance( member.Position );
                    float utility = 1f - (gradient / (float) (game.Board.Width + game.Board.Height));
                    _scratchBoard[cell.local] += utility;
                } );
            } );
        } );

        return _scratchBoard;
    }


    /// <summary>
    /// Get a utility field which is a gradient toward enemy positions.
    /// </summary>
    /// <param name="game">The game instance</param>
    /// <param name="actor">The actor which needs utility toward enemies</param>
    /// <returns>The utility field.</returns>
    public FloatWindow GetEnemyThreatValue( Game game, Actor actor )
    {
        var otherTeams = game.GetOtherTeams( actor.GetTeamID() );
        ClearScratchBoard();


        //Ugly deep nesting. Break up into multiple passes.
        otherTeams.Do( team =>
        {
            team.GetMembers().Do( member =>
            {
                _scratchBoard.Do( cell =>
                {
                    var attacks = member.GetActionsOfType<AttackAction>();

                    attacks.Do( action =>
                    {
                        var enemyAttacksHere = action.GetEffectUtility( game, member, cell.world );

                        _scratchBoard[cell.local] -= enemyAttacksHere;   
                    } );
                } );
            } );
        } );

        return _scratchBoard;
    }


    private void ClearScratchBoard()
    {
        _scratchBoard.Do( iter =>
        {
            _scratchBoard[iter.local] = 0f;
        } );
    }

    internal void SetWalkability( BoolWindow walkableCells )
    {
        walkableCells.Do( iter =>
        {
            Map.Grid[iter.local.x, iter.local.y].Walkable = iter.value;
        } );
    }

    public void GetCellsManhattan( int range, BoolWindow result )
    {
        result.Modify( ( cell, world, win ) => { 
            return Map.IsValidCoord( world ) && GetManhattanDistance( win.Center, world ) <= range; 
        } );
    }

    public int? GetDistance( Vector2Int start, Vector2Int end )
    {
        var path = Map.GetPath( start, end, true );
        if( path == null )
            return null;
        else
            return path.Count;
    }

    public void GetMovableCellsManhattan( int range, BoolWindow result )
    {
        result.Modify( ( cell, world, win ) => {
            var valid = Map.IsValidCoord( world ) && GetManhattanDistance( win.Center, world ) <= range;
            if( valid ) {
                var path = Map.GetPath( win.Center, world );
                if( path == null )
                    valid = false;
                else
                    valid = valid && path.Count <= range;
            }
            return valid;
        } );
    }



    /// <summary>
    /// Find all the cells which can be moved to 
    /// </summary>
    /// <param name="range"></param>
    /// <param name="result"></param>
    public void GetMovableCells( int range, BoolWindow result )
    {
        result.Modify( ( local, world, win ) => {
            var path = Map.GetPath( result.Center, local );
            return Map.IsValidCoord( world ) && GetManhattanDistance( win.Center, world ) <= range && path.Count <= range;
        } );
    }


    public BoolWindow GetCellsManhattan( int originX, int originY, int range )
    {
        BoolWindow result = new BoolWindow( range * 2 );
        result.X = originX - range;
        result.Y = originY - range;

        result.Do( cell => {
            result[cell.local] = Map.IsValidCoord( cell.world ) && GetManhattanDistance( cell.window.Center, cell.world ) <= range;
        } );

        return result;
    }


    public void DoOnCellsInRange( int originX, int originY, int range, System.Action<Vector2Int> onValidCell )
    {
        Vector2Int cell = new Vector2Int();

        for( int x = -range; x <= range; x++ )
        {
            for( int y = -range; y <= range; y++ )
            {
                int zeroX = x + range;
                int zeroY = y + range;

                int dX = originX + x;
                int dY = originY + y;

                if( Map.IsValidCoord( dX, dY ) && GetManhattanDistance( originX, originY, dX, dY ) <= range )
                {
                    cell.x = dX;
                    cell.y = dY;
                    onValidCell?.Invoke( cell );
                }
            }
        }
    }


    public static int GetManhattanDistance( int x, int y, int dX, int dY )
    {
        return Mathf.Abs( dX - x ) + Mathf.Abs( dY - y );
    }


    public static int GetManhattanDistance( Vector2Int a, Vector2Int b )
    {
        return Mathf.Abs( b.x - a.x ) + Mathf.Abs( b.y - a.y );
    }


    public bool IsCoordInBoard( int x, int y )
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }


    public bool IsCoordInBoard( Vector2Int coord )
    {
        return coord.x >= 0 && coord.y >= 0 && coord.x < Width && coord.y < Height;
    }


    public bool SetActorOccupiesCell( Vector2Int cell, bool occupied )
    {
        if( !IsCoordInBoard( cell ) )
            return false;
        //If we are occupying, we need to check if it's already occupied.
        if( occupied && !CanActorOccupyCell( cell ) )
            return false;
        Map.SetCellOccupied( cell, occupied );
        return true;
    }


    public bool CanActorOccupyCell( Vector2Int cell )
    {
        return Map.CellWalkable( cell );
    }

    public Stack<GridStarNode> GetPath(Vector2Int start, Vector2Int end )
    {
        return Map.GetPath( start, end );
    }

}
