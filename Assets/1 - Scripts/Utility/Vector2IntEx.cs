using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntEx
{

    public static int ManhattanDistance( this Vector2Int vec )
    {
        return Mathf.Abs( vec.x ) + Mathf.Abs( vec.y );
    }

    public static int ManhattanDistance( this Vector2Int vec, Vector2Int other )
    {
        return Mathf.Abs( vec.x - other.x ) + Mathf.Abs( vec.y - other.y );
    }

    public static Vector3 GetWorldPosition( this Vector2Int vec )
    {
        return new Vector3( vec.x + .5f, 0f, vec.y + .5f );
    }

}
