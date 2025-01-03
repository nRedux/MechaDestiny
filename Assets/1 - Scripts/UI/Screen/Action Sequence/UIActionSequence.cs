using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIActionSequence : UIPanel
{
    private const int DEFAULT_MAX_BLOCKS = 5;
    private const int INFINITE_COST = int.MaxValue;

    public UIActionSequenceItem ItemPrefab;
    public HorizontalLayoutGroup ItemsRoot;

    public Image ExhaustedAPBar;

    private List<UIActionSequenceItem> _items = new List<UIActionSequenceItem>();
    private float _rootWidth = 0f;
    private float _itemsWidth = 0f;
    private int _maxPointUsable = 0;
    private int _actorPointUsable = 0;
    private RectTransform _itemsRootRT = null;

    public System.Action OnUIFire;


    public RectTransform ItemRootRT
    {
        get
        {
            return _itemsRootRT ??= ItemsRoot.Opt()?.GetComponent<RectTransform>();
        }
    }


    private float ItemsRootWidth
    {
        get => ItemsRoot != null ? ItemRootRT.rect.width : 1f;
    }


    private int PointsUsed => _items?.Select( x => GetSafeCostForAction( x.Action ) ).Sum() ?? INFINITE_COST;


    public List<ActorAction> GetSelectedSequence()
    {
        return _items.Select( x => x.Action ).ToList();
    }


    private void PreparePartitions( Actor actor )
    {
        if( actor == null )
            return;

        var maxAP = actor.GetStatistic( StatisticType.MaxAbilityPoints );
        _maxPointUsable = maxAP?.Value ?? DEFAULT_MAX_BLOCKS;

        var AP = actor.GetStatistic( StatisticType.AbilityPoints );
        _actorPointUsable = AP.Value;

        if( ExhaustedAPBar != null )
            ExhaustedAPBar.fillAmount = 1f - ( _actorPointUsable / (float)_maxPointUsable ) - ( ItemsRoot.padding.right / _rootWidth ) * 1;

        float spacingGain = ( _maxPointUsable - 1) * -ItemsRoot.spacing;

        _itemsWidth = (_rootWidth + spacingGain) / _maxPointUsable;
    }


    protected override void Awake()
    {
        base.Awake();
        if( ItemsRoot != null )
            _rootWidth = ItemRootRT.rect.width - ItemsRoot.padding.left - ItemsRoot.padding.right;
    }


    public void Show( Actor controlActor )
    {
        if( controlActor == null )
            return;

        PreparePartitions( controlActor );
        ItemRootRT.Opt()?.DestroyChildren();
        _items.Clear();
        base.Show();
    }


    private int GetSafeCostForAction( ActorAction action )
    {
        if( action == null )
            return 1;
        return Mathf.Max( action.Cost, 1 );
    }

    public void UIBtn_Fire()
    {
        OnUIFire?.Invoke();
    }

    public bool CanAddItem( ActorAction action )
    {
        if( action == null )
            return false;
        return _actorPointUsable - (PointsUsed + GetSafeCostForAction(action) ) >= 0;
    }


    public void AddItem( ActorAction action )
    {
        if( !CanAddItem( action ) )
            return;
        var newUIAction = CreateBlock( action );
    }


    private UIActionSequenceItem CreateBlock( ActorAction action )
    {
        var newInstance = ItemPrefab.Opt()?.Duplicate();

        ActionSequenceItemPart part = ActionSequenceItemPart.Left;
           
        newInstance.Opt()?.Initialize( action, part, this );
        newInstance.Opt()?.transform.SetParent( ItemRootRT, false );
        if( newInstance != null )
            _items.Add( newInstance );
        SizeBlock( newInstance );
        UpdateSpriteParts();

        return newInstance;
    }


    private void SizeBlock( UIActionSequenceItem item )
    {
        if( item == null )
            return;
        var rt = item.GetComponent<RectTransform>();
        var rect = rt.rect;
        rt.sizeDelta = new Vector2( _itemsWidth, rect.height );
    }


    private void UpdateSpriteParts()
    {
        for( int i = 0; i < _items.Count; i++ )
        {
            _items[i].UpdateSpritePart( GetSpritePart( i ) );
        }
    }


    private ActionSequenceItemPart GetSpritePart( int index )
    {
        if( _items.Count == 1 )
            return ActionSequenceItemPart.Whole;

        if( index == 0 )
            return ActionSequenceItemPart.Left;

        if( index > 0 && index < _items.Count - 1 )
            return ActionSequenceItemPart.Center;

        if( index == _items.Count - 1 )
            return ActionSequenceItemPart.Right;

        return ActionSequenceItemPart.Whole;
    }


    internal void RemoveActionFromSequence( UIActionSequenceItem item )
    {
        if( _items.Remove( item ) )
        {
            Destroy( item.gameObject );
            UpdateSpriteParts();
        }
    }

    public override void OnHide()
    {
        base.OnHide();
        OnUIFire = null;
    }
}
