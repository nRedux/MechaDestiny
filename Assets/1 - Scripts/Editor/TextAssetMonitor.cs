using UnityEditor;
using UnityEngine;

public class TextAssetMonitor : AssetPostprocessor
{
    public static event System.Action TextAssetModified;

    static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
    {
        foreach( string asset in importedAssets )
        {
            if( asset.EndsWith( ".txt" ) ) // Check for .txt files
            {
                Debug.Log( $"Text asset imported or modified: {asset}" );
                TextAssetModified?.Invoke();
            }
        }
    }
}
