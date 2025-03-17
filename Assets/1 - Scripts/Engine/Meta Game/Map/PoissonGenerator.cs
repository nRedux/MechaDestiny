using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PoissonGenerator : MonoBehaviour
{
    public VariablePoissonSampling sampler;

    List<VariablePoissonSampling.Point> points = new List<VariablePoissonSampling.Point>();

    [Button]
    public void Make()
    {
        sampler.CalculatePoints();
        points = sampler.GetPointCopy(new Rect());

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        if( points == null )
            return;
        foreach( var point in points )
        {
            Gizmos.DrawSphere(point.Position3, point.Radius);
        }
    }
}
