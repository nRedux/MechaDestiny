using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


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
    public int Range;

    public Vector2Int Target;

    public override int BoardRange => Range;



    private ActorActionState _state;


    private FloatWindow GenerateMoveHeatmap( Game game, Actor actor )
    {

        FloatWindow utility = new FloatWindow( Range * 2, game.Board );
        utility.Clamping = BoardWindowClamping.Positive;
        utility.MoveCenter( actor.Position );

        GenerateAttackHeatmap( game, actor, utility );
        //CalculatePursueValue( game, actor, utility );
        //Not clear on what would be good threat values.
        //CalculateThreatValue( game, actor, utility );

        var record = AITools.Instance.Opt()?.RecordWindow( utility, "Movement", "Attack Potentials" );
        if( record != null )
            record.Note( $"Active Weapon: {actor.ActiveWeapon.GetAssetSync().name}" );

        return utility;
    }


    private void GenerateAttackHeatmap( Game game, Actor actor, FloatWindow utility )
    {
        var actions = actor.GetActionsOfType<AttackAction>();
        if( actions.Count == 0 )
            return;

        if( actor.AIPersonality != null )
            actor.ActiveWeapon = actor.AIPersonality?.CheckPreferredWeapon( actor );

        utility.Do( iter =>
        {
            //The cell on the board? If not bail
            if( !game.Board.IsCoordInBoard( iter.world ) )
                return;

            //TODO: Looping through when really we shouldn't need a loop. The AI should have one attack ability.
            foreach( var action in actions )
            {
                //Get all cells the action can effect
                if( action is AttackAction aiAction )
                {
                    utility[iter.local] += aiAction.GetUtilityAtLocation( game, actor, iter.world, Range );
                }
            }

            //Prefer closer options.
            var dist = Board.GetManhattanDistance( actor.Position, iter.world );
            var maxRange = actor.GetMechData().Legs.GetStatisticValue( StatisticType.Range );
            utility[iter.local] -= dist / (float)maxRange;
        },
        Range );
    }


    private void CalculatePursueValue( Game game, Actor actor, FloatWindow utility )
    {
        var enemyProxUtility = game.Board.GetEnemyProximityUtility( game, actor );
        //enemyProxUtility.DebugCells();
        
        utility.Do( iter =>
        {
            var proxLocalCoord = enemyProxUtility.WorldToLocalIndex(iter.world);
            var proxVal = enemyProxUtility[proxLocalCoord];
            utility[iter.local] += proxVal;
        }, Range );
        
    }

    private void CalculateThreatValue( Game game, Actor actor, FloatWindow utility )
    {
        var enemyProxUtility = game.Board.GetEnemyThreatValue( game, actor );
        enemyProxUtility.DebugCells();

        utility.Do( iter =>
        {
            var proxLocalCoord = enemyProxUtility.WorldToLocalIndex( iter.world );
            var proxVal = enemyProxUtility[proxLocalCoord];
            utility[iter.local] += proxVal;
        }, Range );

    }


    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );
        _state = ActorActionState.Started;
        var gfxBoard = GameEngine.Instance.GfxBoard;

        SortedList<float, Vector2Int> targets = new SortedList<float, Vector2Int>( new DuplicateKeyComparer<float>() );

        //Get utility of each potential move location
        List<Vector2Int> firstPassOptions = new List<Vector2Int>();
        FloatWindow utility = GenerateMoveHeatmap( game, actor );
        utility.Do( iter =>
        {
            var path = gfxBoard.GetPath( actor.Position, iter.world );
            if( path!= null && path.Count > 0 )
            {
                targets.Add( iter.value, iter.world );
            }
        } );


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

        ActionResult res = new MoveActionResult( actor, GameEngine.Instance.Board.GetPath( actor.Position, Target ) );
        actor.SetPosition( Target, game );

        _state = ActorActionState.Executing;
        res.OnComplete = () => 
        {
            End(); //Whole action done. Can move on.
        };
        UIManager.Instance.ExecuteResult( res );

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

    public override ActorActionState State()
    {
        return _state;
    }

    public override void End()
    {
        _state = ActorActionState.Finished;
        base.End();
    }
}
