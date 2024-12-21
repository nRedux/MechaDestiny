using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

public abstract class DataProviderAsset<TAssetType, TData> : ScriptableObject where TData: DataObject<TAssetType>, new()
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
