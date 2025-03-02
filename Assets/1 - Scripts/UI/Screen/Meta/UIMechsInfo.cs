using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UIMechsInfo : MonoBehaviour
{

    public UIMech SelectedMechUI;
    public UIMechList MechList;

    private UIMech _selectedMech;

    private void Awake()
    {
        if( MechList != null )
            MechList.MechClicked += echClicked;
    }

    private void Start()
    {
        if( MechList.MechCollection != null )
            SelectActor( MechList.GetActorUIs().FirstOrDefault() );
        else
            SelectActor( null );
    }

    private void echClicked( UIMech mech )
    {
        SelectActor( mech );
    }

    private void SelectActor( UIMech mech )
    {
        _selectedMech = mech;

        if( SelectedMechUI != null && _selectedMech != null )
        {
            SelectedMechUI.gameObject.SetActive( true );
            SelectedMechUI.Refresh( mech.MechData );
        }
        if( _selectedMech == null )
        {
            SelectedMechUI.Opt()?.gameObject.SetActive( false );
        }
    }
}
