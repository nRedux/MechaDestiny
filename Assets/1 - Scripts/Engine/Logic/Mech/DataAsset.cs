using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public class DataAsset<TData> : SerializedScriptableObject where TData : new()
{
    [HideLabel]
    [HideReferenceObjectPicker] 
    public TData Data = new TData();

    public TData GetDataCopy()
    {
        TData newData = Json.Clone<TData>( Data );
        SetupNewData( newData );
        return newData;
    }

    public virtual void SetupNewData( TData newData ) { }
}
