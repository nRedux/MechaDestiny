using UnityEngine;
using UnityEngine.UI;

public class UIGrid : MonoBehaviour
{
    public UIGridRow RowPrefab;

    private UIGridRow _lastRow = null;

    public void Add( Transform t )
    {
        if( _lastRow == null || !_lastRow.CanAddElement() )
            CreateRow();
        _lastRow.Add( t );
    }

    public void CreateRow()
    {
        if( RowPrefab == null )
            throw new System.Exception( $"{nameof(RowPrefab)} is null. Can't create rows." );

        var newRow = Instantiate<UIGridRow>( RowPrefab );
        newRow.gameObject.SetActive( true );
        newRow.transform.SetParent( transform, false );
        _lastRow = newRow;
    }

    public void DestroyRows()
    {
        transform.DestroyChildren();
    }

}
