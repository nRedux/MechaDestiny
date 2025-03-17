using System;
using UnityEngine;
using UnityEngine.Events;

public class GfxWorldIndicator: MonoBehaviour
{

    public float TransitionDuration = .1f;
    public UnityEvent OnStartHighlight;
    public UnityEvent OnStopHighlight;

    private Animator _animator;

    private bool _destroyAfterHide = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Show( )
    {
        gameObject.SetActive( true );
        _animator.Opt()?.Play( "Show", 0, TransitionDuration );
    }

    public void Show( object data )
    {
        _animator.Opt()?.Play( "Show", 0, 0f );
        HandleShowData( data );
    }

    public void StartHighlight()
    {
        OnStartHighlight.Invoke();
    }

    public void StopHighlight()
    {
        OnStopHighlight.Invoke();
    }

    public void Hide( bool destroyAfterHide = false )
    {
        _destroyAfterHide = destroyAfterHide;
        _animator.Opt()?.CrossFade( "Hide", TransitionDuration, 0, 0f );
    }

    public virtual void HandleShowData( object data )
    {

    }

    public void HideFinished()
    {
        if( _destroyAfterHide )
            Destroy( gameObject );
    }
}
