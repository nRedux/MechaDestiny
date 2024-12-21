using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public enum ActionSequenceItemPart
{
    Whole,Left,Center,Right
}

public class UIActionSequenceItem: MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public UIActionSequence UISequence;

    public ActorAction Action;
    public TextMeshProUGUI ActionName;
    
    public Mask Mask;
    public Image MaskImage;

    public Sprite LeftSprite;
    public Sprite CenterSprite;
    public Sprite RightSprite;

    public void Initialize( ActorAction action, ActionSequenceItemPart part, UIActionSequence sequence )
    {
        if( action == null )
            return;
        UISequence = sequence;
        UpdateSpritePart( part );
        this.Action = action;
        this.ActionName.Opt()?.SetText( action?.DisplayName.TryGetLocalizedString() ?? string.Empty );
    }

    public void UpdateSpritePart( ActionSequenceItemPart part )
    {
        if( Mask != null )
        {
            Mask.enabled = part != ActionSequenceItemPart.Whole;
        }

        if( MaskImage != null )
        {
            MaskImage.sprite = GetImageForPart( part );
        }
    }

    private Sprite GetImageForPart( ActionSequenceItemPart part )
    {
        switch( part )
        {
            case ActionSequenceItemPart.Left:
                return LeftSprite;
            case ActionSequenceItemPart.Center:
                return CenterSprite;
            case ActionSequenceItemPart.Right:
                return RightSprite;
            default:
                return null;
        }
    }

    public ActorAction Cancel()
    {
        return Action;
    }

    public void OnPointerClick( PointerEventData eventData )
    {
        if( eventData.button == PointerEventData.InputButton.Right )
        {
            UISequence.Opt()?.RemoveActionFromSequence( this );
        }
    }

    public void OnPointerEnter( PointerEventData eventData )
    {
        var hoverUI = UIManager.Instance.ActionSequenceHover;
        if( hoverUI == null )
            return;

        hoverUI.Refresh( this );
        hoverUI.transform.position = this.transform.position;
        hoverUI.Show();
    }

    public void OnPointerExit( PointerEventData eventData )
    {
        UIManager.Instance.ActionSequenceHover.Opt()?.Hide();
    }
}
