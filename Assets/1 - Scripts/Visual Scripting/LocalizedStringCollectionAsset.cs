using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;


[CreateAssetMenu( menuName = "Engine/Localization/String Collection Asset" )]
public class LocalizedStringCollectionAsset : ScriptableObject
{
    public List<LocalizedString> Strings = new List<LocalizedString>();

    public string Localized( int index )
    {
        if( index < 0 || index >= Strings.Count )
            return null;

        var indexed = Strings[index];
        if( indexed == null )
            return null;

        return indexed.TryGetLocalizedString();
    }
}
