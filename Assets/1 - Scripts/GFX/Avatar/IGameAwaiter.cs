using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameAwaitable
{
    bool Chain( IGameAwaitable awaiter );

    void Begin();

    bool IsComplete { get; }
}


public abstract class ActionAwaiter : IGameAwaitable
{

    public IGameAwaitable Next { get; private set; }

    public bool IsComplete { get; private set; }

    public void Begin() { }

    public bool Chain( IGameAwaitable awaiter )
    {
        if( Next == null )
        {
            if( !Next.Chain( awaiter ) )
                return false;
            return true;
        }

        Next = awaiter;
        return true;
    }

    public abstract IEnumerator DoAction();
}

