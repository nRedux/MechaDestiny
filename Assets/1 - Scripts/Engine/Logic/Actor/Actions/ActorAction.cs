using UnityEngine;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;
using UnityEngine.Localization;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine.Serialization;
using Sirenix.OdinInspector;

public enum ResultType
{
    Damage = 100
}


public enum CanStartActionResult
{
    Success,
    InsufficientAP,
    NoWeapon
}


public enum ActorActionState
{
    Started,
    Executing,
    Finished
}


public enum ActionCategory
{
    Control,
    Attack,
    Support,
    Augment,
    NONE
}

public interface IAbilityCost
{
    int GetAPCost();
}

public interface ICatagorizedAbility
{
    ActionCategory Category { get; }
}

public interface IDescribedObject
{
    LocalizedString DisplayName { get; }
    LocalizedString Description { get; }
}

public abstract class ActorAction
{
    public ActionCategory Category;

    [HideIf( nameof(HideAP) )]
    [FormerlySerializedAs("Cost")]
    [FormerlySerializedAs( "APCost" )]
    [SerializeField]
    private int _APCost;

#if USE_ACTION_BLOCKS
    public int BlocksUsed;
#endif

    public LocalizedString DisplayName;
    public LocalizedString Description;

    [HideInInspector]
    public bool RespondToEvents = true;

    public virtual bool HideAP => false;

    public virtual int APCost => _APCost;

    public ActorActionState State
    {
        get;
        protected set;
    } = ActorActionState.Started;

    public virtual void Start( Game game, Actor actor ) { }
    public virtual void Tick() { }
    public virtual void TurnEnded() { }
    public virtual void End() { State = ActorActionState.Finished; }

    public virtual void TriggerEvent(Actor actor, Actor source, ActorEvent evt)
    {

    }

    public bool AllowActionSelect { get; protected set; } = true;


    /// <summary>
    /// Checks if the action is in a state where it's allowed to execute
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    public virtual CanStartActionResult AllowedToExecute( Actor actor )
    {
        if( !actor.CanSpendStatistic( StatisticType.AbilityPoints, APCost ) )
            return CanStartActionResult.InsufficientAP;

        return CanStartActionResult.Success;
    }

    public void SpendAP( Actor actor )
    {
        actor.SpendStatistic( StatisticType.AbilityPoints, APCost );
    }

    public void SpendAP( Actor actor, int cost )
    {
        actor.SpendStatistic( StatisticType.AbilityPoints, cost );
    }
}


