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
using static UnityEditor.FilePathAttribute;
using static UnityEditor.PlayerSettings;

[System.Serializable]
public class TerrainSamplerPoint
{
    public Vector3 Position;
    public float Radius;

    //If used by a population action, should be set true.
    public bool UsedForPopulation = false;


    /// <summary>
    /// Tells the population system this isn't usable for population.
    /// </summary>
    /// <returns></returns>
    public void Use()
    {
        UsedForPopulation = true;
    }


    /// <summary>
    /// Was the point already used in a population routine?
    /// </summary>
    /// <returns></returns>
    public bool AlreadyUsed()
    {
        return UsedForPopulation;
    }
}


public class TerrainSamplerException: System.Exception
{
    public TerrainSamplerException(): base() { }
    public TerrainSamplerException( string msg) : base(msg) { }
    public TerrainSamplerException( string msg, System.Exception inner) : base(msg, inner) { }
}


public class TerrainPointSampler : MonoBehaviour
{
    NavMeshSurface nms;
    [Tooltip("How close to the walkable space a sample must be to be considered.")]
    public float WalkableProximity;
    
    public VariablePoissonSampling Sampler;

    public List<TerrainSamplerPoint> Path;

    public Curve3D Curve;

    public float EventTimeVariation = .5f;

    public int DesiredPathLength = 7;
    public int MaxIterations = 1000;
    public float MaxTurningAngle = 200;
    [ReadOnly]
    public float TurningAngle = 0f;

    private Rect _bounds;
    [SerializeField]
    private List<TerrainSamplerPoint> _points;
    private Terrain _terrain;
    private Vector3 _terrainSize;

    private void Start()
    {
        Events.Instance.AddListener<DoSceneWarmup>( OnSceneWarmup );
    }

    private void OnDestroy()
    {
        Events.Instance.RemoveListener<DoSceneWarmup>( OnSceneWarmup );
    }

    private void OnSceneWarmup( DoSceneWarmup e )
    {
        Build();
        RunPopulators( _points, RunManager.Instance.RunData.WorldMapData );
    }

    public List<TerrainSamplerPoint> GetSample()
    {
        _terrain = GetComponent<Terrain>();
        _terrainSize = _terrain.terrainData.size;
        _bounds = new Rect( new Vector2( _terrainSize.x * .5f, _terrainSize.z * .5f ), new Vector2( _terrainSize.x, _terrainSize.z ) );
        _bounds.center = new Vector2( _terrainSize.x * .5f, _terrainSize.z * .5f );
        Sampler.FieldWidth = _terrainSize.x;
        Sampler.FieldHeight = _terrainSize.z;
        Sampler.CalculatePoints();
        var samples = Sampler.GetPointCopy( _bounds ).Select( x =>
        {
            RaycastHit hit;
            if( GameUtils.RaycastGround( x.Position3, out hit ) )
            {
                return new TerrainSamplerPoint() { Position = hit.point, Radius = x.Radius };
            }
            else
                return new TerrainSamplerPoint() { Position = x.Position3, Radius = x.Radius } ;
        } ).ToArray();

        var effectors = TerrainPopulatorEffect.GetEffectors();

        List<TerrainSamplerPoint> result = new List<TerrainSamplerPoint>();
        NavMeshHit nmh;
        result = samples.Where( x => {
            bool effectorsAllows = !effectors.Blocks( x.Position );
            bool navMeshAllows = NavMesh.SamplePosition( x.Position, out nmh, WalkableProximity, NavMesh.AllAreas );
            return effectorsAllows && navMeshAllows;
        } ).ToList();
        return result;
    }


    [Button]
    public void Sample()
    {
        _points = GetSample();
    }


    [Button]
    public void BuildPath()
    {
        GfxWorld world = FindFirstObjectByType<GfxWorld>();
        if( world == null )
            return;


        //Create shallow copy of list so we can remove elements as we go.
        List<TerrainSamplerPoint> points = new List<TerrainSamplerPoint>( _points );
        
        Path = new List<TerrainSamplerPoint>();
        /*
        Path.Add( new TerrainSamplerPoint() { Position = world.StartPos.position } );
        TerrainSamplerPoint cur = GetNextPoint( points, world.TargetPos.position, world.StartPos.position );
        while( cur != null )
        {
            cur.Use();
            points.Remove( cur );
            Path.Add( cur );
            cur = GetNextPoint( points, world.TargetPos.position, cur.Position );
        }
        Path.Add( new TerrainSamplerPoint() { Position = world.TargetPos.position } );
        */

        Vector3 toEndFromStart = world.TargetPos.position - world.StartPos.position;

        Path = _points.Where( x =>
        {
            Vector3 posInverse = x.Position - world.StartPos.position;
            Vector3 projOnPath = Vector3.Project( posInverse, toEndFromStart );
            Vector3 offProj = projOnPath - posInverse;
            return offProj.magnitude < x.Radius;
        } ).OrderBy( x => Vector3.Distance( x.Position, world.StartPos.position)).ToList();

        Path.Do( x => x.Use() );

        //Path = Path.Random( Mathf.Min( Path.Count() / 2 ) );
        Path = Path.OrderBy( x => ( x.Position - world.StartPos.position ).magnitude ).ToList();

        Path.Insert( 0, new TerrainSamplerPoint() { Position = world.StartPos.position } );
        Path.Add( new TerrainSamplerPoint() { Position = world.TargetPos.position } );
        Vector3 lastPos = Path[0].Position;
        for( int i = 0; i < Path.Count; i++ )
        {

        }

    }


    [Button]
    public void Build()
    {
        
        for( int i = 0; i < MaxIterations; i++ )
        {
            Sample();
            BuildPath();
            TurningAngle = GetPathTurningAngle();
            if( TurningAngle < MaxTurningAngle )
                break;
        }

        //We only want the points in the kept path to be marked as used. Reset all points, mark points in path used.
        _points.Do( x => x.UsedForPopulation = false );
        Path.Do( x => x.Use() );

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
            turnAngle = Mathf.Max( turnAngle, angle );
        }

        return turnAngle;
    }


    private void BuildCurve()
    {
        Curve.ClearKeys();

        int pathCount = Path.Count;
        for( int i = 0; i < pathCount; i++ )
        {
            float time = i / (float) ( pathCount - 1 );
            Vector3 pos = Path[i].Position;
            Curve.AddKey( time, pos );
        }

        Curve.SmoothCurve();
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
            Vector3 pos = Curve.Evaluate( time );
            Gizmos.DrawLine( lastPos + Vector3.up * 40f, pos + Vector3.up * 40f );
            lastPos = pos;
        }
        Gizmos.color = oldColor;
    }


    private TerrainSamplerPoint GetNextPoint( List<TerrainSamplerPoint> points, Vector3 endPoint, Vector3 location )
    {
        GfxWorld world = FindFirstObjectByType<GfxWorld>();
        if( world == null )
            throw new TerrainSamplerException( "GfxWorld must exist in scene" );

        Vector3 toEndFromStart = world.TargetPos.position - world.StartPos.position;

        Vector3 toEndFromLocation = world.TargetPos.position - location;

        var ordered = points.Where( x =>
        {
            //Everything closer to end than me
            Vector3 toEndFromThis = world.TargetPos.position - x.Position;
            return toEndFromThis.magnitude < toEndFromLocation.magnitude;
        } ).OrderBy( x =>
        {
            //nearest to me
            return ( location - x.Position ).magnitude;
        } );
           
        var nearest3 = ordered.Take( Mathf.Min( ordered.Count(), 3 ) );
        return nearest3.FirstOrDefault();
        /*
        return nearest3.OrderBy( x =>
        {
            Vector3 offCenter = Vector3.Project( x.Position, toEndFromStart ) - x.Position;

            return offCenter.magnitude;
        } ).FirstOrDefault();
        */
        /*
        return points.Where( x =>
        {
            Vector3 toThis = x.Position - location;
            float angleOffGoal = Vector3.Angle( toEnd, toThis );
            return angleOffGoal < 90f && toThis.magnitude < toEnd.magnitude;
        } ).OrderBy( x =>
        {
            return Vector3.Distance( x.Position, location );
            //return Vector3.Distance( x.Position, location );
        } ).FirstOrDefault();
        */
    }


    private void OnDrawGizmos()
    {
        if( !this.enabled )
            return;
        if( _points == null )
            return;

        foreach( var point in _points )
        {
            Gizmos.DrawWireSphere( point.Position, point.Radius );
        }

        RenderGizmosCurve();
    }


    public void RunPopulators( List<TerrainSamplerPoint> points, MapData mapData )
    {
        //Run curve populators
        var curvePopulators = this.GetInterfaceComponents<ITerrainCurvePopulator>();
        curvePopulators.Do( x => x.PopulateCurve( Curve, mapData ) );
        

        //Run point populators
        var pointPopulators = this.GetInterfaceComponents<ITerrainPointPopulator>();
        //Iterate all points, run populator against all points.
        points.Do( x => {
            if( x.AlreadyUsed() )
                return;
            pointPopulators.Do( pop => pop.PopulatePoint( x, mapData ) );
        } );
    }
}
