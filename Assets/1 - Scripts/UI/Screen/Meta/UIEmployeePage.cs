using System;
using UnityEngine;

public class UIEmployeePage : MonoBehaviour
{

    public UIActor SelectedActorUI;
    public UIActorCollection ActorList;

    private UIActor _selectedActor;

    private void Awake()
    {
        if( ActorList != null )
            ActorList.ActorClicked += ActorClicked;
    }

    private void ActorClicked( UIActor actor )
    {
        SelectActor( actor );
    }

    private void SelectActor( UIActor actor )
    {
        _selectedActor = actor;

        if( SelectedActorUI != null && _selectedActor != null )
        {
            SelectedActorUI.gameObject.SetActive( true );
            SelectedActorUI.Refresh( actor.Actor );
        }
        if( _selectedActor == null )
        {
            SelectedActorUI.Opt()?.gameObject.SetActive( false );
        }
    }
}
