using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class DataObject<TData>
{
    [HideInInspector]
    public AssetReference AssetReference;

    public AsyncOperationHandle<TData> Operation;

    public void SetGUID( string dataGUID ) 
    {
        AssetReference = new AssetReference( dataGUID );
    }

    public void GetAsset( Action<TData> onComplete )
    {
        Operation = Addressables.LoadAssetAsync<TData>( AssetReference );
        Operation.Completed += opHandle => onComplete(opHandle.Result);
    }

    public TData GetAssetSync()
    {
        //What if Operation already in place? Thinking ... thinking....
        Operation = Addressables.LoadAssetAsync<TData>( AssetReference );
        Operation.WaitForCompletion();
        return Operation.Result;
    }

    private void ClearFlags( ref int flags )
    {
        flags = 0;
    }

    public void SetFlags( ref int source, int flags )
    {
        source |= flags;
    }

    public void UnsetFlags( ref int source, int flags )
    {
        source &= ~flags;
    }

    public bool HasFlags( int source, int flags )
    {
        return ( source & flags ) == flags;
    }

    public bool HasFlag( int source, int flag )
    {
        return ( source & flag ) != 0;
    }

    public bool NotHasFlag( int source, int flag )
    {
        return ( source & flag ) == 0;
    }
}