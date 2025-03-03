using System;
using System.Collections.Generic;
using UnityEngine;

public class UIMechSelector : UIPanel
{
    public UIMechList MechList;
    public UIBlocker Blocker;
    public System.Action<UIMech> MechSelected;

    protected override void Awake()
    {
        MechList.MechClicked += OnSelect;
        Blocker = GetComponentInChildren<UIBlocker>();
        Blocker.Opt()?.ListenForClicks( OnBlockerClick );
    }

    private void OnBlockerClick()
    {
        Hide();
    }

    public void SelectFromPlayerMechs( System.Action<MechData> selected )
    {
        var data = DataHandler<RunData>.Data;
        MechSelected += mech => { selected?.Invoke( mech.MechData ); };
        Show( data.CompanyData.Mechs );
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
        var data = DataHandler<RunData>.Data;
        MechList.Opt()?.Refresh( data.CompanyData.Mechs );
    }
}
