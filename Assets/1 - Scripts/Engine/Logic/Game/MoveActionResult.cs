using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;

public class MoveActionResult : ActionResult
{
    public Actor Actor;
    public ABPath ABPath = null;
    
    private GfxActor _avatar;
    private float _nodeSatisfiedDistance = .4f;

    private int _currentNode = 0;

    public int TargetNode
    {
        get
        {
            return _currentNode + 1;
        }
    }


    public MoveActionResult( Actor actor, ABPath path )
    {
        this.Actor = actor;
        this.ABPath = path;
    }


    private bool PathInvalid()
    {
        return ABPath == null || ABPath.CompleteState == PathCompleteState.Error || ABPath.CompleteState == PathCompleteState.Partial;
    }

    public override void Start()
    {
        if( PathInvalid() )
            return;

        _avatar = GameEngine.Instance.AvatarManager.GetAvatar( Actor );

        _avatar.transform.rotation = Quaternion.LookRotation( CurrentToTarget() );
        //_avatar.AnimatorMoveSpeed.Value = 1f;
        _avatar.AnimatorMoveSpeed.InterpolateValue( 1f, .4f );
        //_avatar.AnimatorMoveSpeed.Apply();

    }

    private Vector3 GetCurrentNodePosition()
    {
        return (Vector3) this.ABPath.path[_currentNode].position;
    }

    private Vector3 GetTargetNodePosition()
    {
        return (Vector3) this.ABPath.path[TargetNode].position;
    }

    Vector3 ToTargetNode()
    {
        return GetTargetNodePosition() - _avatar.transform.position;
    }

    Vector3 CurrentToTarget()
    {
        var currentNode = GetCurrentNodePosition();
        var targetNode = GetTargetNodePosition();
        return targetNode - currentNode;
    }


    Vector3 PositionOnPath( float offsetTowardTarget = 0f )
    {
        var curNodePos = GetCurrentNodePosition();
        Vector3 offsetFromCurrent = _avatar.transform.position - curNodePos;
        var toTarget = CurrentToTarget().normalized;

        return curNodePos + Vector3.Project( offsetFromCurrent, toTarget ) + toTarget * offsetTowardTarget;
    }


    private bool TargetIsPathEnd()
    {
        return TargetNode == this.ABPath.path.Count - 1;
    }


    public override ActionResultStatus Update()
    {
        //Empty path, we're done here
        if( PathInvalid() )
            return ActionResultStatus.Finished;

        Vector3 lookAtPos = PositionOnPath( _nodeSatisfiedDistance );
        Vector3 lookDir = lookAtPos - _avatar.transform.position;
        lookDir.y = 0f;

        Quaternion desiredRotation = Quaternion.LookRotation( lookDir, Vector3.up );// _avatar.TurnSpeed * Time.deltaTime );

        _avatar.transform.rotation = Quaternion.Slerp( _avatar.transform.rotation, desiredRotation, Time.deltaTime * 3.3f /** ( 1f - Mathf.Abs( Quaternion.Dot( _avatar.transform.rotation, desiredRotation ) )*/ );

        //Calculate actual stopping distance based on velocity
        //stopping dist == ( V * T ) / 2 
        //Use to determine proximity to begin stopping
        float velocity = _avatar.AnimatorMoveSpeed.Animator.velocity.magnitude;
        float stoppingTime = .3f;
        float stoppingDistance = ( velocity * stoppingTime ) / 2f;
        float stopDist = _avatar.AnimatorMoveSpeed.Animator.velocity.magnitude;
        Debug.Log( ToTargetNode().magnitude.ToString() + " : " + stoppingDistance.ToString() );

        if( TargetIsPathEnd() && ToTargetNode().magnitude < stoppingDistance )
        {
            _avatar.AnimatorMoveSpeed.InterpolateValue( 0f, stoppingTime );
            return ActionResultStatus.Finished;
        }
        else if( !TargetIsPathEnd() && ToTargetNode().magnitude < _nodeSatisfiedDistance ) // Close enough to node
        {
            _currentNode++;
        }

        for( int i = 0; i < ABPath.path.Count - 1; i++ )
        {
            Debug.DrawLine( (Vector3) ABPath.path[i].position, (Vector3) ABPath.path[i + 1].position );
        }

        return ActionResultStatus.Running;
    }

}
