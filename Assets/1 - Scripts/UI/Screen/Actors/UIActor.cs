using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Edgeflow.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;


public enum UIACtorMode
{
    Small,
    Full
}

public class UIActor : MonoBehaviour, IUIItem<Actor>
{

    public UIACtorMode Mode;

    public GameObject MechFieldRoot;

    public TMP_Text Name;
    public TMP_Text Class;
    public TMP_Text Activity;
    public TMP_Text MechName;
    public Image Portrait;

    [ShowIf(nameof(ShowIfFull))]
    public GameObject MechUIRoot;

    
    private Actor _actor;
    private TMProLinkHandler _mechLinkHandler;
    private Button _button;

    public Actor Actor
    {
        get => _actor;
    }
    public Action<IUIItem<Actor>> Clicked { get; set; }

    public bool ShowIfFull()
    {
        return Mode == UIACtorMode.Full;
    }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.Opt()?.onClick.AddListener( OnClicked );
        _mechLinkHandler = MechName.GetComponent<TMProLinkHandler>();
        if( _mechLinkHandler != null )
        {
            _mechLinkHandler.Click = MechNameClicked;
        }
    }

    private void MechNameClicked( string id )
    {
        UIRunInfo.Instance.SelectMech( Actor.PilotedMech );
    }


    private void OnClicked()
    {
        Clicked?.Invoke( this );
    }


    public void Refresh( Actor actor )
    {
        if( actor == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( actor )}' cannot be null" );
        _actor = actor;

        RefreshContent();
    }

    private string GetMechNameText( string linkID, string mechName )
    {
        return $"<link=\"{linkID}\">{mechName}</link>";
    }

    public void PickMechToPilot()
    {
        UIRunInfo.Instance.MechSelector.SelectFromPlayerMechs( targetMech =>
        {
            if( _actor.PilotedMech != null )
            {
                if( targetMech.Pilot != null )
                {
                    //we HAVE a mech, AND the target mech has a pilot. We're swapping?
                    UIRunInfo.Instance.OptionDialog.ShowAcceptCancel( "Pilots swap", "Pilots will be swapping mechs.", () => {
                        //Swap pilots. Make both exit their mechs, and then assign to the mechs.
                        var otherPilot = targetMech.Pilot;
                        otherPilot.StopPilotingMech();
                        var myOldMech = _actor.PilotedMech;
                        _actor.StopPilotingMech();

                        _actor.StartPilotingMech( targetMech );
                        otherPilot.StartPilotingMech( myOldMech );
                        UIRunInfo.Instance.EmployeeInfoPage.RefreshContent();

                    }, () => {
                        //Cancel, nothing to do.
                    } );
                }
                else
                {
                    //Ez mode again, target mech isn't piloted, but we do have to stop piloting the mech we're in.
                    _actor.StartPilotingMech( targetMech );
                }
            }
            else
            {
                if( targetMech.Pilot != null )
                {
                    //we have no mech, but target mech has a pilot. We just kicking them out of their mech?
                    UIRunInfo.Instance.OptionDialog.ShowAcceptCancel( "Mech occupied", "Mech is piloted. If you accept, they won't have a mech.", () => {
                        //Target pilot has to leave their mech.
                        targetMech.Pilot.StopPilotingMech();
                        _actor.StartPilotingMech( targetMech );

                    }, () => { } );
                }
                else
                {
                    //EZ mode, mech is unpiloted, lets take it. We're also not piloting a mech atm.
                    _actor.StartPilotingMech( targetMech );
                }
            }
            UIRunInfo.Instance.EmployeeInfoPage.RefreshContent();
        }, excludeActors: new List<Actor>() { _actor } );
    }

    internal void RefreshContent()
    {
        var actorAsset = _actor.GetAssetSync();
        if( Name != null )
            Name.text = actorAsset.DisplayName.GetLocalizedString();

        if( Portrait != null )
            Portrait.sprite = actorAsset.PortraitImage;

        Activity.Opt()?.SetText( _actor.Activity.ToString() );

        if( _actor.PilotedMech != null )
        {
            var mechAsset = _actor.PilotedMech.GetAssetSync();
            if( mechAsset != null )
            {
                MechFieldRoot.Opt()?.SetActive( true );
                string mechName = mechAsset.DisplayName.TryGetLocalizedString();
                MechName.Opt()?.SetText( GetMechNameText( _actor.PilotedMech.DataID, mechName ) );
            }
        }
        else
            MechFieldRoot.Opt()?.SetActive( false );
    }
}
