using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MoveActionResult : ActionResult
{
    public Actor Actor;
    public ABPath ABPath = null;
    
    private GfxActor _avatar;
    private float _nodeSatisfiedDistance = .5f;

    private int _curNodeIndex = 0;


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


        _avatar.transform.rotation = Quaternion.LookRotation( ToTargetNode() );
        //_avatar.AnimatorMoveSpeed.Value = 1f;
        _avatar.AnimatorMoveSpeed.InterpolateValue( 1f, .4f );
        //_avatar.AnimatorMoveSpeed.Apply();

    }

    Vector3 ToTargetNode()
    {
        var targNode = this.ABPath.path[_curNodeIndex];
        Vector3 targetWorldPos = (Vector3) targNode.position;
        Vector3 delta = targetWorldPos - _avatar.transform.position;
        return delta;
    }

    public override ActionResultStatus Update()
    {
        //Empty path, we're done here
        if( PathInvalid() )
            return ActionResultStatus.Finished;

        Vector3 delta = ToTargetNode();

        //Distance we have to travel to make our turn
        float turnDist = .25f * 2 * Mathf.PI * _nodeSatisfiedDistance;
        //How long will it take us to make the turn?
        float turnTime = turnDist / _avatar.GetLocalVelocity().z;
        //How fast do we need to turn per second to make our turn in time?
        float turnSpeed = ( 90f / turnTime ) * Time.deltaTime;

        _avatar.transform.rotation = Quaternion.RotateTowards( _avatar.transform.rotation, Quaternion.LookRotation( delta ), turnSpeed );// _avatar.TurnSpeed * Time.deltaTime );

        if( (_curNodeIndex < this.ABPath.path.Count && delta.magnitude < _nodeSatisfiedDistance ) || (_curNodeIndex == this.ABPath.path.Count - 1 && delta.magnitude < .02f ) )
        {
            //Next node!
            if( PathComplete() )
            {
                _avatar.AnimatorMoveSpeed.InterpolateValue( 0f, .3f );
                return ActionResultStatus.Finished;
            }
            else
            {
                _curNodeIndex++;
            }
        }

        for( int i = 0; i < ABPath.path.Count - 1; i++ )
        {
            Debug.DrawLine( (Vector3) ABPath.path[i].position, (Vector3) ABPath.path[i + 1].position );
        }

        return ActionResultStatus.Running;
    }


    /// <summary>
    /// Have we finished the path?
    /// </summary>
    /// <returns>True if path complete, otherwise false.</returns>
    private bool PathComplete()
    {
        Vector3 delta = ToTargetNode();
        //Moving toward last node
        return _curNodeIndex == ABPath.path.Count - 1 && delta.magnitude < .02f;
    }
}
