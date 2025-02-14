using System;
using UnityEngine;

//Action responsible for boosting evasion
[System.Serializable]
public class EvadeAction : ActorAction
{
    /// <summary>
    /// The % chance of evasion which the action is providing when used.
    /// </summary>
    public int EvadeChance;

    [NonSerialized]
    public UIInfoPop InfoPop;

    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );
        var evasion = actor.Boosts.GetStatistic( StatisticType.Evasion );

        if( evasion == null )
            actor.Boosts.AddStatistic( StatisticType.Evasion, EvadeChance );
        else
            evasion.Value += EvadeChance;

        CreatePopup( actor, UIInfoPopups.BOOST_POPUP );

        End();
    }

    private void CreatePopup( Actor actor, string popupType )
    {
        if( string.IsNullOrEmpty( popupType ) )
            return;

        var avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        if( avatar == null )
        {
            Debug.LogError("Avatar null");
            return;
        }
        var torso = avatar.Torso;
        if( torso == null )
        {
            Debug.LogError("Avatar has no torso");
            return;
        }
        UIInfoPopups.Instance.CreatePop( popupType, "Evade UP", torso.transform.position );
    }
}
