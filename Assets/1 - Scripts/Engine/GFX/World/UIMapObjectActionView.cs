using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMapObjectActionView : MonoBehaviour
{
    public UIMapObjectActionButton ActionButton;
    public GridLayoutGroup Layout;

    public void Show() => gameObject.SetActive( true );
    public void Hide() => gameObject.SetActive( false );

    private void Awake()
    {
    }

    public void Refresh( List<GfxMapObjectAction> actions, System.Action<GfxMapObjectAction> onClick )
    {
        Layout.Opt()?.gameObject.DestroyChildren();
        SpawnButtons( actions, onClick );
    }

    private void SpawnButtons( List<GfxMapObjectAction> actions, System.Action<GfxMapObjectAction> onClick )
    {
        foreach( var action in actions )
        {
            NewButton( action, onClick );
        }
    }

    private UIMapObjectActionButton NewButton( GfxMapObjectAction action, System.Action<GfxMapObjectAction> onClick )
    {
        if( ActionButton == null || Layout == null ) return null;

        var newButton = Instantiate( ActionButton );
        newButton.Initialize( action, onClick );
        newButton.transform.SetParent( Layout.transform, false );
        return newButton;
    }

}
