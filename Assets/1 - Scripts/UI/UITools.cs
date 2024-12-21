using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UITools
{

    /// <summary>
    /// This sets your screen position within canvas space (within any other rect transfor, but the idea is for canvas/screen space overlay positioning).
    /// </summary>
    /// <param name="worldPos">The screen position</param>
    public static void SetCanvasScreenPosition( this RectTransform rectTrans, RectTransform container, Vector3 worldPos )
    {
        Vector3 screenPos3 = Camera.main.WorldToScreenPoint( worldPos );
        Vector2 screenPos = new Vector2( screenPos3.x, screenPos3.y );
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle( container, screenPos, null, out anchoredPos );
        rectTrans.anchoredPosition = anchoredPos;
    }

}
