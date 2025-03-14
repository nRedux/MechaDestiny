using System.Linq;
using UnityEngine;

//Action responsible for boosting evasion
[System.Serializable]
public class CounterAttackAction : ActorAction
{
    /// <summary>
    /// The % chance to counter attack when attacked.
    /// </summary>
    public int CounterChance;

    private bool _doCounterAttacks = false;


    public override void ResetForPhase()
    {
        base.ResetForPhase();
        _doCounterAttacks = false;
    }


    public override void Start( Game game, Actor actor )
    {
        base.Start( game, actor );

        _doCounterAttacks = true;

        End();
    }


    private bool DoCounter()
    {
        return Random.value <= CounterChance;
    }


    public override void TriggerEvent( Actor actor, Actor source, ActorEvent evt )
    {
        if( !_doCounterAttacks )
            return;
        if( !DoCounter() )
            return;

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
    }
}
