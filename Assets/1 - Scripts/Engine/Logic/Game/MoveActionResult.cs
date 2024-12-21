using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MoveActionResult : ActionResult
{
    public Actor Actor;
    public Stack<GridStarNode> Path;
    
    private GfxActor _avatar;
    private float _nodeSatisfiedDistance = .5f;


    public MoveActionResult( Actor actor, Stack<GridStarNode> path )
    {
        this.Actor = actor;
        this.Path = path;
    }

    private bool PathInvalid(Stack<GridStarNode> path)
    {
        return Path == null || Path.Count == 0;
    }

    public override void Start()
    {
        if( PathInvalid( Path ) )
            return;

        _avatar = GameEngine.Instance.AvatarManager.GetAvatar( Actor );


        _avatar.transform.rotation = Quaternion.LookRotation( ToTargetNode() );
        //_avatar.AnimatorMoveSpeed.Value = 1f;
        _avatar.AnimatorMoveSpeed.InterpolateValue( 1f, .4f );
        //_avatar.AnimatorMoveSpeed.Apply();

    }

    Vector3 ToTargetNode()
    {
        GridStarNode targetNode = this.Path.Peek();
        Vector3 targetWorldPos = targetNode.WorldPosition();
        Vector3 delta = targetWorldPos - _avatar.transform.position;
        return delta;
    }

    public override ActionResultStatus Update()
    {
        //Empty path, we're done here
        if( PathInvalid( Path ) )
            return ActionResultStatus.Finished;

        Vector3 delta = ToTargetNode();

        //Distance we have to travel to make our turn
        float turnDist = .25f * 2 * Mathf.PI * _nodeSatisfiedDistance;
        //How long will it take us to make the turn?
        float turnTime = turnDist / _avatar.GetLocalVelocity().z;
        //How fast do we need to turn per second to make our turn in time?
        float turnSpeed = ( 90f / turnTime ) * Time.deltaTime;

        _avatar.transform.rotation = Quaternion.RotateTowards( _avatar.transform.rotation, Quaternion.LookRotation( delta ), turnSpeed );// _avatar.TurnSpeed * Time.deltaTime );

        if( (this.Path.Count > 1 && delta.magnitude < _nodeSatisfiedDistance ) || ( this.Path.Count == 1 && delta.magnitude < .02 ) )
        {
            //Next node!
            if( PathComplete() )
            {
                _avatar.AnimatorMoveSpeed.InterpolateValue( 0f, .3f );
                return ActionResultStatus.Finished;
            }
            else
            {
                //Pop, so we move toward next node.
                this.Path.Pop();
            }
        }
        return ActionResultStatus.Running;
    }


    /// <summary>
    /// Have we finished the path?
    /// </summary>
    /// <returns>True if path complete, otherwise false.</returns>
    private bool PathComplete()
    {
        //Moving toward last node
        return Path.Count == 1;
    }
}
