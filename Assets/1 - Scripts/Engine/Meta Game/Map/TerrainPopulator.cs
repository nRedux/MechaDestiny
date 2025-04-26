using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ITerrainPointPopulator
{
    void PopulatePoint( TerrainSamplerPoint point, MapData mapData );
}

public interface ITerrainCurvePopulator
{
    void PopulateCurve( Curve3D curve, MapData mapData );
}
