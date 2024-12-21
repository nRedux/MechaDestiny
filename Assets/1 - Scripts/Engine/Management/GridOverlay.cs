using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class GridOverlayException: System.Exception
{
    public GridOverlayException( string msg ): base( msg )
    {

    }
}

[System.Serializable]
public class GridOverlay
{
    public GameObject CellPrefab;
    public int NumCells;
    private List<GameObject> _available = new List<GameObject>(100);

    private Dictionary<Vector2, GameObject> _usedCells = new Dictionary<Vector2, GameObject>();

    private Material _cellPrefabMaterial;
    private Material _highlightCellMaterial;


    public void Initialize( Material highlightMaterial )
    {
        _highlightCellMaterial = highlightMaterial;
        var renderer = CellPrefab.Opt()?.GetComponentInChildren<Renderer>();
        _cellPrefabMaterial = renderer.Opt()?.sharedMaterial;

        for( int i = 0; i < NumCells; i++ )
        {
            var instance = GameObject.Instantiate<GameObject>( CellPrefab );
            instance.SetActive( false );
            _available.Add( instance );
        }
    }


    public void SetGridColor(Color color)
    {
        if( _cellPrefabMaterial == null )
            return;
        _cellPrefabMaterial.color = color;
    }


    public GameObject GetCell( Vector3 position )
    {
        if( _available.Count == 0 )
            throw new GridOverlayException( $"Too many overlay cells requested. Max cells: {NumCells}." );

        GameObject taken = _available[_available.Count - 1];
        _available.RemoveAt( _available.Count - 1 );
        taken.transform.position = position;
        _usedCells.Add( new Vector2Int((int)position.x, (int) position.z), taken );
        return taken;
    }


    public GameObject GetCellAtLocation( Vector2 location )
    {
        GameObject res = null;
        _usedCells.TryGetValue( location, out res );
        return res;
    }

    /*
    public void ReturnCell( GameObject cell )
    {
        if( !_used.Contains( cell ) )
            return;
        cell.SetActive( false );
        _used.Remove( cell );
        _available.Add( cell );
    }
    */
    public void HighlightCell( GameObject cell )
    {
        if( cell == null )
            return;
        var renderer = cell.GetComponentInChildren<Renderer>();
        if( renderer == null )
            return;
        renderer.sharedMaterial = _highlightCellMaterial;
    }

    public void UnHighlightCell( GameObject cell )
    {
        if( cell == null )
            return;
        var renderer = cell.GetComponentInChildren<Renderer>();
        if( renderer == null )
            return;
        renderer.sharedMaterial = _cellPrefabMaterial;
    }


    public void ReturnAllCells()
    {
        foreach( var cellEntry in _usedCells )
        {
            var cell = cellEntry.Value;
            UnHighlightCell( cell );
            cell.SetActive( false );
            _available.Add( cell );
        }
        _usedCells.Clear();
    }
}