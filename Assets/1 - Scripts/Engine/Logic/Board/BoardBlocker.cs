using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardBlocker : MonoBehaviour
{
    public Bounds Bounds;

    public void ApplyToBoard( BoolWindow cells )
    {
        Vector3 center = transform.TransformPoint( Bounds.center );
        Vector3 extent = transform.TransformVector( Bounds.extents );
        cells.Do( iter => {
            if( CellContained( iter.world, center, extent ) )
            {
                iter.window[iter.local] = false;
            }
        } );
    }


    Vector3 MinVector(Vector3 a )
    {
        return new Vector3( Mathf.Min( -a.x, a.x ), Mathf.Min( -a.y, a.y ), Mathf.Min( -a.z, a.z ) );
    }

    Vector3 MaxVector( Vector3 a )
    {
        return new Vector3( Mathf.Max( -a.x, a.x ), Mathf.Max( -a.y, a.y ), Mathf.Max( -a.z, a.z ) );
    }


    private bool CellContained(Vector2Int cell, Vector3 center, Vector3 extent )
    {
        Vector3 min = center + MinVector( extent );
        Vector3 max = center + MaxVector( extent );
        Vector2 cellPos = new Vector2( cell.x + .5f, cell.y + .5f );
        return cellPos.x >= min.x && cellPos.x <= max.x && cellPos.y >= min.z && cellPos.y <= max.z;
    }

    private void OnDrawGizmos()
    {
        Color oldGizmosColor = Gizmos.color;

        Color WalkableColor = Color.red;
        WalkableColor.a = .8f;
        Gizmos.color = WalkableColor;


        Vector3 oldEuler = transform.eulerAngles;
        Vector3 euler = transform.eulerAngles;
        euler.z = euler.x = 0;
        transform.eulerAngles = euler;
        Gizmos.matrix = transform.localToWorldMatrix;
        transform.eulerAngles = oldEuler;

        Gizmos.DrawCube( Bounds.center, Bounds.size );
        Gizmos.matrix = Matrix4x4.identity;

        Gizmos.color = oldGizmosColor;
    }

    private Vector3 BoundsCenter()
    {
        Vector3 oldEuler = transform.eulerAngles;
        Vector3 euler = transform.eulerAngles;
        euler.y = euler.x = 0;
        transform.eulerAngles = euler;
        Vector3 pt =  transform.TransformPoint( Bounds.center );
        transform.eulerAngles = oldEuler;
        return pt;
    }
}
