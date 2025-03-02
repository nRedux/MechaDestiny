using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class UIMech : MonoBehaviour
{
    public TMP_Text Name;
    public Image Portrait;

    public System.Action<UIMech> Clicked;

    private MechData _mechData;

    private Button _button;

    public MechData MechData
    {
        get => _mechData;
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


    public void Refresh( MechData actor )
    {
        if( actor == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( actor )}' cannot be null" );
        _mechData = actor;
        var actorAsset = actor.GetAssetSync();
        if( Name != null )
            Name.text = actorAsset.DisplayName.TryGetLocalizedString();

    }
}
