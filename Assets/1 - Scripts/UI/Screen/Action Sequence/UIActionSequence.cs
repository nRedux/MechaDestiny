//Available defines
//USE_ACTION_BLOCKS

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Build;

public class UIActionSequence : UIPanel
{
    private const bool USE_ACTION_BLOCKS = false;
    private const int DEFAULT_MAX_BLOCKS = 5;
    private const int INFINITE_COST = int.MaxValue;

    public UIActionSequenceItem ItemPrefab;
    public HorizontalLayoutGroup ItemsRoot;

    public Image ExhaustedAPBar;

    private List<UIActionSequenceItem> _items = new List<UIActionSequenceItem>();
    private float _rootWidth = 0f;
    private float _itemsWidth = 0f;
    private int _maxActionBlocks = 0;
    private int _spentAbilityPoints = 0;
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


    private int PointsUsed => _items?.Select( x => GetAPCostForAction( x.SequenceAction.Action ) ).Sum() ?? INFINITE_COST;
    private int BlocksUsed => _items?.Select( x => GetBlockCostForAction( x.SequenceAction.Action ) ).Sum() ?? INFINITE_COST;


    public List<SequenceAction> GetSelectedSequence()
    {
        return _items.Select( x => x.SequenceAction ).ToList();
    }


    private void EvaluateActorStats( Actor actor )
    {
        if( actor == null )
            return;

#if USE_ACTION_BLOCKS
        var maxAP = actor.GetStatistic( StatisticType.MaxActionBlocks );
#else
        var maxBlocksStat = actor.GetStatistic( StatisticType.MaxAbilityPoints );
#endif
        _maxActionBlocks = maxBlocksStat?.Value ?? DEFAULT_MAX_BLOCKS;
        var apStat = actor.GetStatistic( StatisticType.AbilityPoints );
        _spentAbilityPoints = apStat.Value;
    }


    private void CalculateUILayout()
    {
        if( ExhaustedAPBar != null )
            ExhaustedAPBar.fillAmount = 1f - ( _spentAbilityPoints / (float)_maxActionBlocks ) - ( ItemsRoot.padding.right / _rootWidth ) * 1;

        float spacingGain = ( _maxActionBlocks - 1) * -ItemsRoot.spacing;

        _itemsWidth = (_rootWidth + spacingGain) / _maxActionBlocks;
    }


    protected override void Awake()
    {
        base.Awake();
        if( ItemsRoot != null )
            _rootWidth = ItemRootRT.rect.width - ItemsRoot.padding.left - ItemsRoot.padding.right;
    }

    private void DestroyBlockInstances()
    {
        ItemRootRT.Opt()?.DestroyChildren();
        _items.Clear();
    }

    public void Show( Actor controlActor )
    {
        if( controlActor == null )
            return;

        EvaluateActorStats( controlActor );
        CalculateUILayout();
        DestroyBlockInstances();

        base.Show();
    }


    private int GetAPCostForAction( ActorAction action )
    {
        if( action == null )
            return 1;
        return Mathf.Max( action.APCost, 1 );
    }

    private int GetBlockCostForAction( ActorAction action )
    {
        if( action == null )
            return 1;
        return Mathf.Max( action.BlocksUsed, 1 );
    }

    public void UIBtn_Fire()
    {
        OnUIFire?.Invoke();
    }

    public bool CanAddSequenceAction( SequenceAction action )
    {
        if( action == null )
            return false;

        bool canAffordAP = _spentAbilityPoints - ( PointsUsed + GetAPCostForAction( action.Action ) ) >= 0;

#if USE_ACTION_BLOCKS
        bool canAffordBlocks = _maxActionBlocks - ( BlocksUsed + GetBlockCostForAction( action.Action ) ) >= 0;
        return canAffordAP && canAffordBlocks;
#else
        return canAffordAP;
#endif
    }


    public void AddSequenceAction( SequenceAction action )
    {
        if( !CanAddSequenceAction( action ) )
            return;
        var newUIAction = CreateBlock( action );

        UIManager.Instance.WorldIndicators.TryCreateIndicatorOnSmartPos( GfxWorldIndicators.ATTACK_INDICATOR, action.Target );
    }


    internal void RemoveSequenceAction( UIActionSequenceItem item )
    {
        if( _items.Remove( item ) )
        {
            Destroy( item.gameObject );
            UpdateSpriteParts();

            if( !IsTargetInSequence( item.SequenceAction.Target ) )
                UIManager.Instance.WorldIndicators.DestroyIndicator( item.SequenceAction.Target, true );      
        }
    }


    private int GetBlocksRequired( SequenceAction action )
    {
#if USE_ACTION_BLOCKS
        return action.Action.BlocksUsed;
#else
        return action.Action.APCost;
#endif
    }

    private UIActionSequenceItem CreateBlock( SequenceAction action )
    {
        var newInstance = ItemPrefab.Opt()?.Duplicate();
        int blocksRequired = GetBlocksRequired( action );
        ActionSequenceItemPart part = ActionSequenceItemPart.Left;
           
        newInstance.Opt()?.Initialize( action, part, this );
        newInstance.Opt()?.transform.SetParent( ItemRootRT, false );
        if( newInstance != null )
            _items.Add( newInstance );
        SizeBlock( newInstance, blocksRequired );
        newInstance.CreateDividers( blocksRequired, part );
        UpdateSpriteParts();

        return newInstance;
    }


    private void SizeBlock( UIActionSequenceItem item, int widthCount )
    {
        if( item == null )
            return;
        var rt = item.GetComponent<RectTransform>();
        var rect = rt.rect;
        rt.sizeDelta = new Vector2( _itemsWidth * widthCount, rect.height );
    }


    private void UpdateSpriteParts()
    {
        for( int i = 0; i < _items.Count; i++ )
        {
            _items[i].UpdateSpritePart( GetSpritePart( i ) );
            _items[i].UpdateStretch( GetSpritePart( i ) );
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

    private bool IsTargetInSequence( SmartPoint target )
    {
        return _items.Exists( x =>
        {
            //We have to just rely on position - this could later be a source of bugs if position can be moved as part of an attack.
            //Actors moving as part of an attack will cause bugs either way if indicators are cell based though.
            return x.SequenceAction.Target.Position.ToVector2Int() == target.Position.ToVector2Int();
        } );
    }

    public override void OnHide()
    {
        base.OnHide();
        OnUIFire = null;
        UIManager.Instance.WorldIndicators.DestroyAllIndicators( true );
    }
}
