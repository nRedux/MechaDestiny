
public static class AttackHelper
{

    public static void HandleDeadActor( Actor actor )
    {
        if( actor.IsDead() )
        {
            actor.Die();
        }
    }

    public static AttackActionResult CreateAttackActionResult( GfxActor attackerAvatar, GfxActor targetAvatar )
    {
        var weapon = GetAttackWeapon( attackerAvatar.Actor );
        int count = GetAttackCount( weapon );

        //Start set up attack action result
        AttackActionResult res = new AttackActionResult( targetAvatar.Actor.Position, attackerAvatar.transform )
        {
            Attacker = attackerAvatar,
            AttackerMechComponent = weapon.GetParent() as MechComponentData,
            AttackerWeapon = weapon as MechComponentData,
            Count = count
        };

        //End set up attack action result

        res.MechData = targetAvatar.Actor.GetMechData();
        res.TargetAvatar = targetAvatar;
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

    public static void DoAttackDamage( AttackActionResult res )
    {
        var actor = res.Attacker.Actor;
        var weapon = GetAttackWeapon( actor );
        int shotCount = GetAttackCount( weapon );
        var damage = weapon.GetStatistic( StatisticType.Damage );

        var targetActor = res.TargetAvatar.Actor;
        var targetMechData = targetActor.GetMechData();

        for( int i = 0; i < shotCount; i++ )
        {
            var randomCompEntity = targetMechData.SelectComponentTarget();
            var targetStats = randomCompEntity.GetStatistics();
            var healthStat = targetStats.GetStatistic( StatisticType.Health );
            bool isHit = CalculateHitOrMiss( actor, targetActor );

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


}