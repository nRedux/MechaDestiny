using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverEvent: GameEvent
{
    public int Winner;
}


public class CurrentActorEvent : GameEvent
{
    public Actor Actor;
}


public class PickActiveWeaponEvent: GameEvent
{
    public MechData MechData;
}

public class ActiveWeaponChanged : GameEvent
{
    public MechData MechData;
}

public class TimeModeChanged: GameEvent
{
    public bool IsPlaying;
}

/// <summary>
/// Emitted by Game class to notify any client code which wishes to intercede in in the turn switch logic
/// </summary>
public class GameTurnChangeEvent: GameEvent
{
    public Game Game;
    public int NextTurn;
    public Team NextTeam;
    public bool UIFinished = false;
    private GameTurnChangeEvent() { }
    
    public GameTurnChangeEvent( Game game )
    {
        Game = game;
        NextTurn = game.TurnManager.TurnNumber + 1;
        NextTeam = game.TurnManager.NextTeam;
    }
}