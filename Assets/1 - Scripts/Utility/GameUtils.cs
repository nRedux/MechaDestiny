using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class GameUtils
{
    public const float GROUND_RAYCAST_HEIGHT = 1000f;

    public static List<Vector3> GetRandomVertices( GameObject gameObject, int pointCount )
    {
        const int MAX_ITERATIONS = 100;
        List<Vector3> result = new List<Vector3>();
        Mesh mesh = null;

        MeshFilter mf = gameObject.GetComponent<MeshFilter>();
        if( mf != null && mf.sharedMesh != null )
            mesh = mf.sharedMesh;

        if( mesh == null )
        {
            SkinnedMeshRenderer smr = gameObject.GetComponent<SkinnedMeshRenderer>();
            if( smr != null && smr.sharedMesh != null )
                mesh = smr.sharedMesh;
        }

        if( mesh == null )
            return result;

        List<Vector3> vertOptions = mesh.vertices.ToList();

        //Get average position
        float avgMag = 0f;
        mesh.vertices.Do( x => avgMag += x.magnitude );
        avgMag /= mesh.vertices.Length;

        result.Add( vertOptions[0] );
        vertOptions.RemoveAt( 0 );
        Vector3 gameObjScale = gameObject.transform.localScale;
        //pointCount - 1 because we already took one above.
        for( int i = 0; i < pointCount - 1; i++ )
        {
            bool passComplete = false;
            float passMinSep = avgMag;
            int iterCount = 0;
            while( !passComplete && iterCount < MAX_ITERATIONS )
            {
                if( vertOptions.Count == 0 )
                    return result;

                var passOptions = vertOptions.Where( x => result.Where( y => Vector3.Distance( x, y ) >= passMinSep ).Count() > 0 );
                if( passOptions.Count() > 0 )
                {
                    var res = passOptions.Random();
                    vertOptions.Remove( res );
                    result.Add( res );// new Vector3( res.x * gameObjScale.x, res.y * gameObjScale.y, res.z * gameObjScale.z ) );
                    passComplete = true;
                }

                passMinSep -= passMinSep * .1f;
                iterCount++;
            }
        }

        return result;
    }

    public static List<Transform> CreateEmptiesOnVerticesRecursive( GameObject go, int pointCount, float minSeparation )
    {
        List<Transform> result = new List<Transform>();
        CreateEmptiesOnVerticesRecursive( go, pointCount, minSeparation, result );

        var keepers = result.Random( pointCount );
        result.Do( x =>
        {
            if( !keepers.Contains( x ) )
            {
                if( Application.isPlaying )
                    Object.Destroy( x.gameObject );
                else
                    Object.DestroyImmediate( x.gameObject );
                x = null;
            }
        } );
        result = result.Where( x => x != null ).ToList();
        return result;
    }

    public static void CreateEmptiesOnVerticesRecursive( GameObject go, int pointCount, float minSeparation, List<Transform> results )
    {
        if( results == null )
            results = new List<Transform>();

        var objResults = CreateEmptiesOnVertices( go, pointCount, minSeparation );
        if( objResults.Count > 0 )
            results.AddRange( objResults );

        for( int i = 0; i < go.transform.childCount; i++ )
        {
            CreateEmptiesOnVerticesRecursive( go.transform.GetChild(i).gameObject, pointCount, minSeparation, results );
        }
    }


    public static List<Transform> CreateEmptiesOnVertices( GameObject go, int pointCount, float minSeparation )
    {
        List<Transform> result = new List<Transform>();

        var vertices = GetRandomVertices( go, pointCount );
        SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
        if( smr == null )
            return result;

        var root = smr.rootBone;
        if( root == null )
            return result;

        for( int i = 0; i < vertices.Count; i++ )
        {
            GameObject newEmpty = new GameObject( $"empty_{i}" );

            newEmpty.transform.position = go.transform.TransformPoint( vertices[i] );
            newEmpty.transform.SetParent( root, true );

            result.Add( newEmpty.transform );
        }

        return result;
    }


    public static Vector3 GetWorldPositionForCell( int x, int y )
    {
        return new Vector3( x + .5f, 0, y + .5f );
    }


    public static Vector3 GetWorldPositionForCell( Vector2Int cell )
    {
        return new Vector3( cell.x + .5f, 0, cell.y + .5f );
    }

    public static bool RaycastGroundFromMouse( out RaycastHit hit )
    {
        var ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        return Physics.Raycast( ray, out hit, 1000, 1 << LayerMask.NameToLayer( "Ground" ) );
    }

    public static bool RaycastForSelectables( out RaycastHit hit )
    {
        var ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        return Physics.Raycast( ray, out hit, 1000, 1 << LayerMask.NameToLayer( "Selectable" ) );
    }


    public static bool RaycastGround( Vector3 position, out RaycastHit hit )
    {
        position.y = GROUND_RAYCAST_HEIGHT;
        var ray = new Ray( position, Vector3.down );
        return Physics.Raycast( ray, out hit, GROUND_RAYCAST_HEIGHT + 100, 1 << LayerMask.NameToLayer( "Ground" ) );
    }
}
