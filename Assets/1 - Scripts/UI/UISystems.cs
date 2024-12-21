using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISystems : MonoBehaviour
{    
    public UIScreenFader Fader;

    public UIDialogBox DialogBox;

    public UIPanelBackground PanelBackgroundPrefab;

    public void Start()
    {
        if( DialogBox )
            DialogBox.Hide();
    }

    public UIPanelBackground GetPanelBackground()
    {
        if( PanelBackgroundPrefab == null )
            return null;

        var inst = GameObject.Instantiate<UIPanelBackground>( PanelBackgroundPrefab );
        return inst;
    }
}
