using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveAction : ActorAction
{

    public override void Start( Game game, Actor actor )
    {
        GameEngine.Instance.UIManager.ShowSideAMechInfo( actor, UIManager.MechInfoDisplayMode.Mini );
    }

    public override void End()
    {
    }

}
