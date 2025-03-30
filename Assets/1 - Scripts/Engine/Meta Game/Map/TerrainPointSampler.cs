using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class TerrainSamplerPoint
{
    public Vector3 Position;
    public float Radius;
}

public class TerrainPointSampler : MonoBehaviour
{
    NavMeshSurface nms;
    [Tooltip("How close to the walkable space a sample must be to be considered.")]
    public float WalkableProximity;
    

    public VariablePoissonSampling Sampler;

    public List<TerrainSamplerPoint> Path;

    public AnimationCurve CurveX;
    public AnimationCurve CurveY;
    public AnimationCurve CurveZ;

    public float EventTimeVariation = .5f;


    private Rect _bounds;
    [SerializeField]
    private List<TerrainSamplerPoint> _points;
    private Terrain _terrain;
    private Vector3 _terrainSize;
    


    public List<TerrainSamplerPoint> GetSample()
    {
        _terrain = GetComponent<Terrain>();
        _terrainSize = _terrain.terrainData.size;
        _bounds = new Rect( new Vector2( _terrainSize.x * .5f, _terrainSize.z * .5f ), new Vector2( _terrainSize.x, _terrainSize.z ) );
        _bounds.center = new Vector2( _terrainSize.x * .5f, _terrainSize.z * .5f );
        Sampler.FieldWidth = _terrainSize.x;
        Sampler.FieldHeight = _terrainSize.z;
        Sampler.CalculatePoints();
        var samples = Sampler.GetPointCopy( _bounds );

        var effectors = TerrainPopulatorEffect.GetEffectors();

        List<VariablePoissonSampling.Point> result = new List<VariablePoissonSampling.Point>();
        NavMeshHit nmh;
        result = samples.Where( x => {
            bool effectorsAllows = !effectors.Blocks( x.Position3 );
            bool navMeshAllows = NavMesh.SamplePosition( x.Position3 + Vector3.up * 1f, out nmh, WalkableProximity, NavMesh.AllAreas );
            return effectorsAllows && navMeshAllows;
        } ).ToList();
        return result.Select(x=>new TerrainSamplerPoint() { Position = x.Position3, Radius = x.Radius } ).ToList();
    }


    [Button]
    public void Sample()
    {
        _points = GetSample();
    }



    [Button]
    public void BuildPath()
    {
        List<TerrainSamplerPoint> points = new List<TerrainSamplerPoint>( _points );
        Path = new List<TerrainSamplerPoint>();
        GfxWorld world = FindFirstObjectByType<GfxWorld>( );
        if( world == null )
            return;

        Path.Add( new TerrainSamplerPoint() { Position = world.StartPos.position } );
        TerrainSamplerPoint cur = GetNextPoint( points, world.TargetPos.position, world.StartPos.position );
        while( cur != null )
        {
            points.Remove( cur );
            Path.Add( cur );
            cur = GetNextPoint( points, world.TargetPos.position, cur.Position );
        }
        Path.Add( new TerrainSamplerPoint() { Position = world.TargetPos.position } );
    }

    public int DesiredPathLength = 7;
    public int MaxIterations = 1000;
    public float MaxTurningAngle = 200;
    [ReadOnly]
    public float TurningAngle = 0f;

    [Button]
    public void Build()
    {
        for( int i = 0; i < MaxIterations; i++ )
        {
            Sample();
            BuildPath();
            TurningAngle = GetPathTurningAngle();
            if( Path.Count == DesiredPathLength && TurningAngle < MaxTurningAngle )
                break;
        }
        BuildCurve();
    }

    private float GetPathTurningAngle()
    {
        if( Path == null )
            return 0f;
        float turnAngle = 0;
        for( int i = 2; i < Path.Count; i++ )
        {
            Vector3 v1 = Path[i-1].Position - Path[i - 2].Position;
            Vector3 v2 = Path[i].Position - Path[i - 1].Position;
            float angle = Vector3.Angle( v1, v2 );
            turnAngle += angle;
        }
        return turnAngle;
    }

    public List<TerrainSamplerPoint> GetPath()
    {
        return new List<TerrainSamplerPoint>( Path );
    }

    private void BuildCurve()
    {
        CurveX = new AnimationCurve();
        CurveY = new AnimationCurve();
        CurveZ = new AnimationCurve();
        int pathCount = Path.Count;
        for( int i = 0; i < pathCount; i++ )
        {
            float time = i / (float) ( pathCount - 1 );
            Vector3 pos = Path[i].Position;
            CurveX.AddKey( new Keyframe( time, pos.x ) );
            CurveY.AddKey( new Keyframe( time, pos.y ) );
            CurveZ.AddKey( new Keyframe( time, pos.z ) );
        }

        for( int i = 0; i < pathCount; i++ )
        {
            AnimationUtility.SetKeyLeftTangentMode( CurveX, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyRightTangentMode( CurveX, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyLeftTangentMode( CurveY, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyRightTangentMode( CurveY, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyLeftTangentMode( CurveZ, i, AnimationUtility.TangentMode.Auto );
            AnimationUtility.SetKeyRightTangentMode( CurveZ, i, AnimationUtility.TangentMode.Auto );
        }
    }

    private void RenderGizmosCurve()
    {
        GfxWorld world = FindFirstObjectByType<GfxWorld>();
        if( world == null )
            return;

        if( Path == null )
            return;
        
        var oldColor = Gizmos.color;
        Gizmos.color = Color.red;

        int res = 200;
        Vector3 lastPos = world.StartPos.position;
        for( int i = 0; i < res; i++ )
        {
            float time = i / (float)( res - 1);
            Vector3 pos = new Vector3( CurveX.Evaluate( time ), CurveY.Evaluate( time ), CurveZ.Evaluate( time ) );
            Gizmos.DrawLine( lastPos + Vector3.up * 40f, pos + Vector3.up * 40f );
            lastPos = pos;
        }
        Gizmos.color = oldColor;
    }

    private TerrainSamplerPoint GetNextPoint( List<TerrainSamplerPoint> points, Vector3 endPoint, Vector3 location )
    {
        Vector3 toEnd = endPoint - location;

        return points.Where( x =>
        {
            Vector3 toThis = x.Position - location;
            float angleOffGoal = Vector3.Angle( toEnd, toThis );
            return angleOffGoal < 90f && toThis.magnitude < toEnd.magnitude;
        } ).OrderBy( x =>
        {
            return Vector3.Distance( x.Position, location );
        } ).FirstOrDefault();
    }


    public void SampleIfNeeded()
    {
        _points ??= GetSample();
    }

    private void OnDrawGizmos()
    {
        if( _points == null )
            return;

        foreach( var point in _points )
        {
            Gizmos.DrawWireSphere( point.Position, point.Radius );
        }

        RenderGizmosCurve();
    }

    private Vector3 EvaluateCurve( float time )
    {
        Vector3 result = new Vector3( CurveX.Evaluate( time ), CurveY.Evaluate( time ), CurveZ.Evaluate( time ) );
        return result;
    }

    public void RunPopulators( MapData mapData )
    {
        var pops = GetComponents<TerrainPopulator>();

        int numPrimaryEvents = DesiredPathLength - 2;
        float curveTimePerEvent = 1f / numPrimaryEvents;
        for( int i = 0; i <= numPrimaryEvents; ++i )
        {
            var time = (i) * curveTimePerEvent + Random.value * ( curveTimePerEvent - curveTimePerEvent * .5f );
            if( i == 0 )
                time = Mathf.Max( time, .04f + Random.value * .08f );
            RaycastHit hit;
            if( GameUtils.RaycastGround( EvaluateCurve(time), out hit ) )
            {
                pops.Do( y => y.ProcessSample( hit.point, mapData ) );
            }
        }

        /*
        _points.Do( x =>
        {
            RaycastHit hit;
            if( GameUtils.RaycastGround( x.Position, out hit ) )
            {
                pops.Do( y => y.ProcessSample( hit.point, mapData ) );
            }
        } );
        */
    }
}
