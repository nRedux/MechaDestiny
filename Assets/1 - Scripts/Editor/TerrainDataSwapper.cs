using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TerrainDataSwapper : EditorWindow
{

    private TerrainData _data;

    [MenuItem("Tools/Terrain")]
    public static void DoShow()
    {
        var wnd = GetWindow<TerrainDataSwapper>();
        wnd.Show();
    }


    public void OnGUI()
    {
        if( Selection.activeGameObject == null )
            return;
        var terrain = Selection.activeGameObject.GetComponent<Terrain>();

        if( terrain == null )
            GUI.enabled = false;
        GUI.enabled = true;
        GUI.changed = false;
        _data = EditorGUILayout.ObjectField( _data, typeof( TerrainData ), true ) as TerrainData;

        bool button = GUILayout.Button( new GUIContent( "Swap" ) );
        if( button )
        {
            //Do swap
        }

        GUILayout.Space( 50 );


        bool duplicate = GUILayout.Button( new GUIContent( "Duplicate" ) );
        if( duplicate )
        {
            var dupe = Instantiate<GameObject>( terrain.gameObject );
            var dupeTerrain = dupe.GetComponent<Terrain>();
            var data = terrain.terrainData;

            var path = AssetDatabase.GetAssetPath( data );
            Debug.Log( path );
            int lastSlash = path.IndexOf( '/' );
            var directory = path.Substring( 0, lastSlash );
            var ext = Path.GetExtension( path );
            var fileName = Path.GetFileName( path ).Replace( ext, string.Empty );

            string newPath = string.Empty;
            
            for( var i = 0; i < 20; i++ )
            {
                newPath = directory + "/" + fileName + $"_{i}" + ext;
                if( !AssetDatabase.CopyAsset( path, newPath ) )
                    Debug.LogWarning( $"Failed to copy {newPath}" );
                else
                    break;
            }

            var asset = AssetDatabase.LoadAssetAtPath( newPath, typeof(TerrainData) ) as TerrainData;
            dupeTerrain.terrainData = asset;
            dupeTerrain.Flush();
        }

    }

}
