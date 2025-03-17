using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIMechFull : MonoBehaviour
{
    public TMP_Text Name;
    public RawImage Image;
    private MechData _mechData;

    public MechData MechData
    {
        get => _mechData;
    }

    public void Refresh( MechData actor )
    {
        if( actor == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( actor )}' cannot be null" );
        _mechData = actor;
        var actorAsset = actor.GetAssetSync();
        if( Name != null )
            Name.text = actorAsset.DisplayName.TryGetLocalizedString();

        //Set up preview here.
    }
}
