using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCST
{
    private Game _game;

    public MCST( Game game, int activeTeam )
    {
        this._game = game;
        //this.currentPlayer = activeTeam;
    }

    private Game GetGameCopy( Game game )
    {
        return new Game( game );
    }

    private bool HasMovesLeft( Game game )
    {
        return false;
    }

    public void GetBestMoves()
    {

    }

}
