using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu( menuName = "Engine/Entities/Player Attack Action" )]
public class PlayerAttackAsset : ActorActionAsset
{
    public PlayerAttackAction Data;

    public override ActorAction GetData()
    {
        return Json.Clone<PlayerAttackAction>( Data );
    }
}
