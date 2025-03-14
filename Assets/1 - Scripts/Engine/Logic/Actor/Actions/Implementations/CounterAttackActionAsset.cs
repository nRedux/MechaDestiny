using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu( menuName = "Engine/Entities/Counter Attack Action Asset" )]
public class CounterAttackActionAsset : ActorActionAsset
{
    public CounterAttackAction Data = new CounterAttackAction();

    public override ActorAction GetData()
    {
        return Json.Clone<CounterAttackAction>( Data );
    }
}
