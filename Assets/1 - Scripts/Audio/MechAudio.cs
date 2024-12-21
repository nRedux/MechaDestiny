using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class MechAudio : MonoBehaviour
{
    public StudioEventEmitter Footsteps;

    public void PlayFootstep()
    {
        Footsteps.Play();
    }
}
