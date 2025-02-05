using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Pseudo;

public enum BoardWindowClamping
{
    None,
    Positive,
    Negative
}

/// <summary>
/// Class which should help with performing operations on grids.
/// </summary>
/// <typeparam name="TCellType">The type of the cell</typeparam>
[System.Serializable]
public class BoardWindow<TCellType>
{
    [SerializeField]
    private int _x;
    [SerializeField]
    private int _y;
    [SerializeField]
    private Vector2Int _center = new Vector2Int();
    [SerializeField]
    public int Width;
    [SerializeField]
    public int Height;
    [SerializeField]
    public int Size;
    [SerializeField]
    public Board Board;
    [SerializeField]
    public TCellType[] Cells;
    [SerializeField]
    public int MaxIterDistance = int.MaxValue;
    [SerializeField]
    public BoardWindowClamping Clamping = BoardWindowClamping.None;


    public int X {
        get { return _x; }
        set { _x = value; RecalculateCenter(); }
    }

    public int Y
    {
        get { return _y; }
        set { _y = value; RecalculateCenter(); }
    }

    public TCellType this[int x, int y]
    {
        get => Cells[GetIndex( x, y )];
        set => Cells[GetIndex( x, y )] = Clamp( value );
    }

    public TCellType this[Vector2Int coord]
    {
        get => Cells[GetIndex( coord.x, coord.y )];
        set => Cells[GetIndex( coord.x, coord.y )] = Clamp( value );
    }

    public TCellType this[int index]
    {
        get => Cells[index];
        set => Cells[index] = Clamp( value );
    }

    public Vector2Int Center => _center;


    public BoardWindow( BoardWindow<TCellType> other )
    {
        _x = other._x;
        _y = other._y;
        _center = other._center;
        Width = other.Width;
        Height = other.Height;
        Size = other.Size;
        Board = other.Board;
        Cells = other.Cells.ToArray();

        this.Do( x => this[x.local] = Clamp( this[x.local] ) );
    }


    public BoardWindow( int width, int height, Board board )
    {
        if( width % 2 == 0 )
            width += 1;
        if( height % 2 == 0 )
            height += 1;

        this.Board = board;
        this.Width = width;
        this.Height = height;
        this.Size = width * height;
        this.Cells = new TCellType[Size];
        RecalculateCenter();
    }

    public BoardWindow( int width, int height, Board board, int maxIterDistance )
    {
        if( width % 2 == 0 )
            width += 1;
        if( height % 2 == 0 )
            height += 1;

        this.MaxIterDistance = maxIterDistance;
        this.Board = board;
        this.Width = width;
        this.Height = height;
        this.Size = width * height;
        this.Cells = new TCellType[Size];
        RecalculateCenter();
    }


    public BoardWindow( int sideSize, Board board )
    {
        if( sideSize % 2 == 0 )
            sideSize += 1;

        this.Board = board;
        this.Width = sideSize;
        this.Height = sideSize;
        this.Size = sideSize * sideSize;
        this.Cells = new TCellType[Size];
        RecalculateCenter();
    }

    public BoardWindow( int sideSize, Board board, int maxIterDistance )
    {
        if( sideSize % 2 == 0 )
            sideSize += 1;

        this.MaxIterDistance = maxIterDistance;
        this.Board = board;
        this.Width = sideSize;
        this.Height = sideSize;
        this.Size = sideSize * sideSize;
        this.Cells = new TCellType[Size];
        RecalculateCenter();
    }

    public virtual TCellType Clamp( TCellType value )
    {
        return value;
    }

    public BoardWindow( TCellType[] cells, int width, int height )
    {
        this.Width = width;
        this.Height = height;
        this.Size = height * width;
        this.Cells = cells;
        RecalculateCenter();
    }


    /// <summary>
    /// Does the window contain the cell which is in a world reference?
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    public bool ContainsWorldCell( Vector2Int world )
    {
        return world.x >= _x && world.y >= _y && world.x < _x + Width && world.y < _y + Height;
    }

    private void RecalculateCenter()
    {
        _center = new Vector2Int( _x + Width / 2, _y + Height / 2 );
    }


    private int GetIndex( int x, int y )
    {
        return GetIndex( x, y, this.Width );
    }


    public static int GetIndex( int x, int y, int width )
    {
        return y * width + x;
    }


    public static Vector2Int GetArrayIndex( int index, int width, int height )
    {
        int dY = index / width;
        int dX = index - dY * height;
        return new Vector2Int( dX, dY );
    }


    private Vector2Int IndexToLocalCell( int index )
    {
        return GetArrayIndex( index, Width, Height );
    }


    private Vector2Int IndexToWorldCell( int index )
    {
        Vector2Int coord = IndexToLocalCell( index );
        coord.x = coord.x + _x;
        coord.y = coord.y + _y;
        return coord;
    }


    /// <summary>
    /// Gets the value in the window at a given world coordinate.
    /// </summary>
    /// <param name="worldCell">The world coordinate to query a value from</param>
    /// <returns>The value within the window at the given world Coordinates</returns>
    public TCellType GetValueWorld( Vector2Int worldCell )
    {
        Vector2Int local = WorldToLocalCell( worldCell );
        return this[local];
    }


    public Vector2Int WorldToLocalCell( Vector2Int worldCell )
    {
        return new Vector2Int( worldCell.x - X, worldCell.y - Y );
    }


    private Vector2Int LocalToWorldCell( int x, int y )
    {
        Vector2Int coord = new Vector2Int(x + _x, y + _y);
        return coord;
    }

    private Vector2Int LocalToWorldCell( Vector2Int locelCell )
    {
        Vector2Int coord = new Vector2Int( locelCell.x + _x, locelCell.y + _y );
        return coord;
    }


    public Vector3 IndexToWorldPosition( int index )
    {
        Vector2Int coord = IndexToWorldCell( index );
        return new Vector3( coord.x + .5f, 0f, coord.y + .5f );
    }


    public Vector3 LocalToWorldPosition( int x, int y )
    {
        Vector2Int coord = LocalToWorldCell( x, y );
        return new Vector3( coord.x + .5f, 0f, coord.y + .5f );
    }

    public Vector3 LocalToWorldPosition( Vector2Int localCell )
    {
        Vector2Int coord = LocalToWorldCell( localCell );
        return new Vector3( coord.x + .5f, 0f, coord.y + .5f );
    }


    public void MoveCenter( Vector2Int cellPosition )
    {
        this._x = cellPosition.x - Width / 2;
        this._y = cellPosition.y - Height / 2;
        this.RecalculateCenter();
    }


    public void DebugCells()
    {
        string row = string.Empty;

        for( int y = Height - 1; y >= 0; y-- )
        {
            for( int x = 0; x < Width; x++ )
            {
                row += CellToString( this[x, y] ) + " ";
            }
            row += "\n";
        }
        Debug.Log( row );
    }


    public virtual string CellToString( TCellType item )
    {
        return item.ToString();
    }


    public void Sample<TDestType>( TDestType[,] source, System.Func<Vector2Int, TDestType, TCellType> selector, System.Action<Vector2Int> action )
    {
        if( action == null || selector == null )
            return;

        int sourceWidth = source.GetLength( 0 );
        int sourceHeight = source.GetLength( 1 );

        if( this._x >= sourceWidth || this._y >= sourceHeight )
            return;

        int xBound = Mathf.Min( sourceWidth, _x + this.Width );
        int yBound = Mathf.Min( sourceHeight, _y + this.Height );

        for( int x = _x; x < xBound; x++ )
        {
            for( int y = _y; y < yBound; y++ )
            {
                TDestType sourceCell = source[x, y];
                this.Cells[GetIndex( x-_x, y-_y )] = Clamp( selector( new Vector2Int(x, y), sourceCell ) );
            }
        }
    }


    public void Sample<TDestType>( TDestType[] source, int sourceWidth, System.Func<Vector2Int, TDestType, TCellType> selector, System.Action<Vector2Int> action )
    {
        if( action == null || selector == null )
            return;

        int sourceHeight = source.Length / sourceWidth;
        if( this._x >= sourceWidth || this._y >= sourceHeight )
            return;

        int xBound = Mathf.Min( sourceWidth, _x + this.Width );
        int yBound = Mathf.Min( sourceHeight, _y + this.Height );

        for( int x = _x; x < xBound; x++ )
        {
            for( int y = _y; y < yBound; y++ )
            {
                TDestType sourceCell = source[ GetIndex( x, y, sourceWidth ) ];
                this.Cells[GetIndex( x-_x, y-_y )] = Clamp( selector( new Vector2Int(x,y), sourceCell ) );
            }
        }
    }


    public void Do(System.Action<(Vector2Int local, Vector2Int world, TCellType value, BoardWindow<TCellType> window)> action )
    {
        for( int i = 0; i < Size; i++ )
        {
            Vector2Int world = IndexToWorldCell( i );
            Vector2Int toCenter = Center - world;
            if( Board != null && !Board.IsCoordInBoard( world ) )
                continue;
            if( toCenter.ManhattanDistance() > MaxIterDistance )
                continue;
            action?.Invoke( (local: IndexToLocalCell(i), world: IndexToWorldCell(i), value: Cells[i], window: this) );
        }
    }


    public void Do( System.Action<(Vector2Int local, Vector2Int world, TCellType value, BoardWindow<TCellType> window)> action, int range )
    {
        for( int i = 0; i < Size; i++ )
        {
            Vector2Int world = IndexToWorldCell( i );
            Vector2Int toCenter = Center - world;
            int toCenterDist = toCenter.ManhattanDistance();
            if( toCenterDist > range || toCenterDist > MaxIterDistance )
                continue;
            if( Board != null && !Board.IsCoordInBoard( world ) )
                continue;
            action?.Invoke( (local: IndexToLocalCell( i ), world: world, value: Cells[i], window: this) );
        }
    }


    public void Modify( System.Func<Vector2Int, Vector2Int, BoardWindow<TCellType>, TCellType> action )
    {
        if( action == null )
        {
            Debug.Log("Modify action argument null");
            return;
        }
        for( int i = 0; i < Size; i++ )
        {
            Cells[i] = Clamp( action.Invoke( IndexToLocalCell( i ), IndexToWorldCell( i ), this ) );
        }
    }


}

[System.Serializable]
public class BoolWindow : BoardWindow<bool>
{
    public BoolWindow( int width, int height, Board board ) : base( width, height, board ) {}

    public BoolWindow( int sideSize, Board board ) : base( sideSize, board ) {}

    public BoolWindow( BoolWindow other ) : base( other ) { }

    public override string CellToString( bool item )
    {
        return item == true ? "X" : "O";
    }


    internal void Fill( GridShape shape )
    {
        for( int x = 0; x < shape.Width; x++ )
        {
            for( int y = 0; y < shape.Height; y++ )
            {
                this[new Vector2Int( x, y )] = shape.Cells[x, y];
            }
        }
    }
}

public class FloatWindow : BoardWindow<float>
{

    public string FormatString = "0.0";


    public FloatWindow( int width, int height, Board board ) : base( width, height, board ) { this.Board = board; }

    public FloatWindow( int sideSize, Board board ) : base( sideSize, board ) { this.Board = board; }

    public FloatWindow( FloatWindow other ) : base( other ) { }

    public override string CellToString( float item )
    {
        return item.ToString( FormatString );
    }

    public override float Clamp( float value )
    {
        switch( Clamping )
        {
            case BoardWindowClamping.Positive:
                return Mathf.Clamp( value, 0, float.MaxValue );
            case BoardWindowClamping.Negative:
                return Mathf.Clamp( value, float.MinValue, 0f );
            default:
                return value;
        }
    }
}
