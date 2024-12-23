using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public virtual float GetEffectUtility( Game game, Actor actor, Vector2Int coord )
    {
        FloatWindow rangeWindow = new FloatWindow( BoardRange * 2 );
        rangeWindow.MoveCenter( coord );
        float utility = 0f;

        var otherTeams = game.GetOtherTeams( actor.GetTeamID() );

        if( !game.Board.IsCoordInBoard( coord ) )
            return 0f;

        //TODO: This looks weird. I don't know if I'm doing early outs at the right time. Reevaluate.
        rangeWindow.Do( iter =>
        {
            otherTeams.Do( team =>
            {
                team.GetMembers().Do( member =>
                {
                    //The cell on the board?
                    if( !game.Board.IsCoordInBoard( iter.world ) )
                        return;

                    int manhattanDistance = Board.GetManhattanDistance( coord, member.Position );
                    if( member.Position == iter.world && manhattanDistance <= BoardRange )
                    {
                        //Can we see the enemy from the cell?
                        if( Board.LOS_CanSeeTo( iter.world, member.Position ) )
                            utility += manhattanDistance * 1.1f;
                        else
                            utility += manhattanDistance;
                    }
                } );
            } );
        }, BoardRange );
       
        return utility;
    }



}
