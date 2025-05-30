using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using ScriptableObjectArchitecture;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using Pathfinding;
using UnityEditor.Localization.Plugins.XLIFF.V12;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameEngine : Singleton<GameEngine>
{
    private const int DEFAULT_SIZE = 10;

    public Color[] TeamColors = new Color[] { Color.red, Color.blue };

    public ActorReference TestActor1;
    public List<ActorReference> PlayerActors;
    public List<ActorReference> AIActors;

    public GfxCamera Camera;
    public GfxBoard GfxBoard;
    public UIManager UIManager;
    public GameEventBase EndTurnButtonEvent;

    [HideInInspector]
    public GfxAvatarManager AvatarManager;

    private Game _game;
    private Vector3Int _lastRaycast;

    public Game Game => _game;
    public Board Board => _game.Board;

    private bool _initializationDone = false;
    private bool _doBlockerUpdate = false;
    private List<SpawnLocation> _spawnLocations;


    public bool Initialized
    {
        get => _initializationDone;
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
        //Make sure it's alive
        var co = CoroutineUtils.Instance;

        AwakeUIManager();
        AwakeAvatarManager();
        AwakeCamera();
        AwakeGfxBoard();
        CollectSpawnLocations();
    }


    private void CollectSpawnLocations()
    {
        _spawnLocations = FindObjectsByType<SpawnLocation>( FindObjectsSortMode.None ).ToList();
    }

    private List<SpawnLocation> GetSpawnLocations( PlayerType playerType )
    {
        return FindObjectsByType<SpawnLocation>( FindObjectsSortMode.None ).Where( x => x.PlayerType == playerType ).ToList();
    }

    private void AwakeGfxBoard()
    {
        if( GfxBoard != null )
            GfxBoard = FindFirstObjectByType<GfxBoard>();
    }

    private void AwakeUIManager()
    {
        UIManager.Initialize( this );
    }

    private void AwakeAvatarManager()
    {
        if( AvatarManager == null )
            AvatarManager = new GfxAvatarManager();
        AvatarManager.ActorCreated += AvatarCreated;
    }

    private void AwakeCamera()
    {
        if( Camera == null )
            Camera = FindFirstObjectByType<GfxCamera>();
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
        //Make sure it's loaded everything.
        var runManager = RunManager.Instance;
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

    public async Task CreateAvatars( )
    {
        var teams = Game.Teams;

        foreach( var team in teams )
        {
            var actors = team.GetMembers();

            var loaderTasks = actors.Select( x => AvatarManager.CreateGfxActor( x, avatar =>
            {
                avatar.SyncPositionToActor( x );
                avatar.SetTeamColor( avatar.Actor.GetMechData().MechColor );
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


    private void InitializeGameState()
    {
        _game = new Game();
        GfxBoard.SetBoard( _game.Board );
        CreateTeams();
    }


    private void CreateTeams()
    {
        var playerTeam = CreatePlayerTeam();
        var enemyTeam = CreateAITeam();

        //Lets send the info about these starting actor groups to the lua environment so we can reference it later
        LuaBehaviorManager.Instance.SetSuperGlobal( "playerTeam", playerTeam );
        LuaBehaviorManager.Instance.SetSuperGlobal( "enemyTeam", enemyTeam );
        LuaBehaviorManager.Instance.OnTeamsCreated();
    }


    private Team CreatePlayerTeam()
    {
        TurnPhase playerPhase = new PlayerPhase( EndTurnButtonEvent );

        Team playerTeam = new Team( true );
        playerTeam.SetTurnPhases( new TurnPhase[] { playerPhase } );
        _game.AddTeam( playerTeam );

        var playerActors = RunManager.Instance.RunData.CompanyData.Employees;
        InitializeTeamActors( playerTeam, PlayerType.Ally, playerActors );
        return playerTeam;
    }


    private Team CreateAITeam()
    {
        TurnPhase aiPhase = new AITurnPhase();
        Team aiTeam = new Team( false );
        aiTeam.SetTurnPhases( new TurnPhase[] { aiPhase } );
        _game.AddTeam( aiTeam );

        List<Actor> usedActors = new List<Actor>();
        AIActors.Do( x => usedActors.Add( x.GetDataCopySync() ) );
        if( RunManager.Instance.RunData.CombatEnemies != null )
            usedActors = RunManager.Instance.RunData.CombatEnemies;

        InitializeTeamActors( aiTeam, PlayerType.Enemy, usedActors );
        return aiTeam;
    }


    public void CreateTeamActors( Team team, PlayerType type, List<ActorReference> actors )
    {
        if( actors.Count() == 0 )
            return;

        List<SpawnLocation> spawns = GetSpawnLocations( type ).ToList();
        if( spawns.Count == 0 )
        {
            Debug.LogError( "No spawn points exist." );
            return;
        }

        IEnumerator actorEnum = actors.GetEnumerator();

        while( spawns.Count > 0 )
        {
            var spawn = spawns[0];
            spawns.RemoveAt( 0 );
            var spawnPosition = spawn.transform.position;

            if( !actorEnum.MoveNext() )
            {
                actorEnum = actors.GetEnumerator();
                actorEnum.MoveNext();
            }

            var currentActor = actorEnum.Current as ActorReference;

            var actor = currentActor.GetDataCopySync();
            team.AddMember( actor );
            spawn.UseMe( actor, Game );
        }
    }

    public void InitializeTeamActors( Team team, PlayerType type, List<Actor> actors )
    {
        if( actors.Count() == 0 )
            return;

        List<SpawnLocation> spawns = GetSpawnLocations( type ).ToList();
        if( spawns.Count == 0 )
        {
            Debug.LogError( "No spawn points exist." );
            return;
        }

        IEnumerator actorEnum = actors.GetEnumerator();

        while( spawns.Count > 0 )
        {
            var spawn = spawns[0];
            spawns.RemoveAt( 0 );
            var spawnPosition = spawn.transform.position;

            if( !actorEnum.MoveNext() )
            {
                break;
            }
            var actor = actorEnum.Current as Actor;
            actor.InitializeForCombat();
            team.AddMember( actor );
            spawn.UseMe( actor, Game );
        }
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
