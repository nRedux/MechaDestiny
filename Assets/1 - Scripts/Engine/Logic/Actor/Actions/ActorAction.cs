using UnityEngine;

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;
using UnityEngine.Localization;
using UnityEditor.Localization.Plugins.XLIFF.V12;

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

public abstract class ActorAction
{
    public ActionCategory Category;
    public int Cost;
    public LocalizedString DisplayName;
    public LocalizedString Description;

    public ActorActionState State
    {
        get;
        protected set;
    } = ActorActionState.Started;

    public virtual int BoardRange { get; }
    public virtual void Start( Game game, Actor actor ) { }
    public virtual void Tick() { }
    public virtual void TurnEnded() { }
    public virtual void End() { }

    public bool AllowActionSelect { get; protected set; } = true;


    /// <summary>
    /// Checks if the action is in a state where it's allowed to execute
    /// </summary>
    /// <param name="actor"></param>
    /// <returns></returns>
    public virtual CanStartActionResult AllowedToExecute( Actor actor )
    {
        if( !actor.CanSpendStatistic( StatisticType.AbilityPoints, Cost ) )
            return CanStartActionResult.InsufficientAP;

        return CanStartActionResult.Success;
    }

    public void SpendAP( Actor actor )
    {
        actor.SpendStatistic( StatisticType.AbilityPoints, Cost );
    }

    public void SpendAP( Actor actor, int cost )
    {
        actor.SpendStatistic( StatisticType.AbilityPoints, cost );
    }

}


