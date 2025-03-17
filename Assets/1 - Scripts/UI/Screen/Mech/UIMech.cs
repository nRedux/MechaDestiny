using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;


public class UIMech : MonoBehaviour, IUIItem<MechData>
{
    const string NO_PILOT_NAME = "----";

    public TMP_Text Name;
    public TMP_Text PilotName;

    public Image Portrait;

    private MechData _mechData;

    private Button _button;

    public MechData MechData
    {
        get => _mechData;
    }

    public Action<IUIItem<MechData>> Clicked { get; set; }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.Opt()?.onClick.AddListener( OnClicked );
    }


    private void OnClicked()
    {
        Clicked?.Invoke( this );
    }


    public void Refresh( MechData mech )
    {
        if( mech == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( mech )}' cannot be null" );
        _mechData = mech;
        RefreshContent();
    }

    public void RefreshContent()
    {
        var mechAsset = _mechData.GetAssetSync();
        if( Name != null )
            Name.text = mechAsset.DisplayName.TryGetLocalizedString();


        if( _mechData.HasPilot )
        {
            var pilotAsset = _mechData.Pilot.GetAssetSync();
            PilotName.Opt()?.SetText( pilotAsset.DisplayName.TryGetLocalizedString() );
        }
        else
            PilotName.Opt()?.SetText( NO_PILOT_NAME );
    }
}
