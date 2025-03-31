using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public delegate bool MapObjectFunc( GfxMapObject mapObject );

public delegate void MapObjectDelegate( GfxMapObject mapObject );
public delegate void MapObjectCollectionDelegate( IEnumerable<GfxMapObject> mapObjectCollection );

public class GfxMap : Singleton<GfxMap>
{

    public const float SKY_HEIGHT = 1000f;

    public MapObjectReference Harvester;
    public EncounterData TestEncounter;
    public MxSelection Selection;
    public MxSelectionProcessor SelectionProcessor;

    public Transform StartPos;
    public Transform TargetPos;

    private GfxMoveableMapObject _caravanGfx = null;

    [Button]
    public void DoLUATest()
    {
        //Create a way to extract variable 

        Script script = new Script();
        script.DoString( @"
            a = nil;
            b = 20;
            __exposed__ = { [""a""] = ""GameObject"" }
            function myFunc() end" );

        var table = script.Globals.Get( "__exposed__" );
        
        if( table != null )
        {
            foreach( var item in table.Table.Pairs )
            {
                Debug.Log( $"{item.Key}: {item.Value.ToString()}" );
            }
        }

        string typeName = "MyNamespace.MyClass";
        Type type = null;

        // Iterate through all loaded assemblies
        foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
        {
            type = assembly.GetType( typeName );
            if( type != null )
            {
                break;
            }
        }

        foreach( var key in script.Globals.Keys )
        {
            Debug.Log( key );
        }
    }

    
    private MapObjectData _playerHarvester;

    protected override void Awake()
    {
        //Make sure it's alive
        var co = CoroutineUtils.Instance;

        base.Awake();
        //DontDestroyOnLoad( gameObject );
        //Get the system UI fired up
        var inst = SUIManager.Instance;

        Selection = new MxSelection();
        Selection.OnSelectionAdd += SelectionAdd;
        Selection.OnSelectionRemove += SelectionRemove;
        Selection.OnSelectionClear += SelectionCleared;

        SelectionProcessor = new MxSelectionProcessor( Selection );

        TimeManager.StartTime();

        Events.Instance.AddListener<DoSceneWarmup>( OnSceneWarmup );

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Events.Instance.RemoveListener<DoSceneWarmup>( OnSceneWarmup );
    }

    private void OnSceneWarmup( DoSceneWarmup e )
    {
        
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

    }

    private void InitializeMap()
    {
        RunData data = RunManager.Instance.RunData;

        if( data.WorldMapData == null )
        {
            data.WorldMapData = new MapData( gameObject.scene.name );
            data.WorldMapData.Initialize();
        }

        if( data.Caravan != null )
            _playerHarvester = data.Caravan;
        else
        {
            _playerHarvester = Harvester.GetDataCopySync();
            RunManager.Instance.RunData.Caravan = _playerHarvester;
            RunManager.Instance.RunData.Caravan.Position = StartPos.position;
        }

        CreateCaravanGraphics();
    }



    private void Start()
    {
        EncounterManager.Instance.AddEncounter( TestEncounter );
        CoroutineUtils.DoWaitForEndOfFrame( InitializeMap );
    }


    // Update is called once per frame
    void Update()
    {
        if( TimeManager.Instance != null )
            TimeManager.Instance.Update();

        if( _playerHarvester != null )
            _playerHarvester.Tick( TimeManager.Instance.DayData.HoursDelta );

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


    private async void CreateCaravanGraphics()
    {
        var runData = RunManager.Instance.RunData;
        var caravanAsset = await runData.Caravan.Graphics.GetAssetAsync();

        var instGO = Instantiate<GameObject>( caravanAsset );
        _caravanGfx = instGO.GetComponent<GfxMoveableMapObject>();
        _caravanGfx.Initialize( runData.Caravan );
    }

}




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
                catch( System.Exception ex )
                {
                    //Following and caller protected
                    Debug.LogException( ex );
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
            if( GfxMap.RaycastForSelectables( out hit ) )
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

