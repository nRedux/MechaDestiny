using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public static class AttackHelper
{

    public static bool HandleDeadActor( Actor actor )
    {
        if( actor.IsDead() )
            return actor.Die();
        return false;
    }

    private static MechComponentData SelectComponentTarget( this MechData data )
    {
        if( DevTools.TARGET_TORSO_ONLY )
        {
            return data.Torso;
        }
        else if ( DevTools.TARGET_LEGS_ONLY )
        {
            return data.Legs;
        }
        else if ( DevTools.TARGET_L_ARM_ONLY )
        {
            return data.LeftArm;
        }
        else if( DevTools.TARGET_R_ARM_ONLY )
        {
            return data.RightArm;
        }

        return data.GetAttackTargets().Random();
    }

    public static void CalculateAttackDamage( AttackActionResult res )
    {
        var actor = res.Attacker.Actor;
        int shotCount = res.AttackerWeapon.GetStatisticValue( StatisticType.ShotCount );

        if( res.AttackerWeapon.IsAOE() )
        {
            //Res should have target location in this case.
            //Find all actors within area of effect

            var shape = res.AttackerWeapon.AOEShape;
            if( shape == null )
            {
                Debug.LogError("Weapon doesn't have AOEShape.");
                return;
            }

            BoolWindow shapeWin = new BoolWindow( shape.Width, GameEngine.Instance.Board );
            shapeWin.Fill( shape );
            shapeWin.MoveCenter( new Vector2Int( (int)res.Target.Position.x, (int)res.Target.Position.z ) );

            //TODO: must be turned into a method of the BoolWindow or it's base class
            List<Actor> actors = new List<Actor>();
            shapeWin.Do( x =>
            {
                if( !x.value )
                    return;
                var actor = GameEngine.Instance.Board.GetActorAtCell( x.world );
                if( actor != null )
                    actors.Add( actor );
                res.AffectedAOECells.Add( x.world );

               
            } );

            Debug.Log( $"Found {actors.Count} actors" );

            int fragmentCount = res.AttackerWeapon.GetStatisticValue( StatisticType.FragmentCount );
            actors.Do( target =>
            {
                target.TriggerEvent( ActorEvent.Attacked, res.Attacker.Actor );

                for( int i = 0; i < fragmentCount; i++ )
                {
                    CheckHitAndApplyDamage( actor, target, res );
                }
            } );
        }
        else
        {
            if( res.Target.Actor == null )
                Debug.Log( res.Target.ToString() );

            res.Target.Actor.TriggerEvent( ActorEvent.Attacked, res.Attacker.Actor );

            res.Evaded = AttackHelper.CalculateAttackEvaded( actor, res.Target.Actor );
            for( int i = 0; i < shotCount; i++ )
            {
                CheckHitAndApplyDamage( actor, res.Target.Actor, res );
            }
        }
    }


    private static void CheckHitAndApplyDamage( Actor attacker, Actor target, AttackActionResult res )
    {
        var weaponEntity = attacker.ActiveWeapon;
        int shotCount = weaponEntity.GetStatisticValue( StatisticType.ShotCount );
        var damage = weaponEntity.GetStatisticValue( StatisticType.Damage );
        var targetMechData = target.GetMechData();

        var randomCompEntity = targetMechData.SelectComponentTarget();
        var targetStats = randomCompEntity.GetStatistics();
        var healthStat = targetStats.GetStatistic( StatisticType.Health );


        bool isHit = CalculateHitOrMiss( attacker, target );
        if( res.Evaded )
            isHit = false;
        StatChangeTag tags = StatChangeTag.None;
        if( !isHit )
            tags |= StatChangeTag.Miss;

        ///TODO
        //I dislike watching for the changes instead of just adding the changes.
        res.WatchForStatChanges( randomCompEntity, healthStat );

        ///TODO
        //This could just return the change and we could add it to the result
        healthStat.SetValue( isHit ? healthStat.Value - damage : healthStat.Value, tags );
    }


    private static bool CalculateHitOrMiss( Actor attackActor, Actor targetActor )
    {
        var actor = attackActor;
        var weapon = actor.ActiveWeapon;
        //TODO: Shouldn't this just be manhattan distance?
        var distance = GameEngine.Instance.Board.GetDistance( attackActor.Position, targetActor.Position );
        if( distance == null )
            return false;
        var accuracy = weapon.CalculateAccuracy( distance.Value ) / 100f;
        return UnityEngine.Random.value < accuracy;
    }


    public static bool CalculateAttackEvaded( Actor attackActor, Actor targetActor )
    {
        Debug.Assert( targetActor != null, "Target Actor Null" );
        Debug.Assert( attackActor != null, "Attack Actor Null" );
        Debug.Assert( targetActor.Boosts != null, "TargetActor boosts null" );

        var actor = attackActor;
        var weapon = actor.ActiveWeapon;
        var evasion = targetActor.Boosts.GetStatistic( StatisticType.Evasion );
        if( evasion == null )
            return false;

        return UnityEngine.Random.value < evasion.Value;
    }


    public static int GetFragmentCount( IEntity weapon )
    {
        var weaponStats = weapon.GetStatistics();
        var shotCount = weaponStats.GetStatistic( StatisticType.FragmentCount );
        //TODO: What if there isn't a shotCount stat defined?
        return shotCount.Value;
    }

    public static UIFindAttackTargetRequest ShowWeaponOverlay( Actor actor, MechComponentData weapon )
    {
        UIFindAttackTargetRequest result = null;

        result = new UIFindAttackTargetRequest( actor, weapon, null, null, null ); ;

        //Don't target actors on the same team
        result.MarkInvalidTeams( actor.GetTeamID() );
        UIManager.Instance.RequestUI( result, false );
        return result;
    }


}