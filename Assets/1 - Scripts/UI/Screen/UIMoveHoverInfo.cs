using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;

public class UIMoveHoverInfo : UICellHoverInfo
{

    [HideInInspector]
    public string CurrentAP;
    [HideInInspector]
    public string Change;

    public void Show( Vector2Int cell, int cost, int currentAP )
    {
        this.PositionOver( cell );
        this.CurrentAP = currentAP.ToString();
        this.Change = Mathf.Abs(cost).ToString();
        this.Show();
        RefreshLocalizedStrings();
    }
}
