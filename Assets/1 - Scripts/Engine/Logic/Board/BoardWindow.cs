using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        set => Cells[GetIndex( x, y )] = value;
    }

    public TCellType this[Vector2Int coord]
    {
        get => Cells[GetIndex( coord.x, coord.y )];
        set => Cells[GetIndex( coord.x, coord.y )] = value;
    }

    public TCellType this[int index]
    {
        get => Cells[index];
        set => Cells[index] = value;
    }

    public Vector2Int Center => _center;

    public BoardWindow( int width, int height )
    {
        if( width % 2 == 0 )
            width += 1;
        if( height % 2 == 0 )
            height += 1;

        this.Width = width;
        this.Height = height;
        this.Size = width * height;
        this.Cells = new TCellType[Size];
        RecalculateCenter();
    }

    public BoardWindow( int sideSize )
    {
        if( sideSize % 2 == 0 )
            sideSize += 1;
        this.Width = sideSize;
        this.Height = sideSize;
        this.Size = sideSize * sideSize;
        this.Cells = new TCellType[Size];
        RecalculateCenter();
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
    /// Does the window contain the coordinate
    /// </summary>
    /// <param name="worldCoordinate"></param>
    /// <returns></returns>
    public bool ContainsWorldCoord( Vector2Int worldCoordinate )
    {
        return worldCoordinate.x >= _x && worldCoordinate.y >= _y && worldCoordinate.x < _x + Width && worldCoordinate.y < _y + Height;
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


    private Vector2Int GetLocalCoordinate( int index )
    {
        return GetArrayCoordinate( index, Width, Height );
    }


    public static Vector2Int GetArrayCoordinate( int index, int width, int height )
    {
        int dY = index / width;
        int dX = index - dY * height;
        return new Vector2Int( dX, dY );
    }


    private Vector2Int GetWorldCoordinate( int index )
    {
        Vector2Int coord = GetLocalCoordinate( index );
        coord.x = coord.x + _x;
        coord.y = coord.y + _y;
        return coord;
    }


    /// <summary>
    /// Gets the value in the window at a given world coordinate.
    /// </summary>
    /// <param name="worldCoord">The world coordinate to query a value from</param>
    /// <returns>The value within the window at the given world Coordinates</returns>
    public TCellType GetValueAtWorldCoord( Vector2Int worldCoord )
    {
        Vector2Int local = WorldToLocalIndex( worldCoord );
        return this[local];
    }


    public Vector2Int WorldToLocalIndex( Vector2Int worldCoord )
    {
        return new Vector2Int( worldCoord.x - X, worldCoord.y - Y );
    }


    private Vector2Int GetWorldCoordinate( int x, int y )
    {
        Vector2Int coord = new Vector2Int(x + _x, y + _y);
        return coord;
    }


    public Vector3 GetWorldPosition( int index )
    {
        Vector2Int coord = GetWorldCoordinate( index );
        return new Vector3( coord.x + .5f, 0f, coord.y + .5f );
    }


    public Vector3 GetWorldPosition( int x, int y )
    {
        Vector2Int coord = GetWorldCoordinate( x, y );
        return new Vector3( coord.x + .5f, 0f, coord.y + .5f );
    }


    public void MoveCenter( Vector2Int position )
    {
        this._x = position.x - Width / 2;
        this._y = position.y - Height / 2;
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
                this.Cells[GetIndex( x-_x, y-_y )] = selector( new Vector2Int(x, y), sourceCell );
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
                this.Cells[GetIndex( x-_x, y-_y )] = selector( new Vector2Int(x,y), sourceCell );
            }
        }
    }


    public void Do(System.Action<(Vector2Int local, Vector2Int world, TCellType value, BoardWindow<TCellType> window)> action )
    {
        for( int i = 0; i < Size; i++ )
        {
            Vector2Int world = GetWorldCoordinate( i );
            if( Board != null && !Board.IsCoordInBoard( world ) )
                continue;
            action?.Invoke( (local: GetLocalCoordinate(i), world: GetWorldCoordinate(i), value: Cells[i], window: this) );
        }
    }


    public void Do( System.Action<(Vector2Int local, Vector2Int world, TCellType value, BoardWindow<TCellType> window)> action, int range )
    {
        for( int i = 0; i < Size; i++ )
        {
            Vector2Int world = GetWorldCoordinate( i );
            Vector2Int toCenter = Center - world;
            if( toCenter.ManhattanDistance() > range )
                continue;
            if( Board != null && !Board.IsCoordInBoard( world ) )
                continue;
            action?.Invoke( (local: GetLocalCoordinate( i ), world: world, value: Cells[i], window: this) );
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
            Cells[i] = action.Invoke( GetLocalCoordinate( i ), GetWorldCoordinate( i ), this );
        }
    }


}

[System.Serializable]
public class BoolWindow : BoardWindow<bool>
{
    public BoolWindow( int width, int height ) : base( width, height ) { }

    public BoolWindow( int sideSize ) : base( sideSize ) { }

    public BoolWindow( int sideSize, Board board ) : base( sideSize ) { this.Board = board; }

    public override string CellToString( bool item )
    {
        return item == true ? "X" : "O";
    }
}

public class FloatWindow : BoardWindow<float>
{

    public string FormatString = "0.0";


    public FloatWindow( int width, int height ) : base( width, height ) { }

    public FloatWindow( int sideSize ) : base( sideSize ) { }

    public FloatWindow( int sideSize, Board board ) : base( sideSize ) { this.Board = board; }


    public override string CellToString( float item )
    {
        return item.ToString( FormatString );
    }
}
