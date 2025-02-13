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


    [OnDeserialized]
    public void OnDeserialize( StreamingContext context )
    {
    }



    public override void Tick()
    {
        return;
    }


    private Vector2Int GetAOETarget( Game game, Actor actor, int range )
    {
        var activeWeapon = actor.ActiveWeapon;
        if( activeWeapon == null || !activeWeapon.IsAOE() )
            return new Vector2Int();

        FloatWindow rangeWindow = new FloatWindow( range * 2, game.Board );
        rangeWindow.MoveCenter( actor.Position );

        var others = game.GetOtherTeams( actor.GetTeamID() );

        FloatWindow countsWindow = new FloatWindow( game.Board.Width, game.Board.Height, game.Board );

        BoolWindow AOEWin = activeWeapon.AOEShape.NewBoolWindow( game.Board );
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
                        if( !game.Board.ContainsCell( aoeIter.world ) )
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
        
        //Select ideal weapon based on range.
        if( actor.AIPersonality != null )
            actor.AIPersonality.SelectForAttack( game, actor, this );
        
        var activeWep = actor.ActiveWeapon;
        var range = activeWep.GetStatistic( StatisticType.Range ).Value;


        _game = game;
        _actor = actor;
        this.State = ActorActionState.Started;

        _attackOptions = new BoolWindow( range * 2, game.Board );

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

        UIManager.Instance.ShowSideAMechInfo( actor, UIManager.MechInfoDisplayMode.Mini );
        if( !activeWep.IsAOE() )
            UIManager.Instance.ShowSideBMechInfo( targetActor, UIManager.MechInfoDisplayMode.Mini );

        this.State = ActorActionState.Executing;
        AttackActionResult res = new AttackActionResult( attackerAvatar, targetLocation, actor.ActiveWeapon );
        res.OnComplete = () => {
            UIManager.Instance.HideSideBMechInfo();
            if( !activeWep.IsAOE() )
                UIManager.Instance.ShowSideAMechInfo( actor, UIManager.MechInfoDisplayMode.Full );
            End();
        };

        AttackHelper.CalculateAttackDamage( res );

        TestKilledTargets( res );

        UIManager.Instance.QueueResult( res );
    }


    public override void TurnEnded()
    {

    }

    public override void End()
    {
        this.State = ActorActionState.Finished;
        _uiRequest?.Cancel();
        _uiRequest = null;
    }
}
