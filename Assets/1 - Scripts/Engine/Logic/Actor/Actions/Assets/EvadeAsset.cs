using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu( menuName = "Engine/Entities/Evade Action Asset" )]
public class EvadeAsset : ActorActionAsset
{
    public EvadeAction Data;

    public override ActorAction GetData()
    {
        return Json.Clone<EvadeAction>( Data );
    }
}
