using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class MechAssetReference : DataProviderReference<MechAsset, MechData>
{
    public MechAssetReference( string guid ) : base( guid ) { }
}
