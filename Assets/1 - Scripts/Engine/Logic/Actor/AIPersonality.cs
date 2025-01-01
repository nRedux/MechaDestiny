using Unity.VisualScripting;
using UnityEngine;

public abstract class AIPersonality: ScriptableObject
{
    public virtual void SelectWeapon( Game game, Actor actor, AIAttackAction action )
    {
        
    }

    public virtual MechComponentData CheckPreferredWeapon( Actor mech )
    {
        return mech.ActiveWeapon;
    }

    public virtual int GetIdealAttackRange( Actor mech )
    {
        var activeWep = mech.ActiveWeapon;
        if( activeWep == null )
            return Random.Range( 5, 10 );
        return activeWep.GetStatistic( StatisticType.Range ).Value;
    }

    public AIPersonality Clone()
    {
        return ScriptableObject.Instantiate( this );
    }
}
