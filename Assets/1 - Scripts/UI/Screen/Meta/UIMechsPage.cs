using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UIMechsPage : MonoBehaviour
{

    public UIMechFull SelectedMech;
    public UIMechList MechList;

    private UIMech _selectedActor;

    private void Awake()
    {
        if( MechList != null )
            MechList.ActorClicked += ActorClicked;
    }

    private void Start()
    {
        if( MechList.MechCollection != null )
            SelectMech( MechList.GetActorUIs().FirstOrDefault() );
        else
            SelectMech( null );
    }

    private void ActorClicked( UIMech mech )
    {
        SelectMech( mech );
    }

    private void SelectMech( UIMech mech )
    {
        _selectedActor = mech;

        if( SelectedMech != null && _selectedActor != null )
        {
            SelectedMech.gameObject.SetActive( true );
            SelectedMech.Refresh( mech.MechData );
        }
        if( _selectedActor == null )
        {
            SelectedMech.Opt()?.gameObject.SetActive( false );
        }
    }
}
