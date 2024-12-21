using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIShowable : MonoBehaviour
{

    [Tooltip("Items who's visibility should match this showable")]
    public List<GameObject> MatchVisibility = new List<GameObject>();

    public virtual void Show()
    {
        if( gameObject.activeSelf ) return;
        gameObject.SetActive( true );
        OnShow();
    }


    public virtual void Hide()
    {
        if( !gameObject.activeSelf ) return;
        gameObject.SetActive( false );
        OnHide();
    }

    private void DoMatchVisibilityObjects( bool showState )
    {
        MatchVisibility.Do( x => x.SetActive( showState ) );
    }

    public virtual void OnShow()
    {

    }

    public virtual void OnHide()
    {

    }

}
