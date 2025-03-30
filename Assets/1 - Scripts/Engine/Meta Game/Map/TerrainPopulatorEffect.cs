using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UIElements;


public enum PopulatorEffectType
{
    Block
}

public interface IPrimitiveVolume
{
    public void OnDrawGizmos( Transform transform );

    public bool CanEffect( Transform thisTransform, Vector3 position );
}

[System.Serializable]
public class VolumeSphere : IPrimitiveVolume
{
    [Min(0f)]
    [Tooltip("Radius of the sphere")]
    public float Radius = 100;

    public bool CanEffect( Transform thisTransform, Vector3 location )
    {
        return Vector3.Distance( thisTransform.position, location ) <= Radius;
    }

    public void OnDrawGizmos( Transform transform )
    {
        Gizmos.DrawWireSphere( transform.position, Radius );
    }
}

[System.Serializable]
public class VolumeBox : IPrimitiveVolume
{
    public Vector3 Extent = new Vector3(100, 100, 100);

    public bool CanEffect( Transform thisTransform, Vector3 location )
    {

        Vector3 local = thisTransform.InverseTransformPoint( location );

        return Mathf.Abs( local.x ) <= Extent.x && Mathf.Abs( local.y ) <= Extent.y && Mathf.Abs( local.z ) <= Extent.z;

    }

    public void OnDrawGizmos( Transform transform )
    {
        var oldMat = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS( transform.position, transform.rotation, transform.lossyScale );
        Gizmos.DrawWireCube( Vector3.zero, Extent * 2 );
        Gizmos.matrix = oldMat;
    }
}


public class TerrainPopulatorEffect : MonoBehaviour
{
    [SerializeReference]
    public IPrimitiveVolume Shape = new VolumeSphere();

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
        if( Shape == null )
            return false;
        return Shape.CanEffect( transform, position );
    }

    private void OnDrawGizmos()
    {
        if( Shape == null )
            return;
        Shape.OnDrawGizmos( transform );
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
