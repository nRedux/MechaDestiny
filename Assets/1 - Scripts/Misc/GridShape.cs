using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu( fileName = "New Room Grid", menuName = "Engine/Room Shape")]
public class GridShape: SerializedScriptableObject
{
    [OnValueChanged("OnDimChanged")]
    public int Width = 4;
    [OnValueChanged( "OnDimChanged" )]
    public int Height = 4;

    [TableMatrix( HorizontalTitle = "Transposed Custom Cell Drawing", DrawElementMethod = "DrawColoredEnumElement", ResizableColumns = false, RowHeight = 12, Transpose = true, SquareCells = true )]
    public bool[,] Cells = new bool[4, 4];

    private static bool DrawColoredEnumElement( Rect rect, bool value )
    {
        if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
        {
            value = !value;
            GUI.changed = true;
            Event.current.Use();
        }

#if UNITY_EDITOR
        UnityEditor.EditorGUI.DrawRect( rect, value ? new Color( 0.1f, 0.8f, 0.2f ) : new Color( 0, 0, 0, 0.5f ) );
#endif
        return value;
    }

    public void OnDimChanged()
    {
        Cells = new bool[Width, Height];
    }
}