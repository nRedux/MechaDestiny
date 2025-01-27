using UnityEngine;
using Pathfinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static VariablePoissonSampling;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;

public class Board
{

    public AstarPath Pathing = null;
    public GridGraph Graph;
    

    public int Width => Graph.Width;

    public int Height => Graph.depth;

    private FloatWindow _scratchBoard;

    private List<SingleNodeBlocker> _freeNodeBlockers = new List<SingleNodeBlocker>();
    private Dictionary<Vector2Int, SingleNodeBlocker> _nodeBlockers = new Dictionary<Vector2Int, SingleNodeBlocker>();

    [JsonConstructor]
    public Board()
    {

    }

    public Board( Board other )
    {
        Pathing = Object.FindFirstObjectByType<AstarPath>( FindObjectsInactive.Exclude );
        _scratchBoard = new FloatWindow( other.Width, other.Height, this );
    }


    public Board( AstarPath astarPath )
    {
        this.Pathing = astarPath;
        Graph = Pathing.graphs.FirstOrDefault() as GridGraph;
        if( astarPath == null )
            throw new System.Exception( "You must have a GridGraph in the scene!" );

        if( astarPath.graphs.Length == 0 )
            throw new System.Exception( "You must have a Grid graph in scene AstarPath" );

        var graph = Pathing.graphs[0] as GridGraph;
        _scratchBoard = new FloatWindow( graph.width, graph.depth, this );
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
                //TODO: This is UGLY and we don't know for sure if there is an active weapon - really need a more defined process for knowing there will be
                var mech = member.GetMechData();
                if( mech.ActiveWeapon == null )
                    return;
                var range = mech.ActiveWeapon.GetStatistic( StatisticType.Range );

                _scratchBoard.Do( cell =>
                {
                    var attacks = member.GetActionsOfType<AttackAction>();

                    attacks.Do( action =>
                    {
                        var enemyAttacksHere = action.GetUtilityAtLocation( game, member, cell.world, range.Value );

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

    /// <summary>
    /// Can be replaced with the get constant path function from the pathing API
    /// </summary>
    /// <param name="range"></param>
    /// <param name="result"></param>
    public void GetCellsManhattan( int range, BoolWindow result )
    {
        result.Modify( ( cell, world, win ) => { 
            return IsCoordinateInMap( world ) && GetManhattanDistance( win.Center, world ) <= range; 
        } );
    }

    public int? GetDistance( Vector2Int start, Vector2Int end )
    {
        var path = GetNewPath( start, end, true );
        if( path == null )
            return null;
        else
            return path.path.Count;
    }

    public bool IsCoordinateInMap( Vector2Int coord )
    {
        if( Graph == null )
            return false;

        return Graph.bounds.Contains( coord.WorldPosition() );
    }

    public void GetMovableCellsManhattan( int range, BoolWindow result )
    {
        result.Modify( ( cell, world, win ) => {
            var valid = IsCoordinateInMap( world ) && GetManhattanDistance( win.Center, world ) <= range;
            if( valid ) {
                var path = GetNewPath( win.Center, world );
                if( path == null )
                    valid = false;
                else
                    valid = valid && path.path.Count <= range;
            }
            return valid;
        } );
    }


    public BoolWindow GetCellsManhattan( int originX, int originY, int range )
    {
        BoolWindow result = new BoolWindow( range * 2, this );
        result.X = originX - range;
        result.Y = originY - range;

        result.Do( cell => {
            result[cell.local] = IsCoordinateInMap( cell.world ) && GetManhattanDistance( cell.window.Center, cell.world ) <= range;
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

                Vector2Int coord = new Vector2Int( originX + x, originY + y );
                int dX = originX + x;
                int dY = originY + y;

                if( Pathing.graphs[0] is GridGraph graph )
                {
                    graph.IsInsideBounds( new Vector3( dX, 0, dY ) );
                }
                if( IsCoordinateInMap( coord ) && GetManhattanDistance( originX, originY, dX, dY ) <= range )
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


    private SingleNodeBlocker GetNodeBlocker()
    {
        //Find existing instance
        if( _freeNodeBlockers.Count > 0 )
        {
            var lastIndex = _freeNodeBlockers.Count - 1;
            var free = _freeNodeBlockers[lastIndex];
            _freeNodeBlockers.RemoveAt( lastIndex );
            return free;
        }

        //Create a new instance and return it
        GameObject newBlocker = new GameObject( "Node Blocker" );
        var blockerComponent = newBlocker.AddComponent<SingleNodeBlocker>();
        blockerComponent.manager = GameEngine.Instance.BlockManager;
        _freeNodeBlockers.Add( blockerComponent );
        return blockerComponent;
    }

    public bool SetActorOccupiesCell( Vector2Int cell, bool occupied )
    {
        if( !IsCoordInBoard( cell ) )
            return false;
        //If we are occupying, we need to check if it's already occupied.
        if( occupied && !CanActorOccupyCell( cell ) )
            return false;
        //TODO: Has to be replaced with blocking

        if( occupied )
            BlockNode( cell );
        else
            UnblockNode( cell );
        return true;
    }

    public void BlockNode( Vector2Int location )
    {
        if( !_nodeBlockers.ContainsKey( location ) )
        {
            var blocker = GetNodeBlocker();
            blocker.transform.position = location.WorldPosition();
            blocker.BlockAtCurrentPosition();
        }
    }   

    private void UnblockNode( Vector2Int location )
    {
        SingleNodeBlocker blocker = null;
        if( _nodeBlockers.TryGetValue( location, out blocker ) )
        {
            blocker.Unblock();
            _nodeBlockers.Remove( location );
            _freeNodeBlockers.Add( blocker );
        }
    }


    public bool CanActorOccupyCell( Vector2Int cell )
    {
        /*
        if( GameManager.Instance.BlockManager == null )
            return false;
        Vector3 worldPosition = cell.WorldPosition();
        var node = AstarPath.active.GetNearest( worldPosition, NNConstraint.None ).node;
        */
        return !_nodeBlockers.ContainsKey( cell );
    }

    public ABPath GetNewPath( Vector2Int start, Vector2Int end, bool allowOccupied = false )
    {
        var path = ABPath.Construct( start.WorldPosition(), end.WorldPosition() );
        AstarPath.StartPath( path );
        path.BlockUntilCalculated();

        return path;
    }

}
