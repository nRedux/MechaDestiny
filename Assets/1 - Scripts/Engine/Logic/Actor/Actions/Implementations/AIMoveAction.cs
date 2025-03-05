
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


/*
Each body part needs statistics
Your pilot needs statistics
Your weapons need statistics

An attack has to have some data from the pilot, some from the weapon, possibly some from the mech part it's attached to.

*/

public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
{
    public int Compare( TKey x, TKey y )
    {
        int result = x.CompareTo( y );
        if( result == 0 )
            return (int)Mathf.Sign(UnityEngine.Random.value - .5f); // Handle equality as being greater (to allow duplicates)
        return result;
    }
}

[System.Serializable]
public class AIMoveAction : MoveAction
{
    public Vector2Int Target;

    private int _range;

    private FloatWindow GenerateMoveHeatmap( Game game, Actor actor )
    {
        var mechData = actor.GetMechData();
        var moveRange = mechData.Legs.GetStatisticValue( StatisticType.Range );

        FloatWindow utility = new FloatWindow( moveRange * 2, game.Board ) { MaxIterDistance = moveRange };
        utility.Clamping = BoardWindowClamping.Positive;
        utility.MoveCenter( actor.Position );

        FillAttackHeatmap( game, actor, utility );
        //CalculatePursueValue( game, actor, utility );
        //Not clear on what would be good threat values.
        //CalculateThreatValue( game, actor, utility );

        var record = AITools.Instance.Opt()?.RecordWindow( utility, "Movement", "Attack Potentials" );
        if( record != null && actor.ActiveWeapon != null )
        {
            Debug.Log( actor.ActiveWeapon.GetAssetSync().name );
            record.Note( $"Active Weapon: {actor.ActiveWeapon.GetAssetSync().name}" );
        }
        return utility;
    }


    private void FillAttackHeatmap( Game game, Actor actor, FloatWindow utility  )
    {
        var actions = actor.GetActionsOfType<AttackAction>();
        if( actions.Count == 0 )
            return;

        if( actor.AIPersonality != null )
            actor.ActiveWeapon = actor.AIPersonality?.CheckPreferredWeapon( actor );

        utility.Do( iter =>
        {
            //The cell on the board? If not bail
            if( !game.Board.ContainsCell( iter.world ) )
                return;

            //TODO: Looping through when really we shouldn't need a loop. The AI should have one attack ability.
            foreach( var action in actions )
            {
                //Get all cells the action can effect
                if( action is AttackAction aiAction )
                {
                    utility[iter.local] += aiAction.GetDmgUtilityAtLocation( game, actor, iter.world, _range );
                }
            }
        },
        _range );

        
        //Move cost adjustments - too far, no utility. Further, reduced utility.
        utility.Do( iter =>
        {
            //Prefer closer options.
            var abPath = game.Board.GetNewPath( actor.Position, iter.world, actor );
            if( abPath.CompleteState != PathCompleteState.Complete || abPath.MoveLength() > _range )
                utility[iter.local] = 0;
            else
                utility[iter.local] -= ( abPath.MoveLength() / (float) _range ) * .5f;
        } );

        var nonZeroOptions = utility.Cells.Where( x => x > 0f ).Count() > 0;
        if( !nonZeroOptions )
        {
            utility.Do( iter =>
            {
                utility[iter.local] = GetDistanceToActorUtility( actor, utility, iter.world );
            } );
        }
    }


    private float GetDistanceToActorUtility( Actor requester, FloatWindow moveWindow,  Vector2Int coordinate )
    {

        float desiredRange = UnityEngine.Random.Range( 3, 6 );
        if( requester.AIPersonality != null )
            desiredRange = requester.AIPersonality.GetIdealAttackRange( requester );

        //Debug.Log( desiredRange );
        int maxDistance = GameEngine.Instance.Board.Width * GameEngine.Instance.Board.Height;
        float cellScore = 0f;
        GameEngine.Instance.Game.Teams.Do( team =>
        {
            if( team.Id == requester.GetTeamID() )
                return;

            var members = team.GetMembers();
            members.Do( member =>
            {
                
                Vector2Int coordToActor = member.Position - coordinate;
                float relativeIdeal =  1f - ( Mathf.Abs( desiredRange - coordToActor.magnitude ) / 10f );

                cellScore += relativeIdeal;
            } );
        } );

        return cellScore;
    }


    private void CalculatePursueValue( Game game, Actor actor, FloatWindow utility )
    {
        var enemyProxUtility = game.Board.GetEnemyProximityUtility( game, actor );
        //enemyProxUtility.DebugCells();
        
        utility.Do( iter =>
        {
            var proxLocalCoord = enemyProxUtility.WorldToLocalCell(iter.world);
            var proxVal = enemyProxUtility[proxLocalCoord];
            utility[iter.local] += proxVal;
        }, _range );
        
    }

    private void CalculateThreatValue( Game game, Actor actor, FloatWindow utility )
    {
        var enemyProxUtility = game.Board.GetEnemyThreatValue( game, actor );
        enemyProxUtility.DebugCells();

        utility.Do( iter =>
        {
            var proxLocalCoord = enemyProxUtility.WorldToLocalCell( iter.world );
            var proxVal = enemyProxUtility[proxLocalCoord];
            utility[iter.local] += proxVal;
        }, _range );
    }


    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );

        var mechData = actor.GetMechData();

        //Find max range as either the ap or range of the mech. 1ap per move atm.
        var apRange = actor.GetStatisticValue( StatisticType.AbilityPoints );
        int legsRange = mechData.Legs.Statistics.GetStatistic( StatisticType.Range ).Value;
        _range = Mathf.Min( legsRange, apRange );

        this.State = ActorActionState.Started;
        var gfxBoard = GameEngine.Instance.GfxBoard;

        SortedList<float, Vector2Int> targets = new SortedList<float, Vector2Int>( new DuplicateKeyComparer<float>() );

        //Get utility of each potential move location
        List<Vector2Int> firstPassOptions = new List<Vector2Int>();
        FloatWindow utility = GenerateMoveHeatmap( game, actor );
        utility.Do( iter =>
        {
            var abPath = game.Board.GetNewPath( actor.Position, iter.world, actor );
            if( abPath.CompleteState == PathCompleteState.Complete && abPath.MoveLength() > 0 )
            {
                targets.Add( iter.value, iter.world );
            }
        });


        var best = targets.Last().Value;
        var bestOptions = targets.Where( x => x.Value == best );

        if( bestOptions.Count() > 0 )
        {
            Target = bestOptions.Random().Value;

            //No target, we bail
            if( Target == null )
            {
                End();
                return;
            }
        }
        else
        {
            End();
            return;
        }


        ActionResult res = new MoveActionResult( actor, GameEngine.Instance.Board.GetNewPath( actor.Position, Target, actor ) );
        actor.SetPosition( Target, game );

        this.State = ActorActionState.Executing;
        res.OnComplete = () => 
        {
            End(); //Whole action done. Can move on.
        };
        UIManager.Instance.QueueResult( res );

    }

    public override void Tick()
    {
        return;
    }

    public override void TurnEnded()
    {
        End();
        return;
    }



    public override void End()
    {
        this.State = ActorActionState.Finished;
        base.End();
    }
}
