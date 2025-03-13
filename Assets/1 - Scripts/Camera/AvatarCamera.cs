using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public abstract class AvatarCamera : MonoBehaviour
{

    private System.Action _startBlendFinished = null;

    public void StartBlendFinished( ICinemachineMixer mixer, ICinemachineCamera camera )
    {
        _startBlendFinished?.Invoke();
        _startBlendFinished = null;
    }

    public void Begin(System.Action startFinished, Transform attacker, SmartPoint target )
    {
        _startBlendFinished = startFinished;
        InternalBegin( attacker, target );
    }

    public void End()
    {
        InternalEnd();
    }

    protected abstract void InternalBegin( Transform attacker, SmartPoint target );

    protected abstract void InternalEnd();
}
