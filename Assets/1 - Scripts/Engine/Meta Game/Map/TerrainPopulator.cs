using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Could break this down into interfaces

public interface ITerrainPointPopulator
{

}

public interface ITerrainCurvePopulator
{

}


public abstract class TerrainPopulator : MonoBehaviour
{

    public virtual void ProcessSample( Vector3 position, MapData data )
    {

    }

    public virtual void ProcessCurve( Curve3D curve )
    {

    }

}
