using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor.Search;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class GfxAvatarNullException: Exception
{
    public GfxAvatarNullException( ) : base( ) { }
    public GfxAvatarNullException( string msg ) : base( msg ) { }
}


public class GfxAvatarInvalidPrefab : Exception
{
    public GfxAvatarInvalidPrefab() : base() { }
    public GfxAvatarInvalidPrefab( string msg ) : base( msg ) { }
}


public interface IRequestGroupHandle
{
    void Do();

    void Undo();
}

public class PauseGroupHandle: List<IUIRequest>, IRequestGroupHandle
{
    public PauseGroupHandle( List<IUIRequest> requests )
    {
        this.AddRange( requests );
    }

    public void Do()
    {
        this.Do( x => x.Pause() );
    }

    public void Undo()
    {
        this.Do( x => x.Resume() );
    }
}

public struct NewRequest
{
    public IUIRequest Request;
    public bool Queued;
}


public class UIManager : Singleton<UIManager>
{

    public UIPanel MainCombatUI;
    public UICombatEnd CombatEndUI;
    public UIMechPopup MechInfoPopup;
    
    public UIMechPopup SideA_FocusMechInfo;
    public UIMechPopup SideB_FocusMechInfo;

    public UIMechPopup SideA_FocusMechInfoFull;
    public UIMechPopup SideB_FocusMechInfoFull;

    public UIWeaponPicker WeaponPicker;
    public UIActionPicker ActionPicker;

    public UITurnChange TurnChange;
    public UIMoveHoverInfo MoveHoverInfo;
    public UIAttackHoverInfo AttackHoverInfo;

    public UIActionSequence ActionSequence;
    public UIActionSequenceItemHover ActionSequenceHover;

    public InputActions BattleInput = new InputActions();

    public UITextOverlay DebugTextOverlay;

    public UIAITools AITools;

    public UISimpleHealthbar SimpleHealthbar;

    public GameObject PlayerAttackUI;

    public CombatCamera CombatCamera;

    public GameObject SpawnedObjectsRoot;

    public CombatUserControls UserControls = new CombatUserControls();

    public GfxWorldIndicators WorldIndicators = new GfxWorldIndicators();

    public string SwitchWeaponInput = KeyCode.Q.ToString();

    //public UIMechInfo 

    private GameEngine _gameEngine;

    private List<NewRequest> _newRequests = new List<NewRequest>();

    private List<IUIRequest> _pendingRequests = new List<IUIRequest>();

    private List<IUIRequest> _activeRequests = new List<IUIRequest>();

    private Queue<ActionResult> _actionResults = new Queue<ActionResult>();
    private ActionResult _currentActionResult = null;

    private Actor _activeActor;

    private GameTurnChangeEvent _changeEvent;
    private List<UITextOverlay> _debugOverlays = new List<UITextOverlay>();

    private UIPickActionRequest _actionPickRequest = null;
    private UIPickWeaponRequest _weaponPickRequest = null;

    /// <summary>
    /// The world coordinate hovered cell user is interacting with.
    /// </summary>
    private Vector2Int? _hoveredCell = new Vector2Int( int.MinValue, int.MinValue );
    private Actor _hoveredActor = null;

    public Actor ActiveActor { get => _activeActor; }

    /// <summary>
    /// Raycast performed once per frame for testing cell interactions: Hovered actor and hovered cell tests.
    /// </summary>
    private RaycastHit? _frameRaycast;

    public void Initialize( GameEngine gameEngine )
    {
        _gameEngine = gameEngine;
    }

    protected override void Awake()
    {
        base.Awake();
        SetupListeners();
        HideMechInfos();

        if( CombatCamera == null )
            CombatCamera = FindFirstObjectByType<CombatCamera>( FindObjectsInactive.Include );
    }


    private void Start()
    {
        MechInfoPopup.Opt()?.Hide();
        CombatEndUI.Opt()?.Hide();
        WeaponPicker.Opt()?.Hide();
        ActionPicker.Opt()?.Hide();
        ActionSequence.Opt()?.Hide();
        ActionSequenceHover.Opt()?.Hide();
        PlayerAttackUI.Opt()?.SetActive( false );
        MoveHoverInfo.Opt()?.gameObject.SetActive( false );
        AttackHoverInfo.Opt()?.gameObject.SetActive( false );
        AITools.Opt()?.Hide();
    }

    public void TryPickWeapon( Actor actor )
    {
        /*
        if( _actionPickRequest != null )
            return;
        */
        if( _weaponPickRequest != null )
            return;
        if( !UIPickWeaponRequest.CanExecute( actor ) )
            return;
        if( !Input.GetKeyDown( KeyCode.Q ) )
            return;

        _weaponPickRequest = new UIPickWeaponRequest( actor,
        x =>
        {
            //Resume in case it was paused
            _actionPickRequest?.Resume();
            _weaponPickRequest = null;
        },
        y =>
        {
            //Resume in case it was paused
            _actionPickRequest?.Resume();
            _weaponPickRequest = null;
        },
        () =>
        {
            //Resume in case it was paused
            _actionPickRequest?.Resume();
            _weaponPickRequest = null;
        } );

        //If action pick request already going, pause it.
        _actionPickRequest?.Pause();

        UIManager.Instance.RequestUI( _weaponPickRequest, false );
    }


    public void TryPickAction( Actor actor, UIRequestSuccessCallback<object> succeeded, UIRequestCancelResult cancelled, ActionCategory category )
    {

        if( _weaponPickRequest != null )
            return;

        if( _actionPickRequest != null )
            return;

        //Callbacks for action pick request
        UIRequestSuccessCallback<object> success = x =>
        {
            _actionPickRequest = null;
            succeeded?.Invoke( x );
        };

        UIRequestFailureCallback<bool> fail = y =>
        {
            _actionPickRequest = null;
        };

        UIRequestCancelResult cancel = () =>
        {
            _actionPickRequest = null;
            cancelled?.Invoke();
        };

        _actionPickRequest = new UIPickActionRequest( actor, success, fail, cancel, category );

        UIManager.Instance.RequestUI( _actionPickRequest, false );
    }

    public bool TryEndPickAction()
    {
        if( _actionPickRequest != null )
        {
            _actionPickRequest.Cancel();
            _actionPickRequest = null;
            return true;
        }

        return false;
    }

    public void CreateSimpleHealthbar( Actor actor )
    {
        if( actor == null )
            return;
        if( SimpleHealthbar == null )
            return;

        var avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        var shb = Instantiate( SimpleHealthbar );
        shb.transform.SetParent( SpawnedObjectsRoot.Opt()?.transform ?? transform, false );
        shb.Initialize();
        shb.AssignEntity( actor, avatar.transform );
    }


    public void ClearDebugTextOverlays()
    {
        _debugOverlays.Do( x => Destroy( x.gameObject ) );
        _debugOverlays.Clear();
    }


    public void CreateDebugOverlay( Vector2Int cell, string text )
    {
        if( DebugTextOverlay == null ) return;
        var instance = DebugTextOverlay.Duplicate();
        instance.transform.SetParent( SpawnedObjectsRoot.transform, false );
        instance.Initialize();
        instance.Show( cell, text );
        _debugOverlays.Add( instance );
    }


    public enum MechInfoDisplayMode
    {
        Mini,
        Full
    }

    public void ShowSideAMechInfo( Actor actor, MechInfoDisplayMode mode)
    {
        switch( mode )
        {
            case MechInfoDisplayMode.Mini:
                SideA_FocusMechInfoFull.Opt()?.Hide();
                SideA_FocusMechInfo.Opt()?.AssignEntity( actor );
                SideA_FocusMechInfo.Opt()?.Show();
                break;

            case MechInfoDisplayMode.Full:
                SideA_FocusMechInfo.Opt()?.Hide();
                SideA_FocusMechInfoFull.Opt()?.AssignEntity( actor );
                SideA_FocusMechInfoFull.Opt()?.Show();
                break;
        }
    }


    public void ShowSideBMechInfo( Actor actor, MechInfoDisplayMode mode )
    {
        switch( mode )
        {
            case MechInfoDisplayMode.Mini:
                SideB_FocusMechInfoFull.Opt()?.Hide();
                SideB_FocusMechInfo.Opt()?.AssignEntity( actor );
                SideB_FocusMechInfo.Opt()?.Show();
                break;

            case MechInfoDisplayMode.Full:
                SideB_FocusMechInfo.Opt()?.Hide();
                SideB_FocusMechInfoFull.Opt()?.AssignEntity( actor );
                SideB_FocusMechInfoFull.Opt()?.Show();
                break;
        }
    }


    public void HideSideAMechInfo()
    {
        SideA_FocusMechInfo.Opt()?.Hide();
        SideA_FocusMechInfoFull.Opt()?.Hide();
    }


    public void HideSideBMechInfo()
    {
        SideB_FocusMechInfo.Opt()?.Hide();
        SideB_FocusMechInfoFull.Opt()?.Hide();
    }


    public bool IsPointerOverUI()
    {
        List<RaycastResult> results = new List<RaycastResult>();
        var pointerData = new PointerEventData( EventSystem.current );
        pointerData.position = Mouse.current.position.ReadValue();

        EventSystem.current.RaycastAll( pointerData, results );

        foreach( var res in results )
        {
            if( res.gameObject != null && res.gameObject.layer == LayerMask.NameToLayer( "UI" ) )
                return true;
        }

        return false;
    }

    public void HideMechInfos()
    {
        HideSideAMechInfo();
        HideSideBMechInfo();
    }


    public void ShowSequenceSelector( Actor actor, System.Action onClick = null )
    {
        if( ActionSequence == null ) return;
        ActionSequence.Show( actor );
    }


    public void HideActionSequence() 
    {
        ActionSequence.Opt()?.Hide( );
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        TeardownListeners();
    }


    private void SetupListeners()
    {
        Events.Instance.AddListener<GameOverEvent>( OnGameOverEvent );
        Events.Instance.AddListener<CurrentActorEvent>( OnCurrentActorEvent );
        Events.Instance.AddListener<GameTurnChangeEvent>( OnGameTurnChange );
    }


    private void TeardownListeners()
    {
        Events.Instance.RemoveListener<GameOverEvent>( OnGameOverEvent );
        Events.Instance.RemoveListener<CurrentActorEvent>( OnCurrentActorEvent );
        Events.Instance.RemoveListener<GameTurnChangeEvent>( OnGameTurnChange );
    }

    private void OnGameTurnChange( GameTurnChangeEvent e )
    {
        if( TurnChange == null )
            return;
        TerminateActiveRequests();
        _pendingRequests.Clear();
        HideMechInfos();
        ExecuteTurnChangeUI( e );
    }

    private async void ExecuteTurnChangeUI( GameTurnChangeEvent e )
    {
        e.Game.RunGameLogic = false;
        //We use next team as the UI updates before the turn has changed.
        TurnChange.Refresh( e.NextTurn, e.NextTeam.IsPlayerTeam );
        await TurnChange.Run();
        e.Game.RunGameLogic = true;
    }

    private void OnCurrentActorEvent( CurrentActorEvent e )
    {
        _activeActor = e.Actor;
        if( e.Actor == null )
            return;
        var avatar = GameEngine.Instance.AvatarManager.GetAvatar( e.Actor );
        CombatCamera.MoveToTransform( avatar.transform, 1f );
        CombatCamera.LockedTargetActor = e.Actor.IsPlayer ? null : e.Actor;
    }


    private void OnGameOverEvent( GameOverEvent e )
    {
        HideCombatUI();

        CoroutineUtils.BeginCoroutine( DoCombatEndShow(e) );
    }


    public IEnumerator DoCombatEndShow( GameOverEvent e )
    {
        yield return new WaitUntil( ResultQueueEmpty );
        CombatEndUI.Opt()?.PrepareForShow( e.Winner );
        CombatEndUI.Opt()?.Show();
    }


    private void Update()
    {
        if( Input.GetKeyDown( KeyCode.J ) )
            FullDebugQueue();
        UserControls.Update();
        DoFrameRaycast();
        RunRequests();
        RunResultQueue();
        AIToolsInput();
#if !DISABLE_DEBUG
        DebugRequests();
#endif
    }

    private void AIToolsInput()
    {
        if( Input.GetKeyDown( KeyCode.F1 ) )
        {
            if( AITools != null )
            {
                if( this.AITools.gameObject.activeSelf )
                    this.AITools.Hide();
                else
                    this.AITools.Show();
            }
        }
    }


    private void DebugRequests()
    {
#if !DISABLE_DEBUG
        if( !Input.GetKeyDown( KeyCode.R ) )
            return;
        _pendingRequests.Do( x => Debug.Log( x.GetType().Name ) );
        _activeRequests.Do( x => Debug.Log( x.GetType().Name ) );
#endif
    }


    public void OnActorCreated( GfxActor gfxActor )
    {
        //gfxActor.HoverStarted += OnActorHoverStart;
        //gfxActor.HoverEnded += OnActorHoverEnd;
        CreateSimpleHealthbar( gfxActor.Actor );
    }


    public bool ShowWeaponPicker( System.Action onPicked )
    {
        if( _activeActor == null )
            return false;
        if( WeaponPicker == null )
            return false;
        var mechData = _activeActor.GetSubEntities()[0] as MechData;
        if( mechData is MechData data && !data.HasUsableWeapons() )
            return false;
        WeaponPicker.StartPick( mechData, picked => {
            mechData.ActiveWeapon = picked as MechComponentData;
            Events.Instance.Raise( new ActiveWeaponChanged() );
            onPicked?.Invoke();
        } );

        return true;
    }

    public bool ShowActionPicker( System.Action<ActorAction> pickedAction, ActionCategory category )
    {
        if( _activeActor == null )
            return false;
        if( ActionPicker == null )
            return false;
        var actionOptions = _activeActor.GetActionOptions( category );
        ActionPicker.StartPick( actionOptions, null, pickedAction );
        return true;
    }

    internal void HideWeaponPicker()
    {
        WeaponPicker.Opt()?.Hide();
    }

    internal void HideActionPicker()
    {
        ActionPicker.Opt()?.Hide();
    }


    private void OnActorHoverStart( GfxActor obj )
    {
        MechInfoPopup.AssignEntity( obj.Actor );
        MechInfoPopup.PositionOver( obj.transform );
        MechInfoPopup.Show();
    }


    private void OnActorHoverEnd( GfxActor obj )
    {   
        MechInfoPopup.Hide();
    }



    /// <summary>
    /// Request UI start activity.
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="queue">If activating this from within another request, you MUST set queue = true</param>
    public void  RequestUI( IUIRequest request, bool queue = true )
    {
        if( request == null )
            return;

        _newRequests.Add( new NewRequest() { Queued = queue, Request = request } );
    }


    public IRequestGroupHandle PauseRequests( params Type[] types )
    {
        PauseGroupHandle handle = new PauseGroupHandle( _activeRequests.Where( x => types.Contains( x.GetType() ) ).ToList() );
        handle.Do();
        return handle;
    }


    private void ProcessNewRequests()
    {
        _newRequests.Do( x =>
        {
            if( x.Queued )
            {
                _pendingRequests.Add( x.Request );
            }
            else
            {
                ActivateRequest( x.Request );
            }
        } );
        _newRequests.Clear();
    }

    private void RunRequests()
    {
        ProcessNewRequests();
        UpdateRequestInput();
        TryActivatePendingRequests();
        RunActiveRequests();
    }


    private void TryActivatePendingRequests()
    {
        if( _activeRequests.Count != 0 || _pendingRequests.Count == 0 )
            return;

        var newActiveRequest = _pendingRequests.First();
        _pendingRequests.RemoveAt( 0 );

        ActivateRequest( newActiveRequest );
    }


    private void ActivateRequest( IUIRequest newActiveRequest )
    {
        if( newActiveRequest == null )
            return;
        _activeRequests.Insert( 0, newActiveRequest );
        newActiveRequest.Start();
    }


    private bool IsRequestEnding( IUIRequest request )
    {
        return request.State == UIRequestState.Complete ||
            request.State == UIRequestState.Failed ||
            request.State == UIRequestState.Cancelled;
    }

    private void RunActiveRequests()
    {
        _activeRequests = _activeRequests.Where( x => !IsRequestEnding( x ) ).ToList();

        if(  _activeRequests.Count == 0 )
            return;

        DoToActiveRequests( x => x.Run() );

        _activeRequests = _activeRequests.Where( x => !IsRequestEnding( x ) ).ToList();
    }

    private void DoToActiveRequests( System.Action<IUIRequest> action )
    {
        for( int i = _activeRequests.Count - 1; i >= 0; i-- )
        {
            var req = _activeRequests[i];
            if( req.Paused )
                continue;
            if( IsRequestEnding( req ) )
                continue;
            action( req );
        }
    }


    private void DebugUIQueue()
    {
        Debug.Log( $"Pending Request Count: {_pendingRequests.Count}" );
        Debug.Log( $"Current UI Queue: Active_Requests: {_activeRequests.Count()}" );
    }


    public void FullDebugQueue()
    {
        Debug.Log( $"Pending Request Count: {_pendingRequests.Count}" );
        _pendingRequests.Do( x =>
        {
            Debug.Log( x.GetType().Name + $" {x.State} \n" );
        } );

        Debug.Log( $"Current UI Queue: Active_Requests: {_activeRequests.Count()}" );
        _activeRequests.Do( x =>
        {
            Debug.Log( x.GetType().Name + $" {x.State} \n" );
        } );
    }


    public void QueueResult(ActionResult result )
    {
        _actionResults.Enqueue( result );
    }


    public void RunResultQueue()
    {
        if( _currentActionResult != null )
        {
            var status = _currentActionResult.Update();
            if( status == ActionResultStatus.Finished )
            {
                _currentActionResult.OnComplete?.Invoke();
                _currentActionResult.StopListeningStatChanges();
                _currentActionResult = null;
            }
        }
        else if( _actionResults.Count > 0 )
        {
            _currentActionResult = _actionResults.Dequeue();
              _currentActionResult.Start();
        }
    }


    public bool ResultQueueEmpty()
    {
        return _currentActionResult == null &&
            _actionResults.Count == 0;
    }


    private bool IsCoordinateActive( Vector3 position, BoolWindow Cells )
    {

        int cx = Mathf.FloorToInt( position.x ) - Cells.X;
        int cy = Mathf.FloorToInt( position.z ) - Cells.Y;
        if( cx < 0 || cy < 0 )
            return false;
        if( cx >= Cells.Width || cy >= Cells.Height )
            return false;

        if( Cells[cx, cy] )
        {
            return true;
        }

        return false;
    }


    public bool FindWalkableCellUnderMouse( ref Vector2Int result, BoolWindow cells )
    {
        var hit = _gameEngine.Camera.Raycast();
        if( hit == null || !hit.HasValue )
            return false;

        RaycastHit h = hit.Value;
        Vector3Int intVec = h.point.ToInt();
        if( !_gameEngine.Game.Board.ContainsCell( intVec.x, intVec.z ) ||
            _gameEngine.Game.Board.IsBlocked( new Vector2Int(intVec.x, intVec.z) ) ||
            !IsCoordinateActive( h.point, cells ) )
            return false;

        result = new Vector2Int( intVec.x, intVec.z );
        return true;
    }


    public bool FindAttackableCellUnderMouse( ref Vector2Int result, BoolWindow cells )
    {
        var hit = _gameEngine.Camera.Raycast();
        if( hit == null || !hit.HasValue )
            return false;

        RaycastHit h = hit.Value;
        Vector3Int intVec = h.point.ToInt();
        if( !_gameEngine.Game.Board.ContainsCell( intVec.x, intVec.z ) ||
            !IsCoordinateActive( h.point, cells ) )
            return false;

        result = new Vector2Int( intVec.x, intVec.z );
        return true;
    }


    private void DoFrameRaycast()
    {
        _frameRaycast = _gameEngine.Camera.Raycast();
    }


    private void UpdateRequestInput()
    {
        var lastActor = _hoveredActor;
        if( UserControls.HoveredActorChanged )
        {
            //Old hover ended.
            if( UserControls.LastHoveredActor != null )
            {
                UpdateRequestActorHover( UserControls.LastHoveredActor, false );
            }
            if( UserControls.HoveredActor != null )
            {
                UpdateRequestActorHover( UserControls.HoveredActor, true );
            }
        }
        if( UserControls.HoveredActor != null && Input.GetMouseButtonDown( 0 ) )
        {
            UpdateRequestActorClick( UserControls.HoveredActor );
        }

        if( UserControls.HoveredCell != null )
            UpdateRequestCellHover( UserControls.HoveredCell.Value, false );
        /*
        if( UserControls.HoveredCell != UserControls.LastHoveredCell )
        {
            if( UserControls.LastHoveredCell != null )
                UpdateRequestCellHover( UserControls.LastHoveredCell.Value, false );
            if( UserControls.HoveredCell != null )
                UpdateRequestCellHover( UserControls.HoveredCell.Value, true );
        }
        */
    }


    private void UpdateRequestActorHover( Actor actor, bool hovered )
    {
        if( actor == null )
            return;

        if( _activeRequests.Count() == 0 )
            return;

        DoToActiveRequests( req =>
        {
            if( hovered )
                req.ActorHoverStart( actor );
            else
                req.ActorHoverEnd( actor );
        } );
    }


    private void UpdateRequestCellHover( Vector2Int cell, bool hovered )
    {
        if( _activeRequests.Count() == 0 )
            return;

        DoToActiveRequests( req =>
        {
            req.CellHoverUpdate( cell );
            /*
            if( hovered )
                req.CellHoverStart( cell );
            else
                req.CellHoverEnd( cell );
            */
        } );
    }


    private void UpdateRequestActorClick( Actor actor )
    {
        if( actor == null )
            return;

        DoToActiveRequests( req =>
        {
            req.ActorClicked( actor );
        } );
    }


    private void UpdateRequestCellClick( Vector2Int cell )
    {
        if( cell == null )
            return;

        DoToActiveRequests( req =>
        {
            req.CellClicked( cell );
        } );
    }


    public void HideCombatUI()
    {
        if( MainCombatUI == null )
            return;
        MainCombatUI.Hide();
    }


    public void ShowCombatUI()
    {
        if( MainCombatUI == null )
            return;
        MainCombatUI.Show();
    }


    public void TerminateActiveRequests()
    {
        _actionPickRequest = null;
        _weaponPickRequest = null;
        while( _activeRequests.Count > 0 )
        {
            _activeRequests[0].Cleanup();
            _activeRequests.RemoveAt( 0 );
        }
        DebugUIQueue();
    }


    public void TerminateActiveRequests( object requester )
    {
        //Cleanup any requests which aren't going to be kept
        _activeRequests.Do( x =>
        {
            if( x.GetRequester() == requester )
                x.Cleanup();
        } );
        //Keep only requests not made by the requester
        _activeRequests = _activeRequests.Where( x => x.GetRequester() != requester ).ToList();
    }


    /// <summary>
    /// Terminates a request which is pending 
    /// </summary>
    /// <param name="request">The request to terminate</param>
    /// <returns>True if request was pending and was terminated. False if wasn't a pending request.</returns>
    internal void TerminatePending( IUIRequest request )
    {
        //_activeRequests.Remove( request );
        _pendingRequests.Remove( request );
    }


    /// <summary>
    /// Terminate all requests from a given requester.
    /// </summary>
    /// <param name="requester">The object which was specified as the requester when the request was created.</param>
    internal void TerminatePending( object requester )
    {
        var terminationTargets = _pendingRequests.Where( x => x.GetRequester() == requester ).ToList();
        terminationTargets.Do( x => _pendingRequests.Remove( x ) );
    }


}
