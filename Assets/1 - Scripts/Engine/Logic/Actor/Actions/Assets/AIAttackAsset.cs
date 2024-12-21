using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu( menuName = "Engine/Entities/AI Attack" )]
public class AIAttackAsset : ActorActionAsset
{
    public AIAttackAction Data;

    public override ActorAction GetData()
    {
        return Json.Clone<AIAttackAction>( Data );
    }
}
