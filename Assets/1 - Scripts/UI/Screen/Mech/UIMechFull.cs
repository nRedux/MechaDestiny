using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIMechFull : MonoBehaviour
{
    public TMP_Text Name;
    public RawImage Image;
    public UIMechInfo MechStats;

    private MechData _mechData;

    public MechData MechData
    {
        get => _mechData;
    }

    public void Refresh( MechData mechData )
    {
        if( mechData == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( mechData )}' cannot be null" );
        _mechData = mechData;
        var actorAsset = mechData.GetAssetSync();
        if( Name != null )
            Name.text = actorAsset.DisplayName.TryGetLocalizedString();

        MechStats.AssignEntity( mechData.Pilot );
    }
}
