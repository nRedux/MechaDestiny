using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GfxTester : MonoBehaviour
{
    private List<Vector3> Points = new List<Vector3>();
    public int NumPoints = 10;
    public List<Transform> Empties = new List<Transform>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    [Button]
    public void TestPoints()
    {
        ClearPoints();

        Empties = GameUtils.CreateEmptiesOnVerticesRecursive( gameObject, NumPoints, .2f );
        var keepers = Empties.Random(NumPoints);
        Empties.Do( x =>
        {
            if( !keepers.Contains(x) )
            {
                if( Application.isPlaying )
                    Destroy( x.gameObject );
                else
                    DestroyImmediate( x.gameObject );
                x = null;
            }
        } );
        Empties = Empties.Where( x => x != null ).ToList();
    }

    [Button]
    public void ClearPoints()
    {
        if( Empties == null || Empties.Count == 0 )
            return;
        foreach( var empty in Empties )
        {
            if( empty == null )
                continue;
            if( Application.isPlaying )
                Destroy( empty.gameObject );
            else
                DestroyImmediate( empty.gameObject );
        }

        Empties.Clear();
    }


    private void OnDrawGizmos()
    {

    }
}
