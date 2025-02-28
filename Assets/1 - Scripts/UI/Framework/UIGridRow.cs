using UnityEditor.Build.Pipeline;
using UnityEngine;

public class UIGridRow: MonoBehaviour
{
    public int MaxElements;

    public bool CanAddElement()
    {
        return this.transform.childCount < MaxElements;
    }

    public bool Add( Transform t )
    {
        if( !CanAddElement() )
            return false;
        t.SetParent( transform, false );
        return true;
    }
}
