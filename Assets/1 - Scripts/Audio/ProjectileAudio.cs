using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class ProjectileAudio : MonoBehaviour
{

    public StudioEventEmitter ImpactMetal;

    public void SurfaceHit( EffectSurfaceType surfType )
    {
        switch( surfType )
        {
            case EffectSurfaceType.Metal:
                ImpactMetal.Play();
                break;
        }
    }

}
