using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Pipeline;
using UnityEngine;


public enum PopulatorEffectType
{
    Block
}


public class TerrainPopulatorEffect : MonoBehaviour
{
    public PopulatorEffectType Effect;
    public float Radius;

    public static PopulatorEffectorCollection GetEffectors()
    {
        var effectors = Object.FindObjectsByType<TerrainPopulatorEffect>( FindObjectsSortMode.None ).ToList();
        var result = new PopulatorEffectorCollection( effectors );
        return result;
    }

    public bool CanEffect(Vector3 position )
    {
        return Vector3.Distance( position, transform.position ) < Radius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere( transform.position, Radius );
    }
}


public class PopulatorEffectorCollection : List<TerrainPopulatorEffect>
{
    public bool Blocks( Vector3 position )
    {
        return this.IfAny( x => x.CanEffect( position ) && x.Effect == PopulatorEffectType.Block );
    }

    public PopulatorEffectorCollection( IEnumerable<TerrainPopulatorEffect> collection ): base( collection )
    {
       
    }
}
