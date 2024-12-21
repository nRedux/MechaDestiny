using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICellHoverInfo : UIPanel
{
    public void PositionOver( Vector2Int cell )
    {
        this.SetCanvasScreenPosition( GameUtils.GetWorldPositionForCell( cell ) );
    }
}
