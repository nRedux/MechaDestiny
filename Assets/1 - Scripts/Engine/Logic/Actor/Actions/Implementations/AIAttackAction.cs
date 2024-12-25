using System.Collections;   
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class AIAttackAction : AttackAction
{
    public int Range = 3;
    public int Damage = 1;

    private Game _game;
    private Actor _actor;

    [JsonIgnore]
    [JsonProperty]
    private BoolWindow _attackOptions = null;

    UIFindAttackTargetRequest _uiRequest = null;

    private ActorActionState _state;


    public override int BoardRange => Range;


    public override ActionType ActionPhase => ActionType.Attack;


    [OnDeserialized]
    public void OnDeserialize( StreamingContext context )
    {
        _attackOptions = new BoolWindow( Range * 2 );
    }


    public override ActorActionState State()
    {
        return _state;
    }


    public override void Tick()
    {
        return;
    }


    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );
        //Get valid move locations. Notify the UI we need to display a collection of move locations. Wait for UI to return a result. Execute move.
        _game = game;
        _actor = actor;
        _state = ActorActionState.Started;

        _attackOptions.MoveCenter( actor.Position );
        _game.Board.GetCellsManhattan( Range, _attackOptions );
        Board.LOS_PruneBoolWindow( _attackOptions, actor.Position );

        GfxActor attackerAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );

        if( attackerAvatar == null )
        {
            End();
            Debug.LogError( "Avatar not found for actor." );
            return;
        }

        var targetActor = game.GetRandomMemberFromOtherTeams( thisTeamID: actor.GetTeamID(), validLocations: _attackOptions );
        //Early out? No target.
        if( targetActor == null )
        {
            End();
            Debug.Log( "No target for AI to attack" );
            return;
        }

        var targetAvatar = GameEngine.Instance.AvatarManager.GetAvatar( targetActor );

        
        _state = ActorActionState.Executing;
        AttackActionResult res = AttackHelper.CreateAttackActionResult( attackerAvatar, new SmartPoint( targetAvatar ) );

        UIManager.Instance.ShowSideAMechInfo( actor, UIManager.MechInfoDisplayMode.Mini );
        UIManager.Instance.ShowSideBMechInfo( targetActor, UIManager.MechInfoDisplayMode.Mini );
        res.OnComplete = () => {
            UIManager.Instance.HideSideBMechInfo();
            UIManager.Instance.ShowSideAMechInfo( actor, UIManager.MechInfoDisplayMode.Full );
            End();
        };

        AttackHelper.DoAttackDamage( res );

        //TODO: This should be checked after the attack completes. Right now it's going to make the sequence look wrong. 
        AttackHelper.HandleDeadActor( targetActor );

        UIManager.Instance.ExecuteResult( res );
    }



    public override void TurnEnded()
    {

    }


    public override float GetEffectUtility( Game game, Actor actor, Vector2Int coord )
    {
        FloatWindow rangeWindow = new FloatWindow( Range * 2 );
        rangeWindow.MoveCenter( coord );
        float utility = 0f;

        var others = game.GetOtherTeams( actor.GetTeamID() );

        bool method = true;
        if( method )
        {
            rangeWindow.Do( iter =>
            {
                others.Do( team =>
                {
                    team.GetMembers().Do( member =>
                    {
                        if( !game.Board.IsCoordInBoard( coord ) )
                            return;
                        int manhattanDistance = Board.GetManhattanDistance( coord, member.Position );
                        if( member.Position == iter.world && manhattanDistance <= Range )
                        {
                            utility += manhattanDistance;
                        }
                    } );
                } );
            }, Range );
        }
        else
        {
            others.Do( team =>
            {
                team.GetMembers().Do( member =>
                {
                    if( !game.Board.IsCoordInBoard( coord ) )
                        return;
                    int manhattanDistance = Board.GetManhattanDistance( coord, member.Position );
                    if( manhattanDistance <= Range )
                    {
                        utility += 1f;
                    }
                } );
            } );
        }
        return utility;
    }


    public override void End()
    {
        _state = ActorActionState.Finished;
        _uiRequest?.Cancel();
        _uiRequest = null;
    }
}
