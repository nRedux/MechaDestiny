using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMechPopup : UIPanel
{

    public UIMechInfo MechInfo;
    public UIWeaponInfo WeaponInfo;
    public TextMeshProUGUI DisplayName;
    public Image Portrait;

    private UIEntity _thisEntity;

    public void AssignEntity( IEntity entity )
    {
        if( MechInfo == null )
            return;
        if( entity == null )
            return;
        AssignThisEntity( entity );
        MechInfo.AssignEntity( entity );
        RefreshWeaponInfo( entity );
        UpdateVanityUI( entity );
    }

    private void UpdateVanityUI( IEntity entity )
    {
        if( entity is Actor actor )
        {
            var asset = actor.GetAssetSync();
            if( Portrait != null )
                Portrait.sprite = asset.PortraitImage;
            if( DisplayName != null )
                DisplayName.text = asset.DisplayName.TryGetLocalizedString();
        }
    }

    private void AssignThisEntity( IEntity entity )
    {
        _thisEntity = _thisEntity ?? gameObject.GetOrAddComponent<UIEntity>();
        _thisEntity.AssignEntity( entity );
    }

    private void RefreshWeaponInfo( IEntity entity )
    {
        if( entity == null ) return;

        var mechData = entity.GetSubEntities()[0] as MechData;
        WeaponInfo.Opt()?.Refresh( mechData );
    }


}
