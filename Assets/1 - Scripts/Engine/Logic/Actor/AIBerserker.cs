using System.Linq;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( fileName = "New Berserker Personality", menuName = "Engine/AI/Personality - Berserker")]
public class AIBerserker: AIPersonality
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
        //Prefer moving to use melee weapon. We could randomly choose to prefer ranged weapons. Or base it on how many enemies are in range at the moment.
        return GetRangeOrderedWeapons( actor ).Take(1).FirstOrDefault() as MechComponentData;
    }


    /// <summary>
    /// Switch to a weapon the AI can use right now. May not match preferred weapon. Will be based on utility of the weapons now.
    /// </summary>
    /// <param name="game">Game instance</param>
    /// <param name="actor">The AI actor</param>
    /// <param name="action">The AI actors attack action</param>
    public override void SelectForAttack( Game game, Actor actor, AIAttackAction action )
    {
        
        var prefWep = CheckPreferredWeapon( actor );
        
        var ordered = GetRangeOrderedWeapons( actor );
        float bestUtility = float.MinValue;
        MechComponentData bestWeapon = null;
        foreach( var item in ordered )
        {
            actor.ActiveWeapon = item as MechComponentData;
            var range = actor.ActiveWeapon.GetStatisticValue( StatisticType.Range );
            var utility = action.GetUtilityAtLocation( game, actor, actor.Position, range );

            if( actor.ActiveWeapon == prefWep )
                utility *= 10f;

            if( utility > bestUtility )
            {
                bestUtility = utility;
                bestWeapon = item as MechComponentData;
            }
        }

        actor.ActiveWeapon = bestWeapon;
        
        //actor.ActiveWeapon = CheckPreferredWeapon( actor );
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
