using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu( menuName = "Engine/Entities/Player Engage Action" )]
public class PlayerEngageAsset : ActorActionAsset
{
    public PlayerEngageAction Data;

    public override ActorAction GetData()
    {
        return Json.Clone<PlayerEngageAction>( Data );
    }
}
