using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ITerrainPointPopulator
{
    void ProcessSample( TerrainSamplerPoint point, MapData mapData );
}

public interface ITerrainCurvePopulator
{
    void ProcessCurve( Curve3D curve, MapData mapData );
}
