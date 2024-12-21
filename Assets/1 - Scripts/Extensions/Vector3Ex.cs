using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Ex
{
    public static Vector3Int ToInt( this Vector3 vec )
    {
        return new Vector3Int( Mathf.FloorToInt( vec.x ), Mathf.FloorToInt( vec.y ), Mathf.FloorToInt( vec.z ) );
    }
}
