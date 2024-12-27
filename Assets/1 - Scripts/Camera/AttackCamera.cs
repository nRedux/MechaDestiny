using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class AttackCamera : MonoBehaviour
{

    public CinemachineCamera Camera;
    public CinemachineTargetGroup TargetGroup;

    public Transform ViewPositionsRoot;
    public Transform[] ViewPositions;

    private List<GameObject> _tempTargets = new List<GameObject>();


    private GameObject GetTempTarget( Vector3 position )
    {
        GameObject go = new GameObject( "TempCameraTarget" );
        _tempTargets.Add( go );
        go.transform.position = position;
        return go;
    }

    public void SetTargets( Transform attacker, SmartPoint target )
    {
        transform.rotation = Quaternion.identity;

        if( attacker == null || target == null )
            return;


        //Debug.Log( attacker.name + "   " + target.name );
        Vector3 attackDir = target.Position - attacker.position;
        //Debug.Log( attackDir );
        attackDir.y = 0f;
        attackDir = Quaternion.AngleAxis( 25, Vector3.up ) * attackDir;
        Camera.transform.forward = attackDir;
        Vector3 euler = Camera.transform.eulerAngles;
        euler.x = 20;
        Camera.transform.eulerAngles = euler;

        var targetObj = target.Transform?.gameObject ?? GetTempTarget( target.Position);

        TargetGroup.Targets = new List<CinemachineTargetGroup.Target>() {
            new CinemachineTargetGroup.Target() { Object = attacker, Radius = 1f, Weight = 1f },
            new CinemachineTargetGroup.Target() { Object = targetObj.transform, Radius = 1f, Weight = 1f }
        };


        //We attempt to find a better view angle now.
        Vector3 goodViewVec;
        if( FindGoodViewVector( targetObj.transform, attacker, out goodViewVec ) )
        {
            Camera.transform.forward = goodViewVec;
            //Debug.Log( "Found good view angle!" );
        }
        else
        {
            //Debug.LogError( "Found no good view angle." );
        }
    }

    internal void SetActive( bool active )
    {
        gameObject.SetActive( active );
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private bool FindGoodViewVector( Transform attackTarget, Transform attacker, out Vector3 goodLookVector )
    {

        Vector3 groupCenter = GetTargetGroupPosition();

        //Align viewing angles to target.
        Vector3 rootViewDir = groupCenter - transform.position;
        rootViewDir.y = 0f;

        ViewPositionsRoot.forward = rootViewDir;

        goodLookVector = Vector3.zero;
        //Make sure we can see the attack target, also make sure we can see the attacker from this angle.
        var goodViews = ViewPositions.Where( x => IsGoodViewAngle( x, attackTarget, .35f ) && IsGoodViewAngle( x, attacker, .35f ) );
        if( goodViews.Count() == 0 )
        {
            return false;
        }

        var goodLookSource = goodViews.Random();
        goodLookVector = groupCenter - goodLookSource.position;
        //Debug.Log( $"Using view angle {goodLookSource.name}" );
        return true;
    }

    private bool IsGoodViewAngle( Transform viewAngle, Transform target, float radius )
    {
        int layerMask = 1 << (int)Layers.Environment;
        Vector3 direction = target.position - viewAngle.position;
        Ray r = new Ray( target.position - direction.normalized * 1000, direction );
        bool result = !Physics.SphereCast( r, radius, 1000f, layerMask );

        Ray r2 = new Ray( target.position, -direction );
        bool result2 = !Physics.SphereCast( r, radius, direction.magnitude, layerMask );


        if( result || result2 )
        {
           // Debug.Log( $"Raycast from {viewAngle.name} didn't hit anything" );
        }
        else
        {
            //Debug.Log( $"Raycast from {viewAngle.name} hit something" );
        }

        return result; 
    }


    /// <summary>
    /// Get the center of the group. We just take the average position of members.
    /// </summary>
    /// <returns>The average position of all objects in the group</returns>
    private Vector3 GetTargetGroupPosition()
    {
        var pos = Vector3.zero;
        if( TargetGroup.Targets.Count == 0 )
            return TargetGroup.transform.position;
        TargetGroup.Targets.Do( x => pos += x.Object.position );
        pos /= TargetGroup.Targets.Count;
        return pos;
    }

    internal void ClearTempTargets()
    {
        _tempTargets.Do( x => Destroy( x ) );
        _tempTargets.Clear();
    }
}
