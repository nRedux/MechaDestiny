using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldOverlay : MonoBehaviour
{
    private RectTransform _rectTransform = null;
    public RectTransform RectTransform
    {
        get
        {
            _rectTransform ??= GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    private RectTransform _canvasRectTransform = null;
    public RectTransform CanvasRectTransform
    {
        get
        {
            _canvasRectTransform ??= GetComponent<RectTransform>();
            return _canvasRectTransform;
        }
    }


    /// <summary>
    /// This sets your screen position within canvas space.
    /// </summary>
    /// <param name="position">The screen position</param>
    public void SetCanvasScreenPosition( Vector3 position )
    {
        var canvasRT = CanvasRectTransform;
        var thisRT = RectTransform;
        if( !canvasRT || !thisRT )
            return;

        RectTransform.SetCanvasScreenPosition( CanvasRectTransform, position );
    }
}
