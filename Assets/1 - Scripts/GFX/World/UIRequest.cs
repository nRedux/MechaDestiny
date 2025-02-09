using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
public class UIRequestCallbackNullException : Exception
{
    public UIRequestCallbackNullException( string msg ) : base( msg )
    {
    }
}

public delegate void UIRequestSuccessCallback<T>( T result );
public delegate void UIRequestFailureCallback<T>( T error );
public delegate void UIRequestCancelResult();

public enum UIRequestState
{
    Idle,
    Running,
    Complete,
    Failed,
    Cancelled,
}

public interface IUIRequest
{
    UIRequestState State { get; }
    object GetRequester();
    void Start ();
    void Run();
    void Cleanup();
    void ActorHoverStart( Actor actor );
    void ActorHoverEnd( Actor actor );
    void CellHoverStart( Vector2Int cell );
    void CellHoverEnd( Vector2Int cell );
    void ActorClicked( Actor actor );
    void CellClicked( Vector2Int actor );
    void PerformCompletion();
}

public abstract class UIRequest<TResult, TError>: IUIRequest
{
    public UIRequestSuccessCallback<TResult> Succeeded;
    public UIRequestFailureCallback<TError> Failed;
    public UIRequestCancelResult Cancelled;

    private object _requester;
    protected UIRequestState _state = UIRequestState.Idle;

    private System.Action _completion;

    public UIRequestState State => _state;


    public UIRequest( UIRequestSuccessCallback<TResult> onSuccess, UIRequestFailureCallback<TError> onFailure, UIRequestCancelResult onCancel, object requester )
    {
        this._requester = requester;
        this.Succeeded = onSuccess;
        this.Failed = onFailure;
        this.Cancelled = onCancel;
    }


    public void SetStateRunning()
    {
        _state = UIRequestState.Running;
    }


    public void Succeed( TResult result )
    {
        if( _state == UIRequestState.Complete )
            return;
        Cleanup();
        _state = UIRequestState.Complete;
        if( Succeeded == null )
            throw new UIRequestCallbackNullException( "OnSuccess callback null" );
        var succeededCallback = Succeeded;
        NukeCallbacks();
        succeededCallback?.Invoke( result );
    }


    protected void Fail( TError error )
    {
        if( _state == UIRequestState.Failed )
            return;
        Cleanup();
        _state = UIRequestState.Failed;
        if( Failed == null )
            throw new UIRequestCallbackNullException( "OnFailure callback null" );
        var failedCallback = Failed;
        NukeCallbacks();
        failedCallback?.Invoke( error );
    }


    public void Cancel()
    {
        if( _state == UIRequestState.Cancelled )
            return;
        Cleanup();
        _state = UIRequestState.Cancelled;
        UIManager.Instance.TerminatePending( this );
        OnCancelled();
        if( Cancelled == null )
            throw new UIRequestCallbackNullException( "OnSuccess callback null" );

        var cancelCallback = Cancelled;
        NukeCallbacks();
        cancelCallback?.Invoke();
    }

    public void NukeCallbacks()
    {
        Cancelled = null;
        Failed = null;
        Succeeded = null;
    }


    /// <summary>
    /// Allow UI to handle 
    /// </summary>
    protected abstract void OnCancelled();

    public virtual void Start() { _state = UIRequestState.Running; }
    public abstract void Run();
    public abstract void Cleanup();

    public object GetRequester()
    {
        return _requester;
    }

    public virtual void ActorHoverStart( Actor actor )
    {
    }

    public virtual void ActorHoverEnd( Actor actor )
    {
    }

    public virtual void ActorClicked( Actor actor )
    {
    }

    public virtual void CellHoverStart( Vector2Int cell )
    {
        
    }

    public virtual void CellHoverEnd( Vector2Int cell )
    {
        
    }

    public virtual void CellClicked( Vector2Int cell )
    {
    }

    public void PerformCompletion()
    {
        _completion?.Invoke();
    }
}