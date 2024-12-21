using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GfxCamera : MonoBehaviour
{
    public const string GROUND_LAYER = "Ground";

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    public RaycastHit? Raycast()
    {
        Ray r = _camera.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;
        if( Physics.Raycast( r, out hit, 1000, 1 << LayerMask.NameToLayer( GROUND_LAYER ) ) )
        {
            return hit;
        }
        else
        {
            return null;
        }
    }
}
