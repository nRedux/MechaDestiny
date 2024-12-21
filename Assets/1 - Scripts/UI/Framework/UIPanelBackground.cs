using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPanelBackground : MonoBehaviour, IPointerClickHandler
{
    public System.Action OnClick;

    public void Attach( UIPanel panel )
    {
        if( panel == null )
            return;

        var canvas = panel.GetComponentInParent<Canvas>();
        RectTransform rt = this.gameObject.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, 10000 );
        rt.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, 10000 );
        rt.transform.position = canvas.transform.position;
        int sibIndex = panel.transform.GetSiblingIndex();
        rt.transform.SetParent( panel.transform.parent, true );
        rt.transform.SetSiblingIndex( Mathf.Max( 0, sibIndex - 1 ) );

    }

    public void OnPointerClick( PointerEventData eventData )
    {
        OnClick?.Invoke();
    }
}
