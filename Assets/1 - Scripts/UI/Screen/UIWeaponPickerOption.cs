using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class UIWeaponPickerOption : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public TMP_Text NameText;

    public System.Action<UIWeaponPickerOption> Picked;

    public IEntity Entity;

    public System.Action<UIWeaponPickerOption> PointerEntered;
    public System.Action<UIWeaponPickerOption> PointerExited;
    public System.Action<UIWeaponPickerOption> Clicked;

    

    public void OnPointerEnter( PointerEventData eventData )
    {
        PointerEntered?.Invoke( this );
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        PointerExited?.Invoke( this );
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        Clicked?.Invoke( this );
    }

    public void Initialize( IEntity Entity )
    {
        this.Entity = Entity;
        Refresh();

        Button btn = GetComponent<Button>();
        btn.onClick.AddListener( OnClick );
        //BindPointerEvents();
    }

    private void OnClick()
    {
        Picked?.Invoke( this );
    }

    public void Refresh()
    {
        if( Entity == null )
        {
            Debug.LogError("Entity cannot be null.");
            return;
        }

        if( Entity is MechComponentData data )
        {
            var x = data.GetAssetSync();
            if( NameText != null )
                NameText.text = x.DisplayName.TryGetLocalizedString();
        }
    }

}
