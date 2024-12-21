using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu( menuName = "Engine/Entities/AI Move Action" )]
public class AIMoveActionAsset : ActorActionAsset
{
    public AIMoveAction Data;


    public override ActorAction GetData()
    {
        return Json.Clone<AIMoveAction>( Data );
    }
}
