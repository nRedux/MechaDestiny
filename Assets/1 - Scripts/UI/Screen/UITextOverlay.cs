using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;

public class UITextOverlay : UICellHoverInfo
{

    public TMP_Text Text;
    Vector2Int _hoverTarget;

    public void Show( Vector2Int cell, string text )
    {
        _hoverTarget = cell;
        Text.Opt()?.SetText( text );
        this.PositionOver( cell );
        this.Show();
    }

    private void Update()
    {
        PositionOver( _hoverTarget );
    }
}
