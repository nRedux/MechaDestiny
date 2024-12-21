using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Localization;


[CreateAssetMenu(menuName = "Engine/Localization/String Asset")]
public class LocalizedStringAsset: ScriptableObject
{
    public LocalizedString String;

    public string Localized { get => String.TryGetLocalizedString(); }
}
