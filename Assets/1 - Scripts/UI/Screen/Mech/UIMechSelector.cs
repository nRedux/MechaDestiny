using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIMechSelector : UIPanel
{
    public UIMechList MechList;
    public UIBlocker Blocker;
    public System.Action<UIMech> MechSelected;

    protected override void Awake()
    {
        MechList.Clicked += item => OnSelect((UIMech)item);
        Blocker = GetComponentInChildren<UIBlocker>();
        Blocker.Opt()?.ListenForClicks( OnBlockerClick );
    }

    private void OnBlockerClick()
    {
        Hide();
    }

    public void SelectFromPlayerMechs( System.Action<MechData> selected, List<Actor> excludeActors = null )
    {
        var data = DataHandler.RunData;

        List<MechData> filteredMechs = new List<MechData>( data.CompanyData.Mechs );
        if( excludeActors != null )
        {
            filteredMechs = filteredMechs.Where( x => !excludeActors.Contains( x.Pilot ) ).ToList();
        }

        MechSelected += mech => { selected?.Invoke( mech.MechData ); };
        Show( filteredMechs );
    }

    public void Show( List<MechData> mechData )
    {
        MechList.Opt()?.Refresh( mechData );
        Show();
    }

    private void OnSelect( UIMech mech )
    {
        MechSelected?.Invoke( mech );
        Hide();
    }

    public override void OnHide()
    {
        base.OnHide();
        MechSelected = null;
    }

    public override void OnShow()
    {
        base.OnShow();
        var data = DataHandler.RunData;
    }
}
