using UnityEditor;
using UnityEngine;

public static class MechProjectPreferencesProvider
{
    private static MechEditorProjectPreferences settings;

    [SettingsProvider()]
    public static SettingsProvider CreateCustomPreferencesProvider()
    {
        var provider = new SettingsProvider( "Preferences/Mechspedition", SettingsScope.User )
        {
            label = "Mechspedition",
            guiHandler = ( searchContext ) =>
            {
                if( settings == null )
                {
                    settings = MechEditorProjectPreferences.Settings;
                }

                if( settings != null )
                {
                    EditorGUILayout.LabelField( "Spawn Colors" );
                    settings.AllySpawnColor = EditorGUILayout.ColorField( "Ally Spawn Color", settings.AllySpawnColor );
                    settings.EnemySpawnColor = EditorGUILayout.ColorField( "Enemy Spawn Color", settings.EnemySpawnColor );

                    EditorUtility.SetDirty( settings );
                }
                else
                {
                    EditorGUILayout.HelpBox( $"Settings asset not found. Load path: {MechEditorProjectPreferences.PREFERENCES_ASSET_PATH}", MessageType.Warning );
                }
            }
        };
        return provider;
    }
}