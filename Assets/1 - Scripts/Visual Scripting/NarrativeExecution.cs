using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Edgeflow.UI;
using Edgeflow;

public class NarrativeExecution : MonoBehaviour
{
    public event System.Action OnFinished;

    public bool BlockGameplay;

    private void ExecuteOnFinished()
    {
        ReleaseTurnloopAuthority();
        OnFinished?.Invoke();
        Destroy( gameObject );
    }

    public void TakeTurnloopAuthority()
    {
        BlockGameplay = true;
    }

    public void ReleaseTurnloopAuthority()
    {
        BlockGameplay = false;
    }

    public void Die()
    {
        Debug.Log("I are dead");
        ExecuteOnFinished();
    }
}
