using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;


public class UIItemMechComponent : MonoBehaviour, IUIItem<MechComponentData>
{

    public Image Portrait;

    public TMP_Text Name;
    public TMP_Text WeaponType;


    private MechComponentData _componentData;

    private Button _button;

    public MechComponentData ComponentData
    {
        get => _componentData;
    }

    public Action<IUIItem<MechComponentData>> Clicked { get; set; }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.Opt()?.onClick.AddListener( OnClicked );
    }

    private void OnClicked()
    {
        Clicked?.Invoke( this );
    }

    public void Refresh( MechComponentData mech )
    {
        if( mech == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( mech )}' cannot be null" );
        _componentData = mech;
        RefreshContent();
    }

    public void RefreshContent()
    {
        var componentAsset = _componentData.GetAssetSync();

        if( Portrait != null )
            Portrait.sprite = componentAsset.Portrait;

        if( Name != null )
            Name.text = componentAsset.DisplayName.TryGetLocalizedString();

        if( WeaponType != null )
            WeaponType.text = _componentData.WeaponType.ToString();
    }
}
