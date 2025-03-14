using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public abstract class AvatarCamera : MonoBehaviour
{

    public string ID;

    private System.Action _startBlendFinished = null;

    public void StartBlendFinished( ICinemachineMixer mixer, ICinemachineCamera camera )
    {
        _startBlendFinished?.Invoke();
        _startBlendFinished = null;
    }

    public void Activate(System.Action startFinished, Transform attacker, SmartPoint target )
    {
        _startBlendFinished = startFinished;
        InternalBegin( attacker, target );
        gameObject.SetActive( true );
    }

    public void Deactivate()
    {
        InternalEnd();
        gameObject.SetActive( false );
    }

    protected abstract void InternalBegin( Transform attacker, SmartPoint target );

    protected abstract void InternalEnd();
}
