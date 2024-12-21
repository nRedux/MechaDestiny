using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class AttachmentSlotTypeReference : TrackedReference<AttachmentSlotType>
{
    public AttachmentSlotTypeReference( string guid ) : base( guid ) { }
}
