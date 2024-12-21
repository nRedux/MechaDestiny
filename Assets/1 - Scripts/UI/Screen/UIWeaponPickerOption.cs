using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIWeaponPickerOption : MonoBehaviour
{

    public TMP_Text NameText;

    public System.Action<UIWeaponPickerOption> Picked;

    public IEntity Entity;

    public void Initialize( IEntity Entity )
    {
        this.Entity = Entity;
        Refresh();

        Button btn = GetComponent<Button>();
        btn.onClick.AddListener( OnClick );
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
