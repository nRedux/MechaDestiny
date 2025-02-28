using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIActor : MonoBehaviour
{

    public TMP_Text Name;
    public TMP_Text Class;
    public Image Portrait;

    private Actor _actor;


    public void Refresh( Actor actor )
    {
        if( actor == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( actor )}' cannot be null" );
        _actor = actor;
        var actorAsset = actor.GetAssetSync();
        if( Name != null )
            Name.text = actorAsset.DisplayName.GetLocalizedString();

        if( Portrait != null )
            Portrait.sprite = actorAsset.PortraitImage;
    }
}
