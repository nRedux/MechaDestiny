using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBoard : MonoBehaviour
{
    const float THREE_FOURTHS = 3f / 4f;
    const float THREE_SECONDS = 3f / 2f;

    public float CellSize => _cellSize;

    private float _cellSize;


    public static Vector2 FlatTopCellSpacing( float cellSize )
    {
        return new Vector2( FlatTopCellSpacingHorz( cellSize ), FlatTopCellSpacingVert( cellSize ) );
    }

    public static float FlatTopCellSpacingHorz( float cellSize )
    {
        return THREE_SECONDS * cellSize;
    }

    public static float FlatTopCellSpacingVert( float cellSize )
    {
        return Mathf.Sqrt(3f) * cellSize;
    }

    public static Vector2 PointTopCellSpacing( float cellSize )
    {
        return new Vector2( PointTopCellSpacingHorz( cellSize ), PointTopCellSpacingVert( cellSize ) );
    }

    public static float PointTopCellSpacingHorz( float cellSize )
    {
        return THREE_SECONDS * cellSize;
    }

    public static float PointTopCellSpacingVert( float cellSize )
    {
        return THREE_SECONDS * cellSize;
    }

    public static Vector2 FlatTopHexCorner( Vector2 center, float size, int i )
    {
        var angleDeg = 60 * i;
        float angleRad = Mathf.PI / 180 * angleDeg;
        return new Vector2( center.x + size * Mathf.Cos( angleRad ), center.y + size * Mathf.Sin( angleRad ) );
    }

    public static Vector2 PointTopHexCorner( Vector2 center, float size, int i )
    {
        var angleDeg = 60 * i - 30;
        float angleRad = Mathf.PI / 180 * angleDeg;
        return new Vector2( center.x + size * Mathf.Cos( angleRad ), center.y + size * Mathf.Sin( angleRad ) );
    }

}
