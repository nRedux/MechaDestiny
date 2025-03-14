using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class EvadeActionResult : ActionResult
{
    private ICameraBehavior _camera;
    private ActionResultStatus _status = ActionResultStatus.Running;
    private Actor _actor;

    public EvadeActionResult( Actor actor )
    {
        _actor = actor;
        _camera = new SpecialCameraBehavior(actor, null);
    }

    public override ActionResultStatus Update()
    {
        return _status;
    }

    public override async void Start()
    {
        await _camera.Begin();
        CreatePopup( _actor, UIInfoPopups.BOOST_POPUP );
        await Task.Delay( 700 );
       
        await _camera.End();

        _status = ActionResultStatus.Finished;
    }

    private void CreatePopup( Actor actor, string popupType )
    {
        if( string.IsNullOrEmpty( popupType ) )
            return;

        var avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        if( avatar == null )
        {
            Debug.LogError( "Avatar null" );
            return;
        }
        var torso = avatar.Torso;
        if( torso == null )
        {
            Debug.LogError( "Avatar has no torso" );
            return;
        }
        UIInfoPopups.Instance.CreatePop( popupType, "Evade UP", torso.transform.position );
    }


}


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

        //CreatePopup( actor, UIInfoPopups.BOOST_POPUP );

        EvadeActionResult res = new EvadeActionResult( actor );

        UIManager.Instance.QueueResult( res );

        End();
    }

    public override void TriggerEvent( Actor actor, Actor source, ActorEvent evt )
    {
        /*
        switch( evt )
        {
            case ActorEvent.Attacked:
                actor.GetActionsOfTypeStrict<AttackAction>();
                var attack = actor.GetActionsOfTypeStrict<PlayerAttackAction>().FirstOrDefault();
                if( attack == null )
                    return;

                GameEngine.Instance.Game.WantsAct.Add( new ActorWantsAct() { actor = actor, action = attack, target = new SmartPoint( source ) } );
                break;
        }
        */
    }
}
