using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu( menuName = "Engine/Entities/Actor Move Action" )]
public class ActorMoveAsset : ActorActionAsset
{
    public PlayerMoveAction Data;

    public override ActorAction GetData()
    {
        return Json.Clone<PlayerMoveAction>( Data );
    }
}
