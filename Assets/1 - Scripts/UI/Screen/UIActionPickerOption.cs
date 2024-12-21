using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIActionPickerOption : UISelectorItemOption<ActorAction>
{

    public TMP_Text NameText;


    public override void Refresh()
    {
        if( DataSource == null )
        {
            Debug.LogError("Entity cannot be null.");
            return;
        }

        var x = DataSource;

        /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         * The problem below here is that the ActorAction class couldn't serialize LocalizedString
         * We'd need to make LocalizedString serialize, or we have to follow through with
         * ActorAction/ActorActionAsset being a DataProviderAsset addressable pattern.
         * 
         * I lean toward making LocalizedString serialize properly.
         *!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!*/
        if( NameText != null )
            NameText.text = x.DisplayName.TryGetLocalizedString();

    }
}
