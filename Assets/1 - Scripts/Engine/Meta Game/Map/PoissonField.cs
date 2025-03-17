using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Requires some code from save the light project

/*
public class PoissonField : MonoBehaviour 
{
    public VariablePoissonSampling PoissonSampling = new VariablePoissonSampling();

    public float minTriangleAngles = 5f;

    [OnValueChanged("ManualSeedChange")]
    public bool ManualSeed;

    [HideIf("HideSeed")]
    [OnValueChanged("OnSeedChange")]
    public int Seed;

    public bool ShowMesh;

    //One constraints where the vertices are connected to form the entire constraint
    public List<Vector3> constraints;

    //Constraints by using children to a parent, which we have to drag
    //Should be sorted counter-clock-wise
    public Transform hullConstraintParent;
    //Should be sorted clock-wise
    public List<Transform> holeConstraintParents;

    //The mesh so we can generate when we press a button and display it in DrawGizmos
    Mesh triangulatedMesh;
    List<VariablePoissonSampling.Point> points;


    public bool HideSeed()
    {
        return !ManualSeed;
    }


    public void ManualSeedChange()
    {
        if( !ManualSeed )
        {
            PoissonSampling.ClearSeed();
        }
        else
        {
            PoissonSampling.SetupSeed( Seed );
        }
    }


    public void OnSeedChange( )
    {
        Seed = Mathf.Max( 0, Seed );
        if( ManualSeed )
            PoissonSampling.SetupSeed( Seed );
    }


    public Mesh GenerateTriangulation()
    {
        PoissonSampling.CalculatePoints();
        points = PoissonSampling.GetPointCopy( new Rect() );

        //Hull
        List<Vector3> hullPoints = TestAlgorithmsHelpMethods.GetPointsFromParent(hullConstraintParent);

        List<MyVector2> hullPoints_2d = points.Select(x => x.Position.ToMyVector2()).ToList();
        HashSet<MyVector2> hullPoints_2d_hashSet = new HashSet<MyVector2>();
        foreach( var vec in hullPoints_2d )
            hullPoints_2d_hashSet.Add( vec );

        List<MyVector2> allPoints = new List<MyVector2>();
        allPoints.AddRange(hullPoints_2d);

        Normalizer2 normalizer = new Normalizer2(allPoints);

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        timer.Start();

        //NO HOLES FOR NOW
        HashSet<List<MyVector2>>  allHolePoints_2d_normalized = new HashSet<List<MyVector2>>();
        List<MyVector2> hullPoints_2d_normalized = new List<MyVector2>();
        //Algorithm 3. Constrained delaunay
        HalfEdgeData2 triangleData_normalized = _Delaunay.ConstrainedBySloan( hullPoints_2d_hashSet, hullPoints_2d_normalized, allHolePoints_2d_normalized, shouldRemoveTriangles: true, new HalfEdgeData2());

        timer.Stop();

        Debug.Log($"Generated a delaunay triangulation in {timer.ElapsedMilliseconds / 1000f} seconds");

        //UnNormalize
        HalfEdgeData2 triangleData = normalizer.UnNormalize(triangleData_normalized);

        //From half-edge to triangle
        HashSet<Triangle2> triangles_2d = _TransformBetweenDataStructures.HalfEdge2ToTriangle2(triangleData);
        triangles_2d = HelpMethods.OrientTrianglesClockwise(triangles_2d);

        //From 2d to 3d
        HashSet<Triangle3> triangles_3d = new HashSet<Triangle3>();
        foreach (Triangle2 t in triangles_2d)
        {
            triangles_3d.Add(new Triangle3(t.p1.ToMyVector3_Yis3D(), t.p2.ToMyVector3_Yis3D(), t.p3.ToMyVector3_Yis3D()));
        }

        triangulatedMesh = _TransformBetweenDataStructures.Triangle3ToCompressedMesh_Stripped( triangles_3d, minTriangleAngles );
        return triangulatedMesh;
    }



    private void DrawPoints()
    {
        if( points == null )
            return;
        foreach( var point in points )
        {
            Gizmos.DrawSphere( point.Position3, point.Radius );
        }
    }


    private void ShowMeshGizmo()
    {
        if( !ShowMesh )
            return;
        if( !triangulatedMesh )
            return;
        TestAlgorithmsHelpMethods.DisplayMeshWithRandomColors( triangulatedMesh, Seed );
    }


    private void OnDrawGizmos()
    {
        ShowMeshGizmo();
        DrawPoints();
    }



    private void DisplayDragConstraints()
    {
        if (hullConstraintParent != null)
        {
            List<Vector3> points = TestAlgorithmsHelpMethods.GetPointsFromParent(hullConstraintParent);

            TestAlgorithmsHelpMethods.DisplayConnectedPoints(points, Color.white, true);
        }

        if (holeConstraintParents != null)
        {
            foreach (Transform holeParent in holeConstraintParents)
            {
                List<Vector3> points = TestAlgorithmsHelpMethods.GetPointsFromParent(holeParent);

                TestAlgorithmsHelpMethods.DisplayConnectedPoints(points, Color.white, true);
            }
        }
    }
}
*/