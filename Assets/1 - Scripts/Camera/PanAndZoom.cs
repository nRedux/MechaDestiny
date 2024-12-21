using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.EventSystems;

public class PanAndZoom : MonoBehaviour
{

    [SerializeField]
    private float PanSpeed = 100;
    [SerializeField]
    private float RotationSpeed = 30;

    public Transform RootTransform;

    private CinemachineInputProvider InputProvider;
    public CinemachineVirtualCamera CloseCamera;
    public CinemachineVirtualCamera FarCamera;

    private float ZoomMin = 100;
    private float ZoomMax = 400;

    private float _height = 0;


    private void Awake()
    {
        
        InputProvider = GetComponent<CinemachineInputProvider>();

        FarCamera.enabled = false;

        if( RootTransform == null )
            RootTransform = transform;
    }


    public void Update()
    {
        float x = InputProvider.GetAxisValue( 0 );
        float y = InputProvider.GetAxisValue( 1 );
        float z = InputProvider.GetAxisValue( 2 );

        if( !RotationActive() && ( x != 0 || y != 0 ) && !EventSystem.current.IsPointerOverGameObject() )
        {
            PanScreen(x, y);
        }

        //Need to get the mouse horisontal and
        if( RotationActive() )
        {
            RotateCamera( Input.GetAxis("Mouse X") );
        }

        if( Input.GetMouseButtonDown(2) )
        {
            FarCamera.enabled = !FarCamera.enabled;
        }

        UpdateHeight();
    }


    public void RotateCamera( float value )
    {
        transform.Rotate( Vector3.up * value * RotationSpeed * Time.deltaTime, Space.World );
    }

    public bool RotationActive()
    {
        return Input.GetMouseButton( 1 );
    }


    public void ZoomScreen( float increment )
    {
        //float 
    }


    public void UpdateHeight()
    {
        float terrainHeight = 0;
        GetTerrainHeight( ref terrainHeight );

        Vector3 pos = RootTransform.position;
        pos.y = terrainHeight;
        RootTransform.position = pos;
    }


    private bool GetTerrainHeight( ref float result )
    {
        RaycastHit hit = new RaycastHit();
        Ray r = new Ray( transform.position, Vector3.down );
        if( Physics.Raycast( r, out hit, 5000, 1 << LayerMask.NameToLayer( "Ground" ) ) )
        {
            result = hit.point.y;
            return true;
        }

        return false;
    }


    public Vector2 PanDirection(float x, float y )
    {
        Vector2 direction = Vector2.zero;
        if( y >= Screen.height * .95f )
        {
            direction.y += 1;
        }
        else if( y <= Screen.height * .05f )
        {
            direction.y -= 1;
        }

        if( x >= Screen.width * .95f )
        {
            direction.x += 1;
        }
        else if( x <= Screen.width * .05f )
        {
            direction.x -= 1;
        }

        return direction;
    }


    public void PanScreen( float x, float y )
    {
        Vector2 direction = PanDirection( x, y );

        Vector3 forwardFlat = RootTransform.forward;
        forwardFlat.y = 0;
        forwardFlat.Normalize();
        Vector3 rightFlat = RootTransform.right;
        rightFlat.y = 0;
        rightFlat.Normalize();

        Vector3 finalDir = direction.x * rightFlat + direction.y * forwardFlat;

        RootTransform.position = Vector3.Lerp( RootTransform.position,
                                                RootTransform.position + finalDir * PanSpeed,
                                                Time.deltaTime );
    }

}
