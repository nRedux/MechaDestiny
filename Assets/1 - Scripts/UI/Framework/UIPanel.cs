using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;


public class LocalizedStringsRefresher
{
    private LocalizeStringEvent[] _events;

    public LocalizedStringsRefresher( GameObject gameObject )
    {
        _events = gameObject.GetComponentsInChildren<LocalizeStringEvent>();
    }

    public void RefreshLocalizedStrings()
    {
        _events?.Do( x => x.RefreshString() );
    }

}

public class UIPanel : UIShowable
{
    private Canvas _canvas = null;
    private RectTransform _canvasRectTransform = null;
    private RectTransform _rectTransform = null;
    private LocalizedStringsRefresher _locStringRef;

    public UIPanelBackground PanelBackground { get; protected set; }

    
    protected virtual void Awake()
    {
        Initialize();
    }


    /// <summary>
    /// Must be called once it is a child of a canvas.
    /// </summary>
    public void Initialize()
    {
        _canvas = GetComponentInParent<Canvas>();
        if( _canvas == null )
            return;
        _canvasRectTransform = _canvas.GetComponent<RectTransform>();
        _rectTransform = this.GetComponent<RectTransform>();
        _locStringRef = new LocalizedStringsRefresher( gameObject );
    }

    protected void RefreshLocalizedStrings()
    {
        _locStringRef.RefreshLocalizedStrings();
    }


    /// <summary>
    /// This sets your screen position within canvas space.
    /// </summary>
    /// <param name="position">The screen position</param>
    public void SetCanvasScreenPosition( Vector3 position )
    {
        _rectTransform.PositionOverWorld( _canvasRectTransform, position );
    }

    public void CreateBackground()
    {
        if( PanelBackground != null )
            return;

        PanelBackground = SUIManager.Instance.UISystems.GetPanelBackground( );
        PanelBackground.Attach( this );

    }

    public void KillBackground()
    {
        if( PanelBackground == null )
            return;

        Destroy( PanelBackground.gameObject );
        PanelBackground = null;
    }

    public void PositionOver( Transform transform )
    {
        SetCanvasScreenPosition( transform.position );
    }

    public void PositionOver( Vector3 worldPosition )
    {
        SetCanvasScreenPosition( worldPosition );
    }

}
