using System.Linq;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( fileName = "New Brawler Personality", menuName = "Engine/AI/Personality - Brawler")]
public class AIBrawler : AIPersonality
{

    public int RandomDistReduce = 0;

    //Ideas:
    //Could track last attacked enemy, and have this influence next attacked enemy


    /// <summary>
    /// Get the weapon the AI would "like" to use right now. This is done before moving so it moves to a location which is it's preference.
    /// </summary>
    /// <param name="actor">The ai actor</param>
    /// <returns>The weapon component it would like.</returns>
    public override MechComponentData CheckPreferredWeapon( Actor actor )
    {
        var weps = actor.FindWeaponEntities();
        //Prefer moving to use melee weapon. We could randomly choose to prefer ranged weapons. Or base it on how many enemies are in range at the moment.
        return weps.Random() as MechComponentData;
    }


    /// <summary>
    /// Switch to a weapon the AI can use right now. May not match preferred weapon. Will be based on utility of the weapons now.
    /// </summary>
    /// <param name="game">Game instance</param>
    /// <param name="actor">The AI actor</param>
    /// <param name="action">The AI actors attack action</param>
    public override void SelectForAttack( Game game, Actor actor, AIAttackAction action )
    {
        //Get weapon entities with any utility
        var prefWep = actor.FindWeaponEntities( x =>
        {
            var range = actor.ActiveWeapon.GetStatisticValue( StatisticType.Range );
            var utility = action.GetUtilityAtLocation( game, actor, actor.Position, range );

            return utility > 0;
        } );

        //Randomly select one of the options.
        actor.ActiveWeapon = prefWep.Random( 1 ).FirstOrDefault() as MechComponentData;
    }

    /// <summary>
    /// How close to enemies does this AI like to be at the moment?
    /// </summary>
    /// <param name="actor">The AI actor</param>
    /// <returns>The range the AI would like to be at based on current weapon.</returns>
    public override int GetIdealAttackRange( Actor actor )
    {
        int preferredRange = Random.Range( 2, 3 );

        if( actor.ActiveWeapon != null ) 
        {
            if( actor.ActiveWeapon.HasFeatureFlag( (int) WeaponFlags.Melee ) )
            {
                preferredRange = 1;
            }
            else
            {
                preferredRange = actor.ActiveWeapon.GetStatisticValue( StatisticType.Range );
            }

            preferredRange = Mathf.Max( 1, Random.Range( 0, RandomDistReduce + 1 ) );
        }

        return preferredRange;
    }

    private List<IEntity> GetRangeOrderedWeapons( Actor actor )
    {
        bool SelectWep( MechComponentData data )
        {
            if( data.Type != MechComponentType.Weapon )
                return false;
            if( data.IsBroken() )
                return false;
            return true;
        }

        var weapons = actor.FindWeaponEntities( SelectWep );
        return weapons.OrderBy( x => x.GetStatistic( StatisticType.Range )?.Value ?? 0 ).ToList();
    }


}
