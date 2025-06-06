using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Unity.VisualScripting;



[System.Serializable]
public class PlayerMoveAction : MoveAction
{

    private int CalcCost()
    {
        return 0;
    }

    private Game _game;
    private Actor _actor;
    private IEntity _mainEntity;
    private int _range;

    [JsonProperty]
    private bool[,] _moveOptions = new bool[30, 30];
    BoolWindow _moveOptionsWindow = null;

    UIFindMoveTargetRequest _uiRequest;


    [OnDeserialized]
    public void OnDeserialize( StreamingContext context )
    {
        
    }

    public override CanStartActionResult AllowedToExecute( Actor actor )
    {
        var ap = actor.GetStatistic( StatisticType.AbilityPoints );

        if( ap.Value == 0 )
            return CanStartActionResult.InsufficientAP;

        return base.AllowedToExecute( actor );
    }

    public override void Tick()
    {
        return;
    }

    public override void Start( Game game, Actor actor ) 
    {
        base.Start( game, actor );
        //Get valid move locations. Notify the UI we need to display a collection of move locations. Wait for UI to return a result. Execute move.
        _game = game;
        _actor = actor;

        var mechData = actor.GetMechData();

        _range = mechData.GetMoveRange();

        _moveOptionsWindow = new BoolWindow( _range * 2, game.Board );

        this.State = ActorActionState.Started;
        _moveOptionsWindow.MoveCenter( actor.Position );
        //Get all cells which could be moved to.
        _game.Board.GetMovableCellsManhattan( _range, _moveOptionsWindow, actor );

        UIRequestSuccessCallback<Vector2Int> success = moveTarget =>
        {
            var avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
            if( avatar == null )
            {
                Debug.LogError( "Avatar not found for actor." );
                return;
            }

            int? cost = GameEngine.Instance.Board.GetMovePathDistance( actor.Position, moveTarget, actor );

            //Spend the AP required to run this ability's action
            SpendAP( actor, cost ?? 0); 

            ActionResult res = new MoveActionResult( actor, GameEngine.Instance.Board.GetNewPath( actor.Position, moveTarget, _actor ) );
            res.OnComplete = () => {
                End();
             };
            this.State = ActorActionState.Executing;
            UIManager.Instance.QueueResult( res );

            actor.SetPosition( moveTarget, game );
        };
        UIRequestFailureCallback<bool> failure = moveTarget => { End(); };
        UIRequestCancelResult cancel = () => { End(); };


        _uiRequest = new UIFindMoveTargetRequest( actor, _moveOptionsWindow, success, failure, cancel );
        _uiRequest.OnCellHover += ( x ) =>
        {
            if( x.cost > _range || x.cost < 0 )
                return;
            if( x.hover )
                UIManager.Instance.MoveHoverInfo?.Show( x.location, x.cost, actor.GetStatistic( StatisticType.AbilityPoints )?.Value ?? 0 );
            else
                UIManager.Instance.MoveHoverInfo?.Hide();
        };


        UIManager.Instance.RequestUI( _uiRequest, false );
    }


    public override void TurnEnded()
    {
        End();
    }


    public override void End()
    {
        base.End();
        this.State = ActorActionState.Finished;
        UIManager.Instance.MoveHoverInfo?.Hide();
        CancelUIRequest();
    }


    private void CancelUIRequest()
    {
        if( _uiRequest != null )
        {
            _uiRequest?.Cancel();
            _uiRequest = null;
        }
    }
}