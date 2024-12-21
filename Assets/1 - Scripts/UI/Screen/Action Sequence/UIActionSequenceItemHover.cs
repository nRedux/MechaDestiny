using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

public class UIActionSequenceItemHover : UIPanel
{

    public TextMeshProUGUI Title;
    public TextMeshProUGUI ActionDescription;
    public TextMeshProUGUI ActionCost;

    [HideInInspector]
    public int Cost;

    public void Refresh( UIActionSequenceItem content )
    {
        var action = content.Action;

        Title.Opt()?.SetText( action.DisplayName.TryGetLocalizedString() );
        ActionDescription.Opt()?.SetText( action.Description.TryGetLocalizedString() );
        ActionCost.Opt()?.SetText( action.Cost.ToString() );
        UpdateLocalization( action );
    }

    private void UpdateLocalization( ActorAction action )
    {
        var titleEvent = Title.GetComponent<LocalizeStringEvent>();
        var descEvent = ActionDescription.GetComponent<LocalizeStringEvent>();

        titleEvent.StringReference = action.DisplayName;
        descEvent.StringReference = action.Description;
        Cost = action.Cost;
        RefreshLocalizedStrings();
    }

}
