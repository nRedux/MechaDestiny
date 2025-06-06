using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using static PlayerAttackAction;

public abstract class AttackAction : ActorAction
{
    public ResultDisplayProps DisplayProps { get; set; }

    [HideInInspector]
    [NonSerialized]
    public MechComponentData UsedWeapon;
    [HideInInspector]
    [NonSerialized]
    public SmartPoint Target;


    public override void Start( Game game, Actor actor )
    {
        
    }


    public override void End()
    {
        base.End();
    }

    public override int APCost
    {
        get
        {
            if( UsedWeapon != null )
            {
                return UsedWeapon.GetStatisticValue( StatisticType.AbilityPointCost );
            }
            return 0;
        }
    }


    public override CanStartActionResult AllowedToExecute( Actor actor )
    {
        var baseRes = base.AllowedToExecute( actor );
        if( baseRes != CanStartActionResult.Success )
            return baseRes;

        //Any valid weapon?
        var mechData = actor.GetMechData();
        if( mechData.ActiveWeapon == null )
        {
            mechData.ActiveWeapon = mechData.FindWeaponEntity( mechData );
        }
        return mechData.ActiveWeapon != null ? CanStartActionResult.Success : CanStartActionResult.NoWeapon;
    }


    //Gets how much the AI thinks being at a given coordinate will be good for attacking.
    public virtual float GetDmgUtilityAtLocation( Game game, Actor actor, Vector2Int coord, int range )
    {
        var wep = actor.ActiveWeapon;

        if( wep == null )
            return 0f;

        var wepRange = actor.ActiveWeapon.GetStatisticValue( StatisticType.Range );

        FloatWindow rangeWindow = new FloatWindow( wepRange * 2, game.Board );
        rangeWindow.MoveCenter( coord );
        float utility = 0f;

        var otherTeams = game.GetOtherTeams( actor.GetTeamID() );

        if( !game.Board.ContainsCell( coord ) )
            return 0f;

        //Loop over every cell in attack window
        rangeWindow.Do( iter =>
        {
            //The cell on the board? If not bail
            if( !game.Board.ContainsCell( iter.world ) )
                return;

            var actorAtCell = GameEngine.Instance.Board.GetActorAtCell( iter.world );
            if( actorAtCell == null || actorAtCell.GetTeamID() == actor.GetTeamID() )
                return;

            int manhattanDistance = Board.GetManhattanDistance( coord, actorAtCell.Position );

            //If in range to attack
            if( manhattanDistance <= wepRange )
            {
                //Can we attack from this location?
                if( Board.LOS_CanSeeTo( iter.world, actor.Position ) )
                {
                    //Can see to attack
                    utility += 1f;
                }
                else
                {
                    //Can't see to attack
                    utility += .5f;
                }
            }

        }, range );
       
        return utility;
    }

    protected void TestKilledTargets(AttackActionResult res)
    {
        int numKilled = 0;
        int numHit = 0;
        var changes = res.GetChanges();
        var distinct = changes.Distinct( new StatisticChangeRootComp() ).Select( x => x.Statistic.Entity.GetRoot() as Actor );
        numHit = distinct.Count();

        distinct.Do( x => numKilled += AttackHelper.HandleDeadActor( x ) ? 1 : 0 );

        //Hit and killed the same number?
        if( numHit > 0 && numKilled == numHit )
            OnKillTarget?.Invoke();
    }

    public System.Action OnKillTarget;
}

public class StatisticChangeRootComp : IEqualityComparer<StatisticChange>
{
    public bool Equals( StatisticChange x, StatisticChange y )
    {
        return x.Statistic.Entity.GetRoot() == y.Statistic.Entity.GetRoot();
    }

    public int GetHashCode( StatisticChange obj )
    {
        return obj.GetHashCode();
    }
}

