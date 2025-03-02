using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class DataObject<TData>
{
    [JsonIgnore]
    public string DataID { get; private set; }

    [HideInInspector]
    public AssetReference AssetReference;

    public AsyncOperationHandle<TData> Operation;


    public void InitDataID()
    {
        if( DataID != null )
            return;
        DataID = System.Guid.NewGuid().ToString();
    }

    public void SetGUID( string dataGUID )
    {
        AssetReference = new AssetReference( dataGUID );
    }

    public void GetAsset( Action<TData> onComplete )
    {
        //I think this should be safe for future load operations.... similarly though, when this is deserialized, we could assign the existing assetreference.
        if( Operation.IsValid() )
        {
            onComplete?.Invoke( Operation.Result );
            return;
        }

        Operation = Addressables.LoadAssetAsync<TData>( AssetReference );
        Operation.Completed += opHandle => onComplete(opHandle.Result);
    }

    public TData GetAssetSync()
    {
        //I think this should be safe for future load operations.... similarly though, when this is deserialized, we could assign the existing assetreference.
        if( Operation.IsValid() )
            return Operation.Result;

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
