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
        var sequenceAction = content.SequenceAction;
        UpdateLocalization( sequenceAction.Actor, sequenceAction );
    }

    private void UpdateLocalization( Actor actor, SequenceAction action )
    {
        var titleEvent = Title.GetComponent<LocalizeStringEvent>();
        var descEvent = ActionDescription.GetComponent<LocalizeStringEvent>();

        titleEvent.StringReference = action.Action.DisplayName;
        descEvent.StringReference = action.Action.Description;
        Cost = action.GetCost();
        RefreshLocalizedStrings();
    }

}
