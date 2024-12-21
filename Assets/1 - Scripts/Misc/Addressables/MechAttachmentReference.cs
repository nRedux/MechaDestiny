using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class MechAttachmentReference : TrackedReference<MechAttachmentAsset>
{
    public MechAttachmentReference( string guid ) : base( guid ) { }
}
