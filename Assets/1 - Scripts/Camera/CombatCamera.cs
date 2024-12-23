using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class CombatCamera : MonoBehaviour
{

    [SerializeField]
    private float PanSpeed = 100;
    [SerializeField]
    private float RotationSpeed = 30;


    private float ZoomMin = 100;
    private float ZoomMax = 400;
    private float _height = 0;
    private Coroutine _relocationRoutine = null;
    private Actor _lockedTargetActor = null;


    private bool UserControlsActive
    {
        get
        {
            return GameEngine.Instance.IsPlayerTurn && 
                _relocationRoutine == null && 
                _lockedTargetActor == null;
        }
    }


    public Actor LockedTargetActor
    {
        set
        {
            _lockedTargetActor = value;
        }
    }


    public void Update()
    {
        if( !GameEngine.InstanceExists )
            return;

        ExecuteLockToActor();
        ExecuteUserControls();

        UpdateHeight();
    }


    private void ExecuteLockToActor()
    {
        if( _lockedTargetActor == null )
            return;
        var target = GameEngine.Instance.AvatarManager.GetAvatar( _lockedTargetActor );
        transform.position = target.transform.position;
    }


    private void ExecuteUserControls()
    {
        if( !UserControlsActive ) return;

        float x = Input.mousePosition.x;
        float y = Input.mousePosition.y;
        float z = Input.GetAxis( "Mouse ScrollWheel" );

        //Debug.Log( $"Camera input x:{x} y:{y}" );
        if( !RotationActive() && !EventSystem.current.IsPointerOverGameObject() )
        {
            PanScreen( x, y );
        }

        //Need to get the mouse horizontal and
        if( RotationActive() )
        {
            RotateCamera( Input.GetAxis( "Mouse X" ) );
        }
    }


    public void RotateCamera( float value )
    {
        transform.Rotate( Vector3.up * value * RotationSpeed * Time.deltaTime, Space.World );
    }


    public bool RotationActive()
    {
        return Input.GetMouseButton( 2 );
    }


    public void ZoomScreen( float increment )
    {
    }


    public void UpdateHeight()
    {
        float terrainHeight = 0;
        GetTerrainHeight( ref terrainHeight );

        Vector3 pos = transform.position;
        pos.y = terrainHeight;
        transform.position = pos;
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

        Vector3 forwardFlat = transform.forward;
        forwardFlat.y = 0;
        forwardFlat.Normalize();
        Vector3 rightFlat = transform.right;
        rightFlat.y = 0;
        rightFlat.Normalize();

        Vector3 finalDir = direction.x * rightFlat + direction.y * forwardFlat;

        Vector3 newPos = Vector3.Lerp( transform.position,
                                                transform.position + finalDir * PanSpeed,
                                                Time.deltaTime );

        Vector3 clampedPos = ClampVectorToBoard( newPos );
        transform.position = clampedPos;
    }


    private Vector3 ClampVectorToBoard( Vector3 position )
    {
        return new Vector3( 
            Mathf.Clamp( position.x, 0, GameEngine.Instance.Board.Width ), 
            position.y, 
            Mathf.Clamp( position.z, 0, GameEngine.Instance.Board.Height ) 
        );
    }


    public void MoveToLocation( Vector3 position, float duration )
    {
        CoroutineUtils.EndCoroutine( ref _relocationRoutine );
        _relocationRoutine = CoroutineUtils.DoTransformPositionLerp( transform, position, duration, Mathfx.EaseInOutQuint, onComplete: () => _relocationRoutine = null );
    }

    public void MoveToTransform( Transform destination, float duration )
    {
        CoroutineUtils.EndCoroutine( ref _relocationRoutine );
        _relocationRoutine = CoroutineUtils.DoTransformLerp( transform, destination, duration, Mathfx.EaseInOutQuint, onComplete: () => _relocationRoutine = null );
    }
}
