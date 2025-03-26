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


    public UICombatRewardList RewardList;

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
        RefreshRewards();
        this.PanelBackground.OnClick += () =>
        {
            FinishRewardScreen();
        };
    }

    public void FinishRewardScreen()
    {
        //Take all the items left in the reward screen. Later I'll just make this take the critical "always" items like gold, scrap, etc.
        RewardList.PlayerTakeAllItems();

        //Default behavior is to return to current map
        if( RunManager.Instance.RunData.WorldMapData != null )
        {
            SceneLoadManager.LoadMapDataScene( RunManager.Instance.RunData.WorldMapData, true, null, null );
        }
    }


    private void RefreshRewards()
    {
        List<IItem> items = new List<IItem>();
        if( RunManager.Instance.RunData.CombatMoneyReward.HasValue )
            items.Add( new Money( false, RunManager.Instance.RunData.CombatMoneyReward.Value ) );
        items.AddRange( RunManager.Instance.RunData.CombatRewards );
        RewardList.Opt()?.Refresh( items );
    }
}
