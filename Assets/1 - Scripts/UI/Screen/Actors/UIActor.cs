using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class UIActor : MonoBehaviour
{

    public TMP_Text Name;
    public TMP_Text Class;
    public Image Portrait;

    public System.Action<UIActor> Clicked;

    private Actor _actor;

    private Button _button;

    public Actor Actor
    {
        get => _actor;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.Opt()?.onClick.AddListener( OnClicked );
    }


    private void OnClicked()
    {
        Clicked?.Invoke( this );
    }


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
