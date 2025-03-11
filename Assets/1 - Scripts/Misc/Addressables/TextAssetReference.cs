using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Unity.VisualScripting;

[System.Serializable]
public class TextAssetReference : TrackedReference< TextAsset >
{
    public TextAssetReference( string guid ) : base( guid ) { }
}
