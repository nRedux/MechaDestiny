using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;



[System.Serializable]
public class EndActorAction : ActorAction
{


    [OnDeserialized]
    public void OnDeserialize( StreamingContext context )
    {
        
    }


    public override void Tick()
    {
        return;
    }

    public override void Start( Game game, Actor actor ) 
    {
        this.State = ActorActionState.Executing;
    }


    public override void TurnEnded()
    {
        End();
    }


    public override void End()
    {
        base.End();
        this.State = ActorActionState.Finished;
    }
}