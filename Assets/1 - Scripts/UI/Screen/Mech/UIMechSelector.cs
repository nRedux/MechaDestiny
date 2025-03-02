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

    public void Show( List<MechData> mechData )
    {
        MechList.Opt()?.Refresh( mechData );
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
