using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using System.Xml.Serialization;
using System.Threading.Tasks;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameEngine : Singleton<GameEngine>
{
    private const int DEFAULT_SIZE = 10;

    public Color[] TeamColors = new Color[] { Color.red, Color.blue };

    public ActorReference TestActor1;
    public ActorReference AITestActor;

    public GfxCamera Camera;
    public GfxBoard GfxBoard;
    public UIManager UIManager;
    public BoolWindow WalkableCells = new BoolWindow( DEFAULT_SIZE );
    public GameEventBase EndTurnButtonEvent;

    [HideInInspector]
    public GfxAvatarManager AvatarManager;

    private Game _game;
    private Vector3Int _lastRaycast;

    public Game Game => _game;
    public Board Board => _game.Board;


    [OnValueChanged( "SizeChanged" )]
    public int BoardSize = DEFAULT_SIZE;


    private bool _initializationDone = false;
    private bool _doBlockerUpdate = false;


    public bool Initialized
    {
        get => _initializationDone;
    }


    public void UpdateBoardData()
    {
        if( Application.isPlaying )
            return;

        Debug.Log("Update blockers");

#if UNITY_EDITOR
        if( EditorApplication.update != EditorUpdate )
            EditorApplication.update = EditorUpdate;
#endif

        _doBlockerUpdate = true;
    }


#if UNITY_EDITOR
    private void EditorUpdate()
    {
        if( Application.isPlaying )
            return;
        if( _doBlockerUpdate )
        {
            UpdateBlockerData();
            _doBlockerUpdate = false;
        }
    }
#endif

    private void UpdateBlockerData()
    {
        WalkableCells.Do( iter =>
        {
            iter.window[iter.local] = true;
        } );

        BoardBlocker[] blockers = FindObjectsOfType<BoardBlocker>();
        foreach( var blocker in blockers )
        {
            blocker.ApplyToBoard( WalkableCells );
        }
    }


    public bool IsPlayerTurn
    {
        get
        {
            return _game.TurnManager.ActiveTeam.IsPlayerTeam;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        UIManager.Initialize( this );

        AvatarManager = new GfxAvatarManager();
        Camera = FindObjectOfType<GfxCamera>();
        GfxBoard = FindObjectOfType<GfxBoard>();

        AvatarManager.ActorCreated += AvatarCreated;
    }

    private void AvatarCreated( GfxActor createdActor )
    {
        UIManager.OnActorCreated( createdActor );
    }


    private void Start()
    {
        Initialize();
    }

    private async void Initialize()
    {
        InitializeGameState();
        await CreateAvatars();
        _game.Start();
        _initializationDone = true;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        _game.Cleanup();
    }

    public void OnDrawGizmos()
    {
        Color oldGizmosColor = Gizmos.color;

        Color walkableColor = Color.green;
        walkableColor.a = .1f;
        
        Color unwalkableColor = Color.red;
        unwalkableColor.a = .1f;

        WalkableCells.Do( iter =>
        {
            Gizmos.color = iter.value ? walkableColor : unwalkableColor;
            Gizmos.DrawCube( new Vector3( iter.world.x + .5f, 0, iter.world.y + .5f ), new Vector3( 1f, .02f, 1f ) * .97f );
        } );

        Gizmos.color = oldGizmosColor;
    }

    public async Task CreateAvatars( )
    {
        var teams = Game.Teams;

        foreach( var team in teams )
        {
            var actors = team.GetMembers();

            var loaderTasks = actors.Select( x => AvatarManager.CreateGfxActor( x, avatar =>
            {
                avatar.SyncPositionToActor( x );
                avatar.SetTeamColor( GetTeamColor( x.GetTeamID() ) );
            } ) );
            await Task.WhenAll( loaderTasks );
        }
    }


    public Color GetTeamColor( int index )
    {
        if( index < 0 || index >= TeamColors.Length )
            return Color.magenta;
        return TeamColors[index];
    }


    public void SizeChanged()
    {
        WalkableCells = new BoolWindow( BoardSize, BoardSize );
        WalkableCells.MoveCenter( new Vector2Int( WalkableCells.Width / 2, WalkableCells.Height / 2 ) );
    }


    private void InitializeGameState()
    {
        _game = new Game( WalkableCells.Width, WalkableCells.Height );
        _game.SetWalkability( WalkableCells );
        GfxBoard.SetBoard( _game.Board );
        CreateTeams();
    }


    private void CreateTeams()
    {
        TurnPhase playerPhase = new PlayerPhase( EndTurnButtonEvent );
        TurnPhase aiPhase = new AITurnPhase( );

        Team team1 = new Team( true );
        team1.SetTurnPhases( new TurnPhase[] { playerPhase } );

        Team team2 = new Team( false );
        team2.SetTurnPhases( new TurnPhase[] { aiPhase } );

        _game.AddTeam( team1 );
        _game.AddTeam( team2 );

        var actor = TestActor1.GetDataSync();
        actor.SetIsPlayer( true );
        actor.SetPosition( new Vector2Int( 1, 1 ), Game );
        team1.AddMember( actor );


        var actor2 = TestActor1.GetDataSync();
        actor2.SetIsPlayer( true );
        actor2.SetPosition( new Vector2Int( 2, 1 ), Game );
        team1.AddMember( actor2 );

        //var actor2 = TestActor1.GetData();
        //actor2.SetPosition( new Vector2Int( 2, 2 ), Game );
        //team1.AddMember( actor2 );

        var AI = AITestActor.GetDataSync();
        AI.SetIsPlayer( false );
        AI.SetPosition( new Vector2Int( 3, 4 ), Game );
        team2.AddMember( AI );

        var AI2 = AITestActor.GetDataSync();
        AI2.SetIsPlayer( false );
        AI2.SetPosition( new Vector2Int( 3, 5 ), Game );
        team2.AddMember( AI2 );
    }


    public void Update()
    {
        if( !_initializationDone )
            return;
        TickGame();

        //RenderGridAtMouse();
    }


    public void TickGame()
    {
        _game.Tick();
    }


    public int RoundToInt( float val )
    {
        return (int) val;
    }


}
