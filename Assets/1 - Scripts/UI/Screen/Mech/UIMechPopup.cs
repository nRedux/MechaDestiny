using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;


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
        if( entity is Actor actor )
            RefreshWeaponInfo( actor.ActiveWeapon );
        
        UpdateVanityUI( entity );
    }

    public override void OnHide()
    {
        base.OnHide();
        //Stop listening to whatever was assigned previously, safety
        StopListeningWeaponChange( _thisEntity?.Entity );
    }


    private void StartListeningWeaponChange(IEntity entity)
    {
        if( entity is Actor actor )
        {
            var mechData = actor.GetMechData();
            if( mechData != null && mechData.ActiveWeaponChanged != WeaponChanged )
                mechData.ActiveWeaponChanged += WeaponChanged;
        }
    }

    private void StopListeningWeaponChange( IEntity entity )
    {
        if( entity is Actor actor )
        {
            actor.GetMechData().ActiveWeaponChanged -= WeaponChanged;
        }
    }

    private void WeaponChanged( MechComponentData data )
    {
        RefreshWeaponInfo( data );
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
        //Stop listening to whatever was assigned previously, safety
        StopListeningWeaponChange( _thisEntity?.Entity );
        _thisEntity = _thisEntity ?? gameObject.GetOrAddComponent<UIEntity>();
        _thisEntity.AssignEntity( entity );
        StartListeningWeaponChange( entity );
    }

    private void RefreshWeaponInfo( MechComponentData weapon )
    {
        if( weapon == null ) return;
        WeaponInfo.Opt()?.Refresh( weapon );
    }


}
