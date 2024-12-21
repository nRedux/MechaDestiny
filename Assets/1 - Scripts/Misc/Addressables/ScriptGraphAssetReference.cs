using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Unity.VisualScripting;

[System.Serializable]
public class ScriptGraphAssetReference : TrackedReference< ScriptGraphAsset >
{
    public ScriptGraphAssetReference( string guid ) : base( guid ) { }
}
