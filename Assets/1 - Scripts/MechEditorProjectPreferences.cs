using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CreateAssetMenu( fileName = "MechEditorProjectSettings", menuName = "Engine/Settings/MechEditorProjectSettings" )]
public class MechEditorProjectPreferences : ScriptableObject
{
    public const string PREFERENCES_ASSET_PATH = "Assets/2 - Assets/MechEditorProjectPreferences.asset";


    public Color AllySpawnColor;
    public Color EnemySpawnColor;

    private static MechEditorProjectPreferences _settings;

    public static MechEditorProjectPreferences Settings
    {
        get
        {
            if( _settings == null )
            {
                _settings = AssetDatabase.LoadAssetAtPath<MechEditorProjectPreferences>( PREFERENCES_ASSET_PATH );
            }
            return _settings;
        }
    }
}
#endif