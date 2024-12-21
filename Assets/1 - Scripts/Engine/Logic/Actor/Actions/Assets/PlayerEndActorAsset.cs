using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu( menuName = "Engine/Entities/Player End Actor" )]
public class PlayerEndActorAsset : ActorActionAsset
{
    public EndActorAction Data;

    public override ActorAction GetData()
    {
        return Json.Clone<EndActorAction>( Data );
    }
}
