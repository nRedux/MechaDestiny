using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerHandler
{

    private Game _game;

    public AIPlayerHandler( Game game )
    {
        this._game = game;
    }

    public void Tick()
    {
        
    }


    public bool ShouldEndTurn()
    {
        return false;
    }

}
