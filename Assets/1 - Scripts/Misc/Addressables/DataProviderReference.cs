using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataProviderReference<TAsset, TData> : AssetReferenceT<TAsset> where TAsset : DataProviderAsset<TAsset, TData> where TData : DataObject<TAsset>, new()
{

    public DataProviderReference( string guid ) : base( guid ) { }

    public AsyncOperationHandle<TAsset> Operation;

    private System.Action<TAsset> _onComplete;

    public TData GetDataSync( )
    {
        var asset = GetAssetSync();
        var data = asset.GetDataCopy();
        data.SetGUID( this.AssetGUID );
        return data;
    }

    public void GetAsset( Action<TAsset> assetReadyCallback )
    {
        if( !Operation.IsValid() )
        {
            Operation = Addressables.LoadAssetAsync<TAsset>( this );
            Operation.Result.Data.SetGUID( AssetGUID );
            _onComplete += assetReadyCallback;
            Operation.Completed += OperationComplete;
        }
        else
        {
            if( Operation.IsDone )
            {
                assetReadyCallback( Operation.Result );
            }
            else
            {
                _onComplete += assetReadyCallback;
            }
        }
    }

    public TAsset GetAssetSync( )
    {
        if( !Operation.IsValid() )
        {
            Operation = Addressables.LoadAssetAsync<TAsset>( this );
            Operation.WaitForCompletion();
            Operation.Result.Data.SetGUID( AssetGUID );
            return Operation.Result;
        }
        else
        {
            return Operation.Result;
        }
    }

    private void OperationComplete( AsyncOperationHandle<TAsset> opHandle )
    {
        var result = opHandle.Result;
        _onComplete( result );
    }

}
