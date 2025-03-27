using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UIEmployeePage : UIPanel
{

    public UIActor SelectedActorUI;
    public UIActorCollection ActorList;

    private UIActor _selectedActor;

    protected override void Awake()
    {
        if( ActorList != null )
            ActorList.Clicked = item => ActorClicked((UIActor) item);
    }

    private void Start()
    {
        if( ActorList.Collection != null )
            SelectActor( ActorList.GetUIs().FirstOrDefault() );
        else
            SelectActor( null );
    }

    public override void OnShow()
    {
        ActorList.Refresh( RunManager.Instance.RunData.CompanyData.Employees );
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
            //Begin rendering mech
            if( _selectedActor.Actor.PilotedMech != null )
                UIRunInfo.Instance.StartRenderingMech( _selectedActor.Actor.PilotedMech );

            SelectedActorUI.gameObject.SetActive( true );
            SelectedActorUI.Refresh( actor.Actor );
        }
        if( _selectedActor == null )
        {
            SelectedActorUI.Opt()?.gameObject.SetActive( false );
        }
    }

    public void RefreshContent()
    {
        SelectedActorUI.Opt()?.RefreshContent();
        ActorList.Opt()?.RefreshElementsContent();
    }
}
