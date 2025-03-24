using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using TMPro;

public class UICombatEnd : UIPanel
{

    public TMP_Text StatusText;

    public LocalizedString VictoryText;
    public LocalizedString DefeatText;


    public void PrepareForShow( int winner )
    {
        RefreshStatusText( winner );
    }


    private void RefreshStatusText( int winner )
    {
        if( StatusText == null )
            return;
        StatusText.text = GetStatusLabelText( winner );
    }


    private string GetStatusLabelText( int winner )
    {
        if( winner == 0 )
        {
            return VictoryText.TryGetLocalizedString( "Victory text" );
        }
        else
        {
            return DefeatText.TryGetLocalizedString( "Victory text" );
        }
    }

    //Need flow control post combat.

    public override void Show()
    {
        base.Show();
        this.CreateBackground();
        this.PanelBackground.OnClick += () =>
        {
            if( RunManager.Instance.RunData.WorldMapData != null )
            {
                SceneLoadManager.LoadMapDataScene( RunManager.Instance.RunData.WorldMapData, true, null, null );
            }
        };
    }

    private void ExecutePostCombatScript()
    {

    }

}
