using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;



[System.Serializable]
public class EndActorAction : ActorAction
{

    private ActorActionState _state;


    public override ActionType ActionPhase => ActionType.End;


    [OnDeserialized]
    public void OnDeserialize( StreamingContext context )
    {
        
    }

    public override ActorActionState State()
    {
        return _state;
    }

    public override void Tick()
    {
        return;
    }

    public override void Start( Game game, Actor actor ) 
    {
        _state = ActorActionState.Executing;
    }


    public override void TurnEnded()
    {
        End();
    }


    public override void End()
    {
        base.End();
        _state = ActorActionState.Finished;
    }
}