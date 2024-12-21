using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;

public class UIAttackHoverInfo : UICellHoverInfo
{
    [HideInInspector]
    public string Accuracy;

    public void Show( Actor attacker, Actor target )
    {
        this.PositionOver( target.Position );
        var mechData = attacker.GetMechData();
        if( mechData.ActiveWeapon is MechComponentData weaponComp )
        {
            var distance = GameEngine.Instance.Board.GetDistance( attacker.Position, target.Position );
            if( distance == null )
                return;
            this.Accuracy = weaponComp.CalculateAccuracy( distance.Value ).ToString();
            this.Show();
            RefreshLocalizedStrings();
        }
    }
}
