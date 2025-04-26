using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EntitySeriesPopulator : MonoBehaviour, ITerrainCurvePopulator
{

    public MapObjectReference[] MapObjects;

    [Range(0f, 1f)]
    [Tooltip("Minimum distance along the curve before first population is performed")]
    public float MinPopulationDistance;

    private MapObjectAsset[] _assets;

    private int _nextIndex = 0;


    /// <summary>
    /// Stores a copy of the entries in MapObjects which are valid addressable references.
    /// </summary>
    private MapObjectReference[] _validMapObjectRefs = null;

    private IEnumerator<MapObjectReference> _mapObjEnumerator;


    /// <summary>
    /// Cache valid entries in MapObjects for use during population
    /// </summary>
    private IEnumerable<MapObjectReference> GetMapObjectRefs()
    {
        return MapObjects.Where( x => x.RuntimeKeyIsValid() );
    }


    /// <summary>
    /// Get an asset from our map object references to use for population
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public MapObjectAsset GetAssetAtIndex( int index )
    {
        GetMapObjectRefs();

        if( _assets == null )
            _assets = new MapObjectAsset[_validMapObjectRefs.Length];

        //Load the asset if needed
        if( _assets[index] == null )
        {
            if( !_validMapObjectRefs[index].RuntimeKeyIsValid() )
                return null;
            _assets[index] = _validMapObjectRefs[index].GetAssetSync();
        }
        return _assets[index];
    }


    public MapObjectAsset GetNextMapObject()
    {
        GetMapObjectRefs();
        var current = _mapObjEnumerator.Current;
        return current.GetAssetSync();
    }


    public MapObjectData GetMapObjectdata()
    {
        var asset = GetNextMapObject();
        if( asset == null )
            return null;
        return asset.Opt()?.GetDataCopy();
    }


    private float GetPopulationRange()
    {
        return 1f - MinPopulationDistance;
    }

    public void PopulateCurve( Curve3D curve, MapData mapData )
    {
        var references = GetMapObjectRefs().ToList();
        int numMapObjRefs = references.Count();
        float populationRange = GetPopulationRange();
        float curveTimePerEvent = populationRange / (numMapObjRefs-1);

        for( int i = 0; i < numMapObjRefs; ++i )
        {
            var time = MinPopulationDistance + i * curveTimePerEvent;// + Random.value * ( curveTimePerEvent - curveTimePerEvent * .5f );
            //if( i == 0 )
            //    time = Mathf.Max( time, .04f + Random.value * .08f );
            RaycastHit hit;
            Vector3 curveSamplePos = curve.Evaluate( time );
            if( GameUtils.RaycastGround( curveSamplePos, out hit ) )
            {
                var instance = references[i].GetDataCopySync();
                if( instance == null )
                {
                    Debug.LogError( "Couldn't load asset in reference" );
                    continue;
                }
                instance.Position = hit.point;
                mapData.AddMapdata( instance );
            }
            else
            {
                Debug.LogError( $"{nameof(EntitySeriesPopulator)} Missed terrain." );
            }
        }


    }

}