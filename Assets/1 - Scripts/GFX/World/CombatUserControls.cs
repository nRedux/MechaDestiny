using System;
using System.Linq;
using UnityEngine;

public class CombatUserControls
{
    public InputActions InputActions = null;

    /// <summary>
    /// Raycast performed once per frame for testing cell interactions: Hovered actor and hovered cell tests.
    /// </summary>
    private RaycastHit? _frameRaycast;
    /// <summary>
    /// The world coordinate hovered cell user is interacting with.
    /// </summary>
    private Vector2Int? _hoveredCell = new Vector2Int( int.MinValue, int.MinValue );

    public InputAction Interact => InputActions.Interact;
    public InputAction Cancel => InputActions.Cancel;
    public InputAction SelectMech { get; private set; }


    public Vector2Int? HoveredCell
    {
        get;
        private set;
    } = new Vector2Int( int.MinValue, int.MinValue );


    public Vector2Int? LastHoveredCell
    {
        get;
        private set;
    }


    public Actor HoveredActor
    {
        get;
        private set;
    }


    public Actor LastHoveredActor
    {
        get;
        private set;
    }


    public bool HoveredActorChanged { get; private set; }


    public CombatUserControls()
    {
        InputActions = new InputActions();
        SelectMech = InputActions.NewAction( x => { } );
        Interact.AddActivateListener( OnInteract );
    }


    private void OnInteract( InputActionEvent evt )
    {
        if( evt.Used )
            return;

        if( HoveredActor != null && HoveredActor.IsPlayer )
        {
            SelectMech.Activate( HoveredActor );
            evt.Use();
        }
    }


    public void Update()
    {
        var gameEngine = GameEngine.Instance;
        DoFrameRaycast( gameEngine );
        UpdateHoveredActor( gameEngine );
        UpdateHoveredCell( gameEngine );
        InputActions.Update();
    }


    private void DoFrameRaycast( GameEngine engine )
    {
        _frameRaycast = engine.Camera.Raycast();
    }


    private void UpdateHoveredActor( GameEngine engine )
    {
        if( _frameRaycast == null || !_frameRaycast.HasValue )
        {
            return;
        }

        RaycastHit h = _frameRaycast.Value;
        Vector3Int intVec = h.point.ToInt();
        if( !engine.Game.Board.ContainsCell( intVec.x, intVec.z ) )
        {
            return;
        }

        var hitCell = new Vector2Int( intVec.x, intVec.z );
        //Get all actors at the cell
        //Discard any which should be ignored.
        var hoveredActor = GameEngine.Instance.Board.GetActorsAtCell( hitCell ).FirstOrDefault();
        HoveredActorChanged = HoveredActor != hoveredActor;
        LastHoveredActor = HoveredActor;
        HoveredActor = hoveredActor;
    }


    private void UpdateHoveredCell( GameEngine engine )
    {
        LastHoveredCell = HoveredCell;
        if( _frameRaycast == null || !_frameRaycast.HasValue )
        {
            HoveredCell = null;
            return;
        }

        RaycastHit h = _frameRaycast.Value;
        Vector3Int castPoint = h.point.ToInt();
        if( !engine.Game.Board.ContainsCell( castPoint.x, castPoint.z ) )
        {
            HoveredCell = null;
        }

        HoveredCell = new Vector2Int( castPoint.x, castPoint.z );
    }

}
