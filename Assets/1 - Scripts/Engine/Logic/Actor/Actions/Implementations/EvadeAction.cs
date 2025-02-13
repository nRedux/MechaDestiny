using UnityEngine;

//Action responsible for boosting evasion
[System.Serializable]
public class EvadeAction : ActorAction
{
    /// <summary>
    /// The % chance of evasion which the action is providing when used.
    /// </summary>
    public int EvadeChance;

    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );
        var evasion = actor.Boosts.GetStatistic( StatisticType.Evasion );

        if( evasion == null )
            actor.Boosts.AddStatistic( StatisticType.Evasion, EvadeChance );
        else
            evasion.Value += EvadeChance;

        End();
    }
}
