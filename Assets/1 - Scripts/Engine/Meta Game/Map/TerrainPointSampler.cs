using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class TerrainPointSampler : MonoBehaviour
{
    NavMeshSurface nms;
    public float Size;
    public VariablePoissonSampling Sampler;
    private Rect _bounds;
    [SerializeField]
    private List<VariablePoissonSampling.Point> _points;
    private Terrain _terrain;
    private Vector3 _terrainSize;
    


    public List<VariablePoissonSampling.Point> GetSample()
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
            bool navMeshAllows = NavMesh.SamplePosition( x.Position3 + Vector3.up * 1f, out nmh, Size, NavMesh.AllAreas );
            return effectorsAllows && navMeshAllows;
        } ).ToList();
        return result;
    }


    [Button]
    public void Sample()
    {
        _points = GetSample();
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
            Gizmos.DrawWireSphere( point.Position3, point.Radius );
        }
    }

    public void RunPopulators( MapData mapData )
    {
        var pops = GetComponents<TerrainPopulator>();

        _points.Do( x =>
        {
            RaycastHit hit;
            if( GameUtils.RaycastGround( x.Position3, out hit ) )
            {
                pops.Do( y => y.ProcessSample( hit.point, mapData ) );
            }
        } );
    }

}
