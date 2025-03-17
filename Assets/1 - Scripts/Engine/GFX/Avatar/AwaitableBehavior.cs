using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AwaitableBehavior : MonoBehaviour
{

    public AwaitableBehavior Next { get; private set; }

    public bool IsComplete { get; private set; }

    public bool Begun { get; private set; }


    public bool Chain( AwaitableBehavior awaiter )
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


    public bool ChainComplete()
    {
        if( Next != null )
        {
            return Next.ChainComplete();
        }
        return this.IsComplete;
    }


    public void Begin() 
    {
        Debug.LogError("Begin");
        Begun = true;
        Await();
    }


    public virtual IEnumerator Action()
    {
        yield return null;
    }


    private void Await()
    {
        StartCoroutine( AwaitRoutine() );
    }


    private IEnumerator AwaitRoutine( )
    {
        Begun = true;
        yield return StartCoroutine( Action() );

        IsComplete = true;

        if( Next != null )
        {
            Next.Begin();
        }
    }


    public IEnumerator AwaitComplete()
    {
        if( !Begun )
        {
            Debug.LogError( "AwaitComplete called on an awaitable behavior which has not been begun." );
            yield break;
        }

        bool done = false;
        while( !done )
        {
            if( ChainComplete() )
                yield break;
            yield return null;
        }

    }
}