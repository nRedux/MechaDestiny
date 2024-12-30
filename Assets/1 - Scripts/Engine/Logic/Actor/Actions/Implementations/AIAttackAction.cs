using System.Collections;   
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using Newtonsoft.Json;
using System.Drawing;
using System.Linq;

[System.Serializable]
public class AIAttackAction : AttackAction
{
    public int Damage = 1;

    private Game _game;
    private Actor _actor;

    [JsonIgnore]
    [JsonProperty]
    private BoolWindow _attackOptions = null;

    UIFindAttackTargetRequest _uiRequest = null;

    private ActorActionState _state;


    [OnDeserialized]
    public void OnDeserialize( StreamingContext context )
    {
    }


    public override ActorActionState State()
    {
        return _state;
    }


    public override void Tick()
    {
        return;
    }


    private Vector2Int GetAOETarget( Game game, Actor actor, int range )
    {
        var activeWeapon = actor.GetMechData().ActiveWeapon;
        if( activeWeapon == null || !activeWeapon.IsAOE() )
            return new Vector2Int();

        FloatWindow rangeWindow = new FloatWindow( range * 2 );
        rangeWindow.MoveCenter( actor.Position );

        var others = game.GetOtherTeams( actor.GetTeamID() );

        FloatWindow countsWindow = new FloatWindow( game.Board.Width, game.Board.Height );

        BoolWindow AOEWin = activeWeapon.AOEShape.NewBoolWindow();
        //Find cell targets
        rangeWindow.Do( iter =>
        {

            AOEWin.MoveCenter( iter.world );

            //At each location we want any enemies on that cell
            others.Do( team =>
            {
                var members = team.GetMembers();
                AOEWin.Do( aoeIter =>
                {

                    members.Do( member =>
                    {
                        if( !game.Board.IsCoordInBoard( aoeIter.world ) )
                            return;
                        if( member.Position == aoeIter.world )
                        {
                            countsWindow[aoeIter.world] += 1;
                        }
                    } );

                } );

            } );

        }, range );

        float highest = -1;
        Vector2Int cell = new Vector2Int();
        countsWindow.Do( x =>
        {
            if( x.value > highest )
            {
                highest = x.value;
                cell = x.world;
            }
        } );

        return cell;
    }


    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );

        var activeWep = actor.GetMechData().ActiveWeapon;
        var range = activeWep.GetStatistic( StatisticType.Range ).Value;


        _game = game;
        _actor = actor;
        _state = ActorActionState.Started;

        _attackOptions = new BoolWindow( range * 2 );

        _attackOptions.MoveCenter( actor.Position );
        _game.Board.GetCellsManhattan( range, _attackOptions );
        Board.LOS_PruneBoolWindow( _attackOptions, actor.Position );

        GfxActor attackerAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );

        if( attackerAvatar == null )
        {
            End();
            Debug.LogError( "Avatar not found for actor." );
            return;
        }

        //Final target location
        SmartPoint targetLocation = null;

        //The actor targeted. Will have a value below if weapon is not AOE
        Actor targetActor = null;
        GfxActor targetAvatar = null;
        if( activeWep.IsAOE() )
        {
            Vector2Int cellTarget = GetAOETarget( game, actor, range );
            Vector3 worldTarget = new Vector3( cellTarget.x, 0f, cellTarget.y );
            targetLocation = new SmartPoint( worldTarget );
        }
        else
        {
            targetActor = game.GetRandomMemberFromOtherTeams( thisTeamID: actor.GetTeamID(), validLocations: _attackOptions );
            //Early out? No target.
            if( targetActor == null )
            {
                End();
                Debug.Log( "No target for AI to attack" );
                return;
            }

            targetAvatar = GameEngine.Instance.AvatarManager.GetAvatar( targetActor );
            targetLocation = new SmartPoint( targetAvatar );
        }


        _state = ActorActionState.Executing;
        AttackActionResult res = AttackHelper.CreateAttackActionResult( attackerAvatar, targetLocation );

        UIManager.Instance.ShowSideAMechInfo( actor, UIManager.MechInfoDisplayMode.Mini );
        if( !activeWep.IsAOE() )
            UIManager.Instance.ShowSideBMechInfo( targetActor, UIManager.MechInfoDisplayMode.Mini );

        res.OnComplete = () => {
            UIManager.Instance.HideSideBMechInfo();
            if( !activeWep.IsAOE() )
                UIManager.Instance.ShowSideAMechInfo( actor, UIManager.MechInfoDisplayMode.Full );
            End();
        };

        AttackHelper.CalculateAttackDamage( res );

        TestKilledTargets( res );

        UIManager.Instance.ExecuteResult( res );
    }


    public override void TurnEnded()
    {

    }


    public override float GetEffectUtility( Game game, Actor actor, Vector2Int coord, int range )
    {
        FloatWindow rangeWindow = new FloatWindow( range * 2 );
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
                        if( member.Position == iter.world && manhattanDistance <= range )
                        {
                            utility += manhattanDistance;
                        }
                    } );
                } );
            }, range );
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
                    if( manhattanDistance <= range )
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
