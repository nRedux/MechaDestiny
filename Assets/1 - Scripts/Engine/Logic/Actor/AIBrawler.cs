using System.Linq;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( fileName = "New Brawler Personality", menuName = "Engine/AI/Personality - Brawler")]
public class AIBrawler : AIPersonality
{

    public int RandomDistReduce = 0;
    [Tooltip("Chance that melee will be done. Otherwise will do ranged.")]
    [Range(0, 100)]
    public int meleeChance = 0;

    //Ideas:
    //Could track last attacked enemy, and have this influence next attacked enemy


    /// <summary>
    /// Get the weapon the AI would "like" to use right now. This is done before moving so it moves to a location which is it's preference.
    /// </summary>
    /// <param name="actor">The ai actor</param>
    /// <returns>The weapon component it would like.</returns>
    public override MechComponentData CheckPreferredWeapon( Actor actor )
    {
        var weps = actor.FindFunctionalWeapons();
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
        var prefWep = actor.FindFunctionalWeapons().Where( x =>
        {
            var range = x.GetStatisticValue( StatisticType.Range );
            var utility = action.GetDmgUtilityAtLocation( game, actor, actor.Position, range );

            return utility > 0;
        } );

        var melee = prefWep.Where( x => x.WeaponType == WeaponType.Melee ).FirstOrDefault();
        var ranged = prefWep.Where( x => x.WeaponType != WeaponType.Melee  );


        //Occasionally 
        if( Random.value * 100 < meleeChance || ranged.Count() == 0 )
            actor.ActiveWeapon = melee;
        else
        {
            //Take any ranged weapon instead
            actor.ActiveWeapon = ranged.Random( 1 ).FirstOrDefault();
        }
    }

    private List<MechComponentData> GetRangeOrderedWeapons( Actor actor )
    {
        var weapons = actor.FindFunctionalWeapons();
        return weapons.OrderBy( x => x.GetStatistic( StatisticType.Range )?.Value ?? 0 ).ToList();
    }


}
