using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class MechComponentReference : DataProviderReference<MechComponentAsset, MechComponentData>
{
    public MechComponentReference( string guid ) : base( guid ) { }
}
