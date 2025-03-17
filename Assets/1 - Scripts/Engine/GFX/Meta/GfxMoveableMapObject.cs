using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class GfxMoveableMapObject : GfxMapObject
{
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        GameManager.RaycastGround( Data.Position, out hit );
        transform.position = hit.point;

        if( Data.Heading != Vector3.zero )
            transform.forward = Data.Heading;
    }
}
