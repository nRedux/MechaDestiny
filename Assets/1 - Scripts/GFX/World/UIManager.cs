using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;


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

    public UISimpleHealthbar SimpleHealthbar;

    public GameObject PlayerAttackUI;

    public CombatCamera CombatCamera;

    public GameObject SpawnedObjectsRoot;

    public string SwitchWeaponInput = KeyCode.Q.ToString();

    //public UIMechInfo 

    private GameEngine _gameEngine;

    private List<IUIRequest> _pendingRequests = new List<IUIRequest>();

    private List<IUIRequest> _activeRequests = new List<IUIRequest>();

    private Queue<ActionResult> _actionResults = new Queue<ActionResult>();
    private ActionResult _currentActionResult = null;

    private Actor _activeActor;

    private GameTurnChangeEvent _changeEvent;

    //Generalized hovered detecting
    private Vector2Int? _hoveredCell = new Vector2Int( int.MinValue, int.MinValue );
    private Actor _hoveredActor = null;


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
    }

    public void CreateSimpleHealthbar( Actor actor )
    {
        if( actor == null )
            return;
        if( SimpleHealthbar == null )
            return;

        var avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        var shb = Instantiate( SimpleHealthbar );
        shb.transform.SetParent( SpawnedObjectsRoot.transform ?? transform, false );
        shb.Initialize();
        shb.AssignEntity( actor, avatar.transform );
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

    public void HideMechInfos()
    {
        HideSideAMechInfo();
        HideSideBMechInfo();
    }

    public void ShowActionSequence( Actor actor, System.Action onClick = null )
    {
        if( ActionSequence == null ) return;

        ActionSequence.OnUIFire += onClick;
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
        TurnChange.Refresh( e.Game.TurnManager.TurnNumber, e.Game.TurnManager.ActiveTeam.IsPlayerTeam );
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
        DoFrameRaycast();
        RunRequests();
        RunResultQueue();
#if !DISABLE_DEBUG
        DebugRequests();
#endif
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
    public void RequestUI( IUIRequest request, bool queue = true )
    {
        if( request == null )
            return;

        if( queue )
        {
            _pendingRequests.Add( request );
        }
        else
        {
            ActivateRequest( request );
        }
    }


    private void RunRequests()
    {
        UpdateRequestInput();
        TryActivateQueuedRequests();
        RunActiveRequests();
    }


    private void TryActivateQueuedRequests()
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
        DebugUIQueue();
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
        if(  _activeRequests.Count == 0 )
            return;
        
        var req = _activeRequests[0];
        req.Run();

        //Request state changed to something interesting?
        if( IsRequestEnding( req ) )
        {
            req.Cleanup();
            _activeRequests.Remove( req );
            DebugUIQueue();
        }
    }

    private void DebugUIQueue()
    {
        Debug.Log( $"Current UI Queue: Active_Requests: {_activeRequests.Count()}" );
        _activeRequests.Do( x =>
        {
            Debug.Log( $"Type: {x.GetType().Name}" );
        } );
    }


    public void ExecuteResult(ActionResult result )
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
        if( !_gameEngine.Game.Board.IsCoordInBoard( intVec.x, intVec.z ) ||
            !_gameEngine.Game.Board.CanActorOccupyCell( new Vector2Int(intVec.x, intVec.z) ) ||
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
        if( !_gameEngine.Game.Board.IsCoordInBoard( intVec.x, intVec.z ) ||
            !IsCoordinateActive( h.point, cells ) )
            return false;

        result = new Vector2Int( intVec.x, intVec.z );
        return true;
    }

    private RaycastHit? _frameRaycast;

    private void DoFrameRaycast()
    {
        _frameRaycast = _gameEngine.Camera.Raycast();
    }

    public Actor GetHoveredActor( )
    {
        if( _frameRaycast == null || !_frameRaycast.HasValue )
        {
            return null;
        }

        RaycastHit h = _frameRaycast.Value;
        Vector3Int intVec = h.point.ToInt();
        if( !_gameEngine.Game.Board.IsCoordInBoard( intVec.x, intVec.z ) )
        {
            return null;
        }

        var hitCell = new Vector2Int( intVec.x, intVec.z );
        //Get all actors at the cell
        //Discard any which should be ignored.
        var hoveredActor = UIManager.Instance.GetActorsAtCell( hitCell ).FirstOrDefault();
        bool result = _hoveredActor != hoveredActor;
        _hoveredActor = hoveredActor;
        return hoveredActor;
    }

    public Vector2Int? GetHoveredCell( )
    {
        if( _frameRaycast == null || !_frameRaycast.HasValue )
        {
            return null;
        }

        RaycastHit h = _frameRaycast.Value;
        Vector3Int castPoint = h.point.ToInt();
        if( !_gameEngine.Game.Board.IsCoordInBoard( castPoint.x, castPoint.z ) )
        {
            _hoveredCell = new Vector2Int( -1, -1 );
            return null;
        }

        
        var hitCell = new Vector2Int( castPoint.x, castPoint.z );
        _hoveredCell = hitCell;
        return hitCell;
    }


    private void UpdateRequestInput()
    {
        var lastActor = _hoveredActor;
        var actorNow = GetHoveredActor();
        if( actorNow != lastActor )
        {
            //Old hover ended.
            if( lastActor != null )
            {
                UpdateRequestActorHover( lastActor, false );
            }
            if( actorNow != null )
            {
                UpdateRequestActorHover( actorNow, true );
            }
        }
        if( actorNow != null && Input.GetMouseButtonDown( 0 ) )
        {
            UpdateRequestActorClick( actorNow );
        }


        Vector2Int? lastCell = _hoveredCell;
        Vector2Int? cellNow = GetHoveredCell();
        if( cellNow != lastCell  )
        {
            if( lastCell != null )
                UpdateRequestCellHover( lastCell.Value, false );
            if( cellNow != null )
                UpdateRequestCellHover( cellNow.Value, true );
        }

    }

    /*
        Actor lastActor = GetHoveredActor();
        Vector2Int lastCell = GetHoveredCell();
        UpdateRequestActorHover( lastActor, false );
        UpdateRequestCellHover( lastCell, false );
    */

    private void UpdateRequestActorHover( Actor actor, bool hovered )
    {
        if( actor == null )
            return;

        if( _activeRequests.Count() == 0 )
            return;

        var req = _activeRequests[0];

        if( hovered )
            req.ActorHoverStart( actor );
        else
            req.ActorHoverEnd( actor );
    }


    private void UpdateRequestCellHover( Vector2Int cell, bool hovered )
    {
        if( _activeRequests.Count() == 0 )
            return;

        var req = _activeRequests[0];

        if( hovered )
            req.CellHoverStart( cell );
        else
            req.CellHoverEnd( cell );
    }


    private void UpdateRequestActorClick( Actor actor )
    {
        if( actor == null )
            return;

        if( _activeRequests.Count() == 0 )
            return;

        var req = _activeRequests[0];
        req.ActorClicked( actor );
    }

    private void UpdateRequestCellClick( Vector2Int cell )
    {
        if( cell == null )
            return;

        if( _activeRequests.Count() == 0 )
            return;

        var req = _activeRequests[0];
        req.CellClicked( cell );
    }


    public List<Actor> GetActorsAtCell( Vector2Int cell )
    {
        List<Actor> actors = new List<Actor>();
        _gameEngine.Game.Teams.Do( x => x.GetMembers().Where( a => a.Position == cell ).Do( z => actors.Add( z ) ) ); ;
        return actors;
    }


    public Actor GetActorAtCell( Vector2Int cell )
    {
        List<Actor> actors = new List<Actor>();
        _gameEngine.Game.Teams.Do( x => x.GetMembers().Where( a => a.Position == cell ).Do( z => actors.Add( z ) ) ); ;
        return actors.FirstOrDefault();
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
    internal bool TerminatePending( IUIRequest request )
    {
        return _pendingRequests.Remove( request );
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
