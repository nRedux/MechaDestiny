using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.AI;

public class GfxMoveableMapObject : GfxMapObject
{

    private NavMeshPath _navPath;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if( HasValidPath() )
        {
            Move( Data.Speed, TimeManager.Instance.DayData.HoursDelta );
        }

        RaycastHit hit;
        GfxMap.RaycastGround( Data.Position, out hit );
        transform.position = hit.point;

        if( Data.Heading != Vector3.zero )
            transform.forward = Data.Heading;
    }

    public void StopMoving()
    {
        Path = null;
        ActionOnPathComplete = null;
    }

    private void Move( float speed, float time )
    {
        if( !HasValidPath() )
            return;
        Vector3 lastPos = Data.Position;
        Vector3 heading = Data.Heading;
        Data.Position = Path.MoveAlong( Data.Position, speed, time, ref heading );
        Data.Heading = heading;

        if( Path.IsComplete() )
        {
            DoPathCompleteAction();
            Path = null;
        }
    }

    public bool SetPath( Vector3 destination, float desiredProximity, MapObjectData targetOnComplete, System.Type actionOnArrive )
    {
        _navPath = _navPath ?? new NavMeshPath();
        this.TargetOnComplete = targetOnComplete;
        this.ActionOnPathComplete = actionOnArrive?.Name;
        if( NavMesh.CalculatePath( transform.position, destination, NavMesh.AllAreas, _navPath ) )
        {
            Path = new TravelPath( _navPath.corners, desiredProximity );
            UpdateHeading();
            return true;
        }

        return false;
    }


    internal void UpdateHeading()
    {
        if( !HasValidPath() )
            return;
        Vector3 heading = Data.Heading;
        Path.MoveAlong( Data.Position, 1, .1f, ref heading );
        Data.Heading = heading;
    }

    private void DoPathCompleteAction()
    {
        if( string.IsNullOrEmpty( ActionOnPathComplete ) )
            return;

        PathCompleteCallback?.Invoke( ActionOnPathComplete, TargetOnComplete );

        ActionOnPathComplete = null;
        TargetOnComplete = null;
    }
}
