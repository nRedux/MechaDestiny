using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GfxObjectSelected : MonoBehaviour
{

    public UnityEvent Selected;
    public UnityEvent Deselected;

    public virtual void OnSelected()
    {
        Selected.Invoke();
    }

    public virtual void OnDeselected()
    {
        Deselected.Invoke();
    }

}
