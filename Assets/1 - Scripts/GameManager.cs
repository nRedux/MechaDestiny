using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using static UnityEngine.Rendering.DebugUI;

public delegate bool MapObjectFunc( GfxMapObject mapObject );

public delegate void MapObjectDelegate( GfxMapObject mapObject );
public delegate void MapObjectCollectionDelegate( IEnumerable<GfxMapObject> mapObjectCollection );

public class MxSelection
{

    private List<GfxMapObject> _selectedObjects = new List<GfxMapObject>();

    public List<GfxMapObject> SelectedObjects { get => SelectedObjects; }

    /// <summary>
    /// Can be subscribed to to allow normal selection but also allow the interaction which would normally select to be intercepted.
    /// </summary>
    public MapObjectFunc SelectionIntercept;

    public MapObjectDelegate OnSelectionAdd;
    public MapObjectDelegate OnSelectionRemove;
    public MapObjectDelegate OnEnter;
    public MapObjectDelegate OnExit;
    public System.Action OnSelectionClear;
    public MapObjectCollectionDelegate OnSelectionChanged;


    private GfxMapObject _lastHovered;


    public bool DoNormalSelection { get; set; } = true;


    private bool HasSelectionInput()
    {
        return Input.GetMouseButtonDown( 0 );
    }


    public void Add( GfxMapObject obj )
    {
        if( obj == null )
            return;
        if( _selectedObjects.Contains( obj ) )
            return;

        obj.OnSelected();

        _selectedObjects.Add( obj );
        OnSelectionAdd?.Invoke( obj );
        OnSelectionChanged?.Invoke( _selectedObjects );
    }


    public void Remove( GfxMapObject obj )
    {
        if( obj == null )
            return;
        if( !_selectedObjects.Contains( obj ) )
            return;

        obj.OnDeselected();

        _selectedObjects.Remove( obj );
        OnSelectionRemove?.Invoke( obj );
        if( _selectedObjects.Count == 0 )
            OnSelectionClear?.Invoke();
        OnSelectionChanged?.Invoke( _selectedObjects );
    }


    public void Clear()
    {
        _selectedObjects.NonNull().Do( x => x.OnDeselected() );
        _selectedObjects.Clear();
        OnSelectionClear?.Invoke();
        OnSelectionChanged?.Invoke( _selectedObjects );
    }


    public void Update()
    {
        GfxMapObject hoverObj = FindHoveredObject();
        ProcessFrameHover( hoverObj );
        DoSelection( hoverObj );
    }


    private void ProcessFrameHover( GfxMapObject thisFrameHover )
    {
        //Cache last so any subsequent exceptions if possible aren't capable of invalidating normal hover behavior.
        var lastHoverTemp = _lastHovered;
        //Assign before subsequent activities.
        _lastHovered = thisFrameHover;

        if( lastHoverTemp != thisFrameHover )
        {
            if( lastHoverTemp != null )
            {
                try
                {
                    OnExit?.Invoke( lastHoverTemp );
                }
                catch(System.Exception ex)
                {
                    //Following and caller protected
                    Debug.LogException(ex);
                }
            }
            if( thisFrameHover != null )
            {
                try
                {
                    OnEnter?.Invoke( thisFrameHover );
                }
                catch( System.Exception ex )
                {
                    //calling code protected
                    Debug.LogException( ex );
                }
            }
        }
    }


    private GfxMapObject FindHoveredObject()
    {
        GfxMapObject result = null;
        if( !EventSystem.current.IsPointerOverGameObject() )
        {
            RaycastHit hit;
            if( GameManager.RaycastForSelectables( out hit ) )
            {
                result = hit.collider.GetComponentInParent<GfxMapObject>();
            }
        }
        return result;
    }


    private void DoSelection( GfxMapObject interactedObject )
    {
        if( !DoNormalSelection )
            return;
        if( !HasSelectionInput() )
            return;
        if( EventSystem.current.IsPointerOverGameObject() )
            return;

        if( interactedObject != null )
        {
            var intercept = SelectionIntercept?.Invoke( interactedObject );
            if( intercept == true )
                return;

            if( _selectedObjects.Contains( interactedObject ) )
                Remove( interactedObject );
            else
                Add( interactedObject );
        }
        else
        {
            Clear();
        }

    }
}

public class MxSelectionProcessor
{
    private MxSelection _selection;
    private List<GfxMapObjectAction> _actions = null;
    private GfxMapObjectAction _selectedAction = null;

    private GfxMapObject _hoveredObject;

    public MxSelectionProcessor( MxSelection selection )
    {
        _selection = selection;
        if( _selection == null )
            return;

        selection.OnSelectionChanged += SelectionChanged;
        selection.OnSelectionClear += SelectionCleared;
        selection.OnEnter += OnObjectEnter;
        selection.OnExit += OnObjectExit;
        selection.SelectionIntercept += InterceptSelection;
    }


    private bool InterceptSelection( GfxMapObject toSelect )
    {
        return false;
    }

    private void SelectionCleared()
    {
        MapUIManager.Instance.ActionView?.Hide();
    }


    public void SelectionChanged( IEnumerable<GfxMapObject> objects )
    {
        if( objects == null || objects.Count() == 0 )
        {
            _actions = null;
            return;
        }

        var comp = new MapObjectActionComparer();
        var lists = objects.Select( x => x.Actions );
        
        _actions = lists.Aggregate( ( p, n ) => p.Intersect( n, comp ).ToList() );
        UpdateUIActionOptions( _actions, x => { SelectAction( x, true ); } );
    }


    private void UpdateUIActionOptions( List<GfxMapObjectAction> actions, System.Action<GfxMapObjectAction> onClick )
    {
        if( MapUIManager.InstanceExists )
        {
            MapUIManager.Instance.ActionView?.Refresh( actions, onClick );
            MapUIManager.Instance.ActionView?.Show();
        }
    }


    private void SelectAction( GfxMapObjectAction selected, bool uiClick )
    {
        if( selected == _selectedAction )
            return;

        if( _selectedAction != null )
        {
            _selectedAction.SelectedStop();
        }
        _selectedAction = selected;

        _selection.DoNormalSelection = _selectedAction == null;
        if( selected == null )
            return;
        //Need to handle action startup, ui presentation, then response when completed.
        selected.SelectedStart( SelectedCancelled, new MapObjActionSelectArgs() { UIClick = uiClick } ); 
    }


    public void Update()
    {
        if( _selectedAction != null && !_selectedAction.SelectArgs.UIClick && !_selectedAction.WantsActivation( _hoveredObject, _selectedAction ) )
            _selectedAction = null;

        if( !EventSystem.current.IsPointerOverGameObject() )
        {
            var toActivate = _actions?.Where( x => x.WantsActivation( _hoveredObject, _selectedAction ) ).FirstOrDefault();
            if( toActivate != null )
                SelectAction( toActivate, false );
        }

        if( _selectedAction == null ) return;

        var result = _selectedAction.SelectedUpdate( _hoveredObject );
        if( result != null )
        {
            ActivateActions( result );
        }

        //_selectedAction can be null now, if cancellation happened.
        if( _selectedCancelled )
        {
            _selection.DoNormalSelection = true;
            _selectedAction = null;
            CursorManager.Instance.SetCursorMode( CursorMode.Normal );
        }
        _selectedCancelled = false;
    }


    private bool _selectedCancelled = false;
    private void SelectedCancelled()
    {
        _selectedCancelled = true;
    }


    private void ActivateActions( object arg )
    {
        var actionsOfType = _actions.Where( x => x.GetType() == _selectedAction.GetType() );
        actionsOfType.Do( x => x.Activate( arg ) );
        _selectedAction = null;
        _selection.DoNormalSelection = true;
        CursorManager.Instance.SetCursorMode( CursorMode.Normal );
    }


    private void OnObjectEnter( GfxMapObject mapObj )
    {
        _hoveredObject = mapObj;
        _selectedAction.Opt()?.ObjectHover( _hoveredObject );
    }


    private void OnObjectExit( GfxMapObject mapObj )
    {
        if( _hoveredObject == mapObj )
            _hoveredObject = null;

        _selectedAction.Opt()?.ObjectHover( _hoveredObject );
    }
}


public class GameManager : Singleton<GameManager>
{

    public const float SKY_HEIGHT = 1000f;

    public MapObjectReference Harvester;
    public EncounterData TestEncounter;
    public MxSelection Selection;
    public MxSelectionProcessor SelectionProcessor;
    
    
    private MapObjectData _playerHarvester;


    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad( gameObject );
        //Get the system UI fired up
        var inst = SUIManager.Instance;

        InitializeCaravan();

        Selection = new MxSelection();
        Selection.OnSelectionAdd += SelectionAdd;
        Selection.OnSelectionRemove += SelectionRemove;
        Selection.OnSelectionClear += SelectionCleared;

        SelectionProcessor = new MxSelectionProcessor( Selection );

    }

    private void SelectionAdd( GfxMapObject selected )
    {
        Debug.Log( $"Selected {selected.name}" );
    }

    private void SelectionRemove( GfxMapObject selected )
    {
        Debug.Log( $"Deselected {selected.name}" );
    }

    private void SelectionCleared()
    {
        Debug.Log( "Selection cleared" );
    }

    private void ValidateData( MapObjectData harvesterData )
    {
        RunData rd = RunManager.Instance.RunData;

        rd.MapData = new MapData();

        if( !rd.IsValid() )
        {
            RunManager.Instance.RunData.Validate( harvesterData );
        }
    }

    private void InitializeCaravan()
    {
        _playerHarvester = Harvester.GetDataSync();
        ValidateData( _playerHarvester );
    }

    private void Start()
    {
        EncounterManager.Instance.AddEncounter( TestEncounter );

    }


    // Update is called once per frame
    void Update()
    {
        if( TimeManager.Instance != null )
            TimeManager.Instance.Update();

        _playerHarvester.Tick( TimeManager.Instance.DayData.HoursDelta );

        if( RunManager.Instance.RunData.SceneNeedsWarmup() )
            RunManager.Instance.RunData.DoSceneWarmup();

        Selection.Update();
        SelectionProcessor.Update();
    }

    public static bool RaycastGroundFromMouse( out RaycastHit hit )
    {
        var ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        return Physics.Raycast( ray, out hit, 1000, 1 << LayerMask.NameToLayer( "Ground" ) );
    }

    public static bool RaycastForSelectables( out RaycastHit hit )
    {
        var ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        return Physics.Raycast( ray, out hit, 1000, 1 << LayerMask.NameToLayer( "Selectable" ) );
    }

    public static bool RaycastGround( Vector3 position, out RaycastHit hit )
    {
        position.y = SKY_HEIGHT;
        var ray = new Ray( position, Vector3.down );
        return Physics.Raycast( ray, out hit, SKY_HEIGHT + 100, 1 << LayerMask.NameToLayer( "Ground" ) );
    }

}
