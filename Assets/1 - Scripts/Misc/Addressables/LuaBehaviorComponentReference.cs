using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LuaBehaviorComponentReference : TrackedReference<LuaBehaviorComponent>
{
    public LuaBehaviorComponentReference( string guid ) : base( guid ) { }
}
