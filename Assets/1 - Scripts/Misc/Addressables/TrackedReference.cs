using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TrackedReference<TAsset> : AssetReferenceT<TAsset> where TAsset: UnityEngine.Object
{

    public TrackedReference( string guid ) : base( guid ) { }

    public AsyncOperationHandle<TAsset> Operation;

    private System.Action<TAsset> _onComplete;

    public void GetAsset( Action<TAsset> assetReadyCallback )
    {
        if( !Operation.IsValid() )
        {
            Operation = Addressables.LoadAssetAsync<TAsset>( this );
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


    public async Task<TAsset> GetAssetAsync( System.Action<TAsset> result = null )
    {
        if( !Operation.IsValid() )
        {
            Operation = Addressables.LoadAssetAsync<TAsset>( this );
            await Operation.Task;
        }
        else if( Operation.Status == AsyncOperationStatus.None )
        {
            await Operation.Task;
        }
        else if( Operation.Status == AsyncOperationStatus.Failed )
            return null;
        
        result?.Invoke( Operation.Result );
        return Operation.Result;
    }



    public void GetAssetSync( Action<TAsset> assetReadyCallback )
    {
        if( !Operation.IsValid() )
        {
            Operation = Addressables.LoadAssetAsync<TAsset>( this );
            _onComplete += assetReadyCallback;
            Operation.Completed += OperationComplete;
            Operation.WaitForCompletion();
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

    public TAsset GetAssetSync()
    {
        if( !Operation.IsValid() )
        {
            Operation = Addressables.LoadAssetAsync<TAsset>( this );
            Operation.Completed += OperationComplete;
            Operation.WaitForCompletion();
        }

        return Operation.Result;
    }

    private void OperationComplete( AsyncOperationHandle<TAsset> opHandle )
    {
        var result = opHandle.Result;
        _onComplete?.Invoke( result );
    }
}
