using System.Collections.Generic;
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

    public static AttackActionResult CreateAttackActionResult( GfxActor attackerAvatar, SmartPoint target )
    {
        var weapon = GetAttackWeapon( attackerAvatar.Actor );
        int count = GetAttackCount( weapon );

        //Start set up attack action result
        AttackActionResult res = new AttackActionResult( )
        {
            Attacker = attackerAvatar,
            AttackerMechComponent = weapon.GetParent() as MechComponentData,
            AttackerWeapon = weapon as MechComponentData,
            Count = count
        };

        //End set up attack action result
        res.Target = target;
        return res;
    }


    private static MechComponentData SelectComponentTarget( this MechData data )
    {
        if( DevConfiguration.TARGET_TORSO_ONLY )
        {
            return data.Torso;
        }
        else if ( DevConfiguration.TARGET_LEGS_ONLY )
        {
            return data.Torso;
        }
        else if ( DevConfiguration.TARGET_L_ARM_ONLY )
        {
            return data.LeftArm;
        }
        else if( DevConfiguration.TARGET_R_ARM_ONLY )
        {
            return data.RightArm;
        }

        return data.GetSubEntities().Random() as MechComponentData;
    }

    public static void CalculateAttackDamage( AttackActionResult res )
    {
        var actor = res.Attacker.Actor;
        var weaponEntity = GetAttackWeapon( actor );
        int shotCount = GetAttackCount( weaponEntity );
        var damage = weaponEntity.GetStatistic( StatisticType.Damage );

        if( res.AttackerWeapon.IsAOE() )
        {
            //Res should have target location in this case.
            //Find all actors within area of effect

            var weaponComponent = weaponEntity as MechComponentData;
            var shape = weaponComponent.AOEShape;
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
                var actor = UIManager.Instance.GetActorAtCell( x.world );
                if( actor != null )
                    actors.Add( actor );
                res.AffectedAOECells.Add( x.world );
            } );

            Debug.Log( $"Found {actors.Count} actors" );

            int fragmentCount = GetFragmentCount( weaponEntity );
            actors.Do( target =>
            {
                for( int i = 0; i < fragmentCount; i++ )
                {
                    CalculateHitDamage( actor, target, res );
                }
            } );

        }
        else
        {
            for( int i = 0; i < shotCount; i++ )
            {
                CalculateHitDamage( actor, res.Target.GfxActor.Actor, res );
            }
        }
    }


    private static void CalculateHitDamage( Actor attacker, Actor target, AttackActionResult res )
    {
        var weaponEntity = GetAttackWeapon( attacker );
        int shotCount = GetAttackCount( weaponEntity );
        var damage = weaponEntity.GetStatistic( StatisticType.Damage );
        var targetMechData = target.GetMechData();

        var randomCompEntity = targetMechData.SelectComponentTarget();
        var targetStats = randomCompEntity.GetStatistics();
        var healthStat = targetStats.GetStatistic( StatisticType.Health );
        bool isHit = CalculateHitOrMiss( attacker, target );

        StatChangeTag tags = StatChangeTag.None;
        if( !isHit )
            tags |= StatChangeTag.Miss;

        ///TODO
        //I dislike watching for the changes instead of just adding the changes.
        res.WatchForStatChanges( randomCompEntity, healthStat );

        ///TODO
        //This could just return the change and we could add it to the result
        healthStat.SetValue( isHit ? healthStat.Value - damage.Value : healthStat.Value, tags );
    }


    private static bool CalculateHitOrMiss( Actor attackActor, Actor targetActor )
    {
        var actor = attackActor;
        var weapon = GetAttackWeapon( actor ) as MechComponentData;
        var distance = GameEngine.Instance.Board.GetDistance( attackActor.Position, targetActor.Position );
        if( distance == null )
            return false;
        var accuracy = weapon.CalculateAccuracy( distance.Value ) / 100f;
        return UnityEngine.Random.value < accuracy;
    }

    public static IEntity GetAttackWeapon( Actor actor )
    {
        var attackerMechEntity = actor.GetSubEntities()[0];
        var attackerMechData = attackerMechEntity as MechData;
        return attackerMechData.ActiveWeapon;
    }

    public static int GetAttackCount( IEntity weapon )
    {
        var weaponStats = weapon.GetStatistics();
        var shotCount = weaponStats.GetStatistic( StatisticType.ShotCount );
        //TODO: What if there isn't a shotCount stat defined?
        return shotCount.Value;
    }

    public static int GetFragmentCount( IEntity weapon )
    {
        var weaponStats = weapon.GetStatistics();
        var shotCount = weaponStats.GetStatistic( StatisticType.FragmentCount );
        //TODO: What if there isn't a shotCount stat defined?
        return shotCount.Value;
    }


}