using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( menuName = "Engine/Entities/Attachment Slot Type" )]
public class AttachmentSlotType : ScriptableObject
{
    /// <summary>
    /// The type of components which are allowed on this attachment slot.
    /// </summary>
    public MechComponentType ComponentType;
}