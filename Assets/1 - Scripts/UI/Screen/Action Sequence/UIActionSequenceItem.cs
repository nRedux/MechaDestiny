using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public enum ActionSequenceItemPart
{
    Whole,Left,Center,Right
}

public class UIActionSequenceItem: MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private const string ANIMATOR_PREVIEW_SHOW = "Show";

    [HideInInspector]
    public UIActionSequence UISequence;
    public GameObject PipDividerTemplate;
    public GameObject SpacerTemplate;
    public Animator PreviewAnimator;
    public int OverlapStretch = 10;

    public Color HoveredTint;

    public SequenceAction SequenceAction;
    public TextMeshProUGUI ActionName;
    
    public Mask Mask;
    public Image MaskImage;

    public Sprite LeftSprite;
    public Sprite CenterSprite;
    public Sprite RightSprite;


    private Color _startColor;
    private HorizontalLayoutGroup _layoutGroup;
    private GameObject _spacer = null;


    private void Awake()
    {
        PipDividerTemplate.SetActive( true );
        _layoutGroup = transform.GetComponent<HorizontalLayoutGroup>();

        PipDividerTemplate.Opt()?.SetActive( false );
        SpacerTemplate.Opt()?.SetActive( false );
        _startColor = MaskImage.Opt()?.color ?? Color.white;

        if( PreviewAnimator == null )
            PreviewAnimator = GetComponentInChildren<Animator>();
        if( PreviewAnimator != null )
            PreviewAnimator.enabled = false;
    }

    public void ShowAsPreview()
    {
        if( PreviewAnimator != null )
        {
            PreviewAnimator.enabled = true;
            PreviewAnimator.Play( ANIMATOR_PREVIEW_SHOW, 0, 0 );
        }
    }

    public void UpdateStretch( ActionSequenceItemPart part )
    {
        if( _layoutGroup == null )
            return;
        /*
        switch( part )
        {
            case ActionSequenceItemPart.Left:
                _layoutGroup.padding.right = OverlapStretch;
                break;

            case ActionSequenceItemPart.Right:
                _layoutGroup.padding.left = OverlapStretch;
                break;

            case ActionSequenceItemPart.Center:
                _layoutGroup.padding.left = OverlapStretch;
                break;
        }
        */
    }

    public void CreateDividers( int subBlocks, ActionSequenceItemPart part )
    {
        if( _layoutGroup == null )
            return;
        if( subBlocks <= 1 )
            return;

        int realBlockCount = subBlocks - 1;

        var spacerInst = Instantiate<GameObject>( SpacerTemplate );
        spacerInst.transform.SetParent( transform, false );
        spacerInst.SetActive( true );
        for( int i = 0; i < realBlockCount; i++ )
        {
            var pipInst = Instantiate<GameObject>( PipDividerTemplate );
            pipInst.transform.SetParent( transform, false );
            pipInst.SetActive( true );

            spacerInst = Instantiate<GameObject>( SpacerTemplate );
            spacerInst.transform.SetParent( transform, false );
            spacerInst.SetActive( true );
        }
    }


    public void Initialize( SequenceAction action, ActionSequenceItemPart part, UIActionSequence sequence )
    {
        if( action == null )
            return;
        UISequence = sequence;
        UpdateSpritePart( part );
        this.SequenceAction = action;
        this.ActionName.Opt()?.SetText( action?.Action.DisplayName.TryGetLocalizedString() ?? string.Empty );
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
        return SequenceAction.Action;
    }


    public void SetColor( Color color )
    {
        if( MaskImage != null )
            MaskImage.color = color;
    }


    public void OnPointerClick( PointerEventData eventData )
    {
        if( eventData.button == PointerEventData.InputButton.Right )
        {
            UISequence.Opt()?.RemoveSequenceAction( this );
        }
    }


    public void OnPointerEnter( PointerEventData eventData )
    {
        var hoverUI = UIManager.Instance.ActionSequenceHover;
        if( hoverUI == null )
            return;

        if( MaskImage != null )
            MaskImage.CrossFadeColor( HoveredTint, .1f, true, true );

        var indicator = UIManager.Instance.WorldIndicators.GetIndicator( SequenceAction.Target );
        indicator.Opt()?.StartHighlight();


        hoverUI.Refresh( this );
        hoverUI.transform.position = this.transform.position;
        hoverUI.Show();
    }


    public void OnPointerExit( PointerEventData eventData )
    {
        if( MaskImage != null )
            MaskImage.CrossFadeColor( _startColor, .1f, true, true );

        var indicator = UIManager.Instance.WorldIndicators.GetIndicator( SequenceAction.Target );
        indicator.Opt()?.StopHighlight();

        UIManager.Instance.ActionSequenceHover.Opt()?.Hide();
    }
}
