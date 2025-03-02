using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.VersionControl;
using Edgeflow.UI;


public class UIActor : MonoBehaviour
{

    public GameObject MechFieldRoot;

    public TMP_Text Name;
    public TMP_Text Class;
    public TMP_Text Activity;
    public TMP_Text MechName;
    public Image Portrait;

    public System.Action<UIActor> Clicked;

    private Actor _actor;
    private TMProLinkHandler _mechLinkHandler;
    private Button _button;

    public Actor Actor
    {
        get => _actor;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.Opt()?.onClick.AddListener( OnClicked );
        _mechLinkHandler = MechName.GetComponent<TMProLinkHandler>();
        if( _mechLinkHandler != null )
        {
            _mechLinkHandler.Click = MechNameClicked;
        }
    }

    private void MechNameClicked( string id )
    {
        UIRunInfo.Instance.SelectMech( Actor.PilotedMech );
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

        Activity.Opt()?.SetText( actor.Activity.ToString() );

        if( actor.PilotedMech != null )
        {
            var asset = actor.PilotedMech.GetAssetSync();
            if( asset != null )
            {
                MechFieldRoot.Opt()?.SetActive( true );
                MechName.Opt()?.SetText( GetMechNameText( actor.PilotedMech.DataID, asset.name ) );
            }
        }
        else
            MechFieldRoot.Opt()?.SetActive( false );
    }

    private string GetMechNameText( string linkID, string mechName )
    {
        return $"<link=\"{linkID}\">{mechName}</link>";
    }
}
