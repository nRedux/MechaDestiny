using Unity.VisualScripting;
using UnityEngine;

public abstract class AIPersonality: ScriptableObject
{
    private int desiredRange = 0;
    private bool _desiredRangeDone = false;

    public virtual void SelectForAttack( Game game, Actor actor, AIAttackAction action )
    {
        
    }

    public virtual MechComponentData CheckPreferredWeapon( Actor mech )
    {
        return mech.ActiveWeapon;
    }

    public virtual int GetIdealAttackRange( Actor actor )
    {
        _desiredRangeDone = true;
        var weps = actor.FindWeaponEntities();
        int minRange = 1;
        int maxRange = 1;
        weps.Do( x =>
        {
            minRange = Mathf.Min( minRange, x.GetStatisticValue( StatisticType.Range ) );
            maxRange = Mathf.Max( maxRange, x.GetStatisticValue( StatisticType.Range ) );
        } );

        return (int) Mathf.Lerp( minRange, maxRange, Random.Range( .5f, 1f) );
    }

    public AIPersonality Clone()
    {
        return ScriptableObject.Instantiate( this );
    }
}
