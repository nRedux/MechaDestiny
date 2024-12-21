using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameObjectReference : TrackedReference<GameObject>
{
    public GameObjectReference( string guid ) : base( guid ) { }
}
