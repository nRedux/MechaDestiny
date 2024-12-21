using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDialogBox : UIPanel
{

    public Image PortraitImage;
    public TMP_Text TitleText;
    public TMP_Text ContentText;

    public System.Action OnContinue;

    public void Refresh(string titleContent, string stringContent)
    {
        this.TitleText.text = titleContent;
        this.ContentText.text = stringContent;
    }

    public void Show( string titleContent, string stringContent )
    {
        Refresh( titleContent, stringContent );
        CreateBackground();

        //On click hide self!
        this.PanelBackground.OnClick += () =>
        {
            Hide();
            KillBackground();

            //Must copy to a local var.
            //When we call OnContinue, the callback code can end up trying to assign to OnContinue.
            //If we don't cache to a local var then we potentially clear OnContinue when other code has assigned a new listener to it.
            var continueCallback = OnContinue;
            OnContinue = null;

            continueCallback?.Invoke();
        };

        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
        if( this.PanelBackground != null )
        {
            Destroy( this.PanelBackground.gameObject );
            this.PanelBackground = null;
        }
    }

}
