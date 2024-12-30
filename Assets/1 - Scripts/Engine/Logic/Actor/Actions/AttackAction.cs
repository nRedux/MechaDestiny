using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using static PlayerAttackAction;

public abstract class AttackAction : ActorAction
{
    public SequencePos SequencePos { get; internal set; }

    public override void Start( Game game, Actor actor )
    {
        
    }


    public override void End()
    {
        base.End();
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



    public virtual float GetEffectUtility( Game game, Actor actor, Vector2Int coord, int range )
    {
        var mech = actor.GetMechData();
        var wep = mech.ActiveWeapon;
        var wepRange = actor.ActiveWeapon.GetStatisticValue( StatisticType.Range );

        FloatWindow rangeWindow = new FloatWindow( wepRange * 2 );
        rangeWindow.MoveCenter( coord );
        float utility = 0f;

        var otherTeams = game.GetOtherTeams( actor.GetTeamID() );

        if( !game.Board.IsCoordInBoard( coord ) )
            return 0f;

        //Loop over every cell
        rangeWindow.Do( iter =>
        {
            //The cell on the board? If not bail
            if( !game.Board.IsCoordInBoard( iter.world ) )
                return;

            //loop over other teams
            otherTeams.Do( team =>
            {
                //Loop over each member of team
                team.GetMembers().Do( member =>
                {
                    int manhattanDistance = Board.GetManhattanDistance( iter.world, member.Position );
                    if( member.Position == iter.world && manhattanDistance <= wepRange )
                    {
                        var path = game.Board.GetPath( iter.world, coord );
                        if( path == null || path.Count > wepRange )
                            return;

                        float losBoost = Board.LOS_CanSeeTo( iter.world, member.Position ) ? 1.1f : 0;
                        //Can we see the enemy from the cell?
                        utility += ( 1f - ( path.Count / wepRange ) ) * losBoost;
                    }
                } );
            } );
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

        if( numKilled == numHit )
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

