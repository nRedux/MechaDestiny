using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class ActorReference : DataProviderReference<ActorAsset, Actor>
{
    public ActorReference( string guid ) : base( guid ) { }
}
