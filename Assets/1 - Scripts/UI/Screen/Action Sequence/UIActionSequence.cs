//Available defines
//USE_ACTION_BLOCKS

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Build;
using Unity.VisualScripting;
using System;


public class UIActionSequence : UIPanel
{
    private const bool USE_ACTION_BLOCKS = false;
    private const int DEFAULT_MAX_BLOCKS = 5;
    private const int INFINITE_COST = int.MaxValue;

    public UIActionSequenceItem ItemPrefab;
    public HorizontalLayoutGroup ItemsRoot;
    public Image ExhaustedAPBar;
    public Color PreviewColor;

    private List<UIActionSequenceItem> _items = new List<UIActionSequenceItem>();
    private float _rootWidth = 0f;
    private int _maxActionBlocks = 0;
    private int _spentAbilityPoints = 0;
    private RectTransform _itemsRootRT = null;
    private ActionSequence _sequence = null;

    private UIActionSequenceItem _previewBlock;

    public System.Action OnUIFire;


    public RectTransform ItemRootRT
    {
        get
        {
            return _itemsRootRT ??= ItemsRoot.Opt()?.GetComponent<RectTransform>();
        }
    }


#if USE_ACTION_BLOCKS
    private int BlocksUsed => _items?.Select( x => GetBlockCostForAction( x.SequenceAction.Action ) ).Sum() ?? INFINITE_COST;
#endif

    protected override void Awake()
    {
        base.Awake();
        if( ItemsRoot != null )
            _rootWidth = ItemRootRT.rect.width - ItemsRoot.padding.left - ItemsRoot.padding.right;
    }

    public List<SequenceAction> GetSelectedSequence()
    {
        return _items.Where( x => !x.IsPreview ).Select( x => x.SequenceAction ).ToList();
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


    private void UpdateExhaustedUI()
    {
        float padding = ItemsRoot.padding.left - ItemsRoot.padding.right;
        float wid = ItemRootRT.rect.width - (CalculateBlockWidth( _maxActionBlocks - _spentAbilityPoints ) + ItemsRoot.spacing - padding);
        float fill = wid / ( ItemRootRT.rect.width - padding);
        if( ExhaustedAPBar != null )
            ExhaustedAPBar.fillAmount = 1f - fill;
    }


    private void DestroyBlockInstances()
    {
        ItemRootRT.Opt()?.DestroyChildren();
        _items.Clear();
    }

    public void Show( Actor actor, ActionSequence sequence )
    {
        if( actor == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( actor )}' can not be null." );
        if( sequence == null )
            throw new System.ArgumentNullException( $"Argument '{nameof(sequence)}' can not be null." );

        _sequence = sequence;
        _sequence.ActionAdded += SequenceActionAdded;
        _sequence.ActionRemoved += SequenceActionRemoved;

        EvaluateActorStats( actor );
        UpdateExhaustedUI();
        DestroyBlockInstances();

        base.Show();
    }

    public override void Hide()
    {
        _sequence?.ClearListeners();
        _sequence = null;
        base.Hide();
    }

#if USE_ACTION_BLOCKS
    private int GetBlockCostForAction( ActorAction action )
    {
        if( action == null )
            return 1;
        return Mathf.Max( action.BlocksUsed, 1 );
    }
#endif

    public void UIBtn_Fire()
    {
        OnUIFire?.Invoke();
    }

    public void AddSequenceAction( SequenceAction action )
    {
        if( !_sequence.CanAddSequenceAction( action ) )
            return;
        _sequence.AddAction( action );
    }


    internal void RemoveSequenceAction( UIActionSequenceItem item )
    {
        _sequence.RemoveAction( item.SequenceAction );
    }

    private void SequenceActionAdded( SequenceAction action )
    {
        CreateBlock( action );
        UpdateSpriteParts();

        //Shift preview block to end, if it exists
        if( _previewBlock != null )
        {
            if( _items.Remove( _previewBlock ) )
                _items.Add( _previewBlock );
            _previewBlock?.transform.SetAsLastSibling();
        }

        UpdateSpriteParts();
        UIManager.Instance.WorldIndicators.TryCreateIndicatorOnSmartPos( GfxWorldIndicators.ATTACK_INDICATOR, action.Target );
    }

    public void PreviewAction( SequenceAction action )
    {
        EndPreviewAction();
        //Don't preview that which shalt not be showable!
        if( !_sequence.CanAddSequenceAction( action ) )
            return;

        _previewBlock = CreateBlock( action );
        _previewBlock.gameObject.name = "Preview_Block";
        //_previewBlock.SetColor( PreviewColor );
        UpdateSpriteParts();
        _previewBlock?.ShowAsPreview();
    }


    public void EndPreviewAction()
    {
        if( _previewBlock == null )
            return;
        _items.Remove( _previewBlock );
        Destroy( _previewBlock.gameObject );
        _previewBlock = null;
        UpdateSpriteParts();
    }


    private void SequenceActionRemoved( SequenceAction action )
    {
        var uiItem = _items.FirstOrDefault( x => x.SequenceAction == action );

        if( _items.Remove( uiItem ) )
        {
            Destroy( uiItem.gameObject );
            UpdateSpriteParts();

            if( !IsTargetInSequence( uiItem.SequenceAction.Target ) )
                UIManager.Instance.WorldIndicators.DestroyIndicator( uiItem.SequenceAction.Target, true );
        }
    }

    private void OnDestroy()
    {
        _sequence?.ClearListeners();
        _sequence = null;
    }

    private int GetBlocksRequired( SequenceAction action )
    {
#if USE_ACTION_BLOCKS
        return action.Action.BlocksUsed;
#else
        return action.GetCost();
#endif
    }


    private UIActionSequenceItem CreateBlock( SequenceAction action )
    {
        var newInstance = ItemPrefab.Opt()?.Duplicate();
        if( newInstance == null )
            return null;
        _items.Add( newInstance );
        int subBlocksRequired = GetBlocksRequired( action );
        ActionSequenceItemPart part = ActionSequenceItemPart.Left;
           
        newInstance.Opt()?.Initialize( action, part, this );
        newInstance.Opt()?.transform.SetParent( ItemRootRT, false );
        SizeBlock( newInstance, subBlocksRequired );
        newInstance.CreateDividers( subBlocksRequired, part );
        return newInstance;
    }


    private void SizeBlock( UIActionSequenceItem item, int widthCount )
    {
        if( item == null )
            return;
        var rt = item.GetComponent<RectTransform>();

        rt.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, CalculateBlockWidth( widthCount ) );// = new Vector2( , rect.height );
    }

    private float CalculateBlockWidth( int widthCount )
    {
        float itemsWidth = ( _rootWidth + ItemsRoot.spacing ) / ( _maxActionBlocks );
        return ( itemsWidth * widthCount ) - ItemsRoot.spacing;
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
        int itemsCount = _items.Count;

        if( itemsCount == 1 )
            return ActionSequenceItemPart.Whole;

        if( index == 0 )
            return ActionSequenceItemPart.Left;

        if( index > 0 && index < itemsCount - 1 )
            return ActionSequenceItemPart.Center;

        if( index >= itemsCount - 1 )
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
