using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class GridOverlayException: System.Exception
{
    public GridOverlayException( string msg ): base( msg )
    {

    }
}


public class GridCell
{
    public GameObject GameObject;
    public Renderer Renderer;
    public MaterialPropertyBlock PropertyBlock;
    
    private Color _color;
    private float _tint;
    private bool _highlight;

    public GridCell( GameObject gameObject )
    {
        this.GameObject = gameObject;
        this.Renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        this.PropertyBlock = new MaterialPropertyBlock();
        //this.PropertyBlock.SetColor( "_BaseColor", Color.red );
        this.Renderer.SetPropertyBlock( PropertyBlock );
    }


    public Color Color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            UpdateColor();
        }
    }

    public float Tint
    {
        get
        {
            return _tint;
        }
        set
        {
            _tint = value;
            UpdateColor();
        }
    }

    public bool Highlight
    {
        set
        {
            _highlight = value;
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        this.PropertyBlock.SetColor( "_BaseColor", _highlight ? Color.white : _color * _tint );
        this.Renderer.SetPropertyBlock( this.PropertyBlock );
    }


    public void SetActive( bool active )
    {
        GameObject.SetActive( active );
    }

}


[System.Serializable]
public class GridOverlay
{
    public GameObject CellPrefab;
    public int NumCells;

    private List<GridCell> _available = new List<GridCell>(100);
    private Dictionary<Vector2, GridCell> _usedCells = new Dictionary<Vector2, GridCell>();


    public void Initialize( Material highlightMaterial )
    {
        for( int i = 0; i < NumCells; i++ )
        {
            var instance = GameObject.Instantiate<GameObject>( CellPrefab );
            instance.SetActive( false );
            GridCell gridCell = new GridCell( instance ); 
            _available.Add( gridCell );
        }
    }


    public void SetGridColor(Color color)
    {
        _available.Do( x => x.Color = color );
        _usedCells.Do( x => x.Value.Color = color );
    }


    public GridCell GetCell( Vector3 position )
    {
        if( _available.Count == 0 )
            throw new GridOverlayException( $"Too many overlay cells requested. Max cells: {NumCells}." );

        GridCell taken = _available[_available.Count - 1];
        _available.RemoveAt( _available.Count - 1 );
        taken.GameObject.transform.position = position;
        _usedCells.Add( new Vector2Int((int)position.x, (int) position.z), taken );
        return taken;
    }


    public GridCell GetCellAtLocation( Vector2 location )
    {
        GridCell res = null;
        _usedCells.TryGetValue( location, out res );
        return res;
    }


    public void ReturnAllCells()
    {
        foreach( var cellEntry in _usedCells )
        {
            var cell = cellEntry.Value;
            cell.Highlight = false;
            cell.SetActive( false );
            _available.Add( cell );
        }
        _usedCells.Clear();
    }
}