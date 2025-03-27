using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class UIMechsPage : MonoBehaviour
{

    public UIMechFull SelectedMech;
    public UIMechList MechList;

    private UIMech _selectedMech;

    private void Awake()
    {
        if( MechList != null )
            MechList.Clicked += item => ActorClicked((UIMech)item);
    }

    private void Start()
    {
        MechList.Refresh( RunManager.Instance.RunData.CompanyData.Mechs );
        if( MechList.Collection != null )
            SelectMech( MechList.GetUIs().FirstOrDefault() );
        else
            SelectMech( (UIMech)null );
    }

    private void ActorClicked( UIMech mech )
    {
        SelectMech( mech );
    }

    public void SelectMech( MechData mechData )
    {
        var ui = MechList.GetUIs().FirstOrDefault( x => x.MechData == mechData );
        SelectMech( ui );
    }

    private void SelectMech( UIMech mech )
    {
        _selectedMech = mech;

        UIRunInfo.Instance.StartRenderingMech( mech.MechData );

        if( SelectedMech != null && _selectedMech != null )
        {
            SelectedMech.gameObject.SetActive( true );
            SelectedMech.Refresh( mech.MechData );
        }
        if( _selectedMech == null )
        {
            SelectedMech.Opt()?.gameObject.SetActive( false );
        }
    }
}
