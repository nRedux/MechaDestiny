using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISimpleHealthbar : UIPanel
{

    public Color AllyHealthColor;
    public Color EnemyHealthColor;

    public Image HealthFill;

    private StatisticWatcher TorsoWatcher;
    private StatisticWatcher LegsWatcher;
    private StatisticWatcher LArmWatcher;
    private StatisticWatcher RArmWatcher;

    private float _maxHP;
    private int _torsoHP, _legsHP, _LArmHP, _RArmHP;
    private SmartPoint _hoverTarget;

    public void AssignEntity( IEntity entity, Transform HoverTarget )
    {
        if( HealthFill == null )
            return;

        if( entity == null )
            return;

        _hoverTarget = new SmartPoint( HoverTarget );

        if( entity is Actor actor )
        {

            var mechData = actor.GetMechData();

            var torsoStat = mechData.Torso.GetStatistic( StatisticType.Health );
            var legsStat = mechData.Legs.GetStatistic( StatisticType.Health );
            var lArmStat = mechData.LeftArm.GetStatistic( StatisticType.Health );
            var rArmStat = mechData.RightArm.GetStatistic( StatisticType.Health );

            //Thought there was a max health stat when I wrote this. Will keep until there is.
            var maxTorsoStat = mechData.Torso.GetStatistic( StatisticType.Health );
            var maxLegsStat = mechData.Legs.GetStatistic( StatisticType.Health );
            var maxLArmStat = mechData.LeftArm.GetStatistic( StatisticType.Health );
            var maxRArmStat = mechData.RightArm.GetStatistic( StatisticType.Health );

            _torsoHP = torsoStat.Value;
            _legsHP = legsStat.Value;
            _LArmHP = lArmStat.Value;
            _RArmHP = rArmStat.Value;
            _maxHP = maxTorsoStat.Value + maxLegsStat.Value + maxLArmStat.Value + maxRArmStat.Value;

            TorsoWatcher = new StatisticWatcher( torsoStat, OnTorsoHealth );
            LegsWatcher = new StatisticWatcher( legsStat, OnLegsHealth );
            LArmWatcher = new StatisticWatcher( lArmStat, OnLArmHealth );
            RArmWatcher = new StatisticWatcher( rArmStat, OnRArmHealth );

            UpdateUI();
            ChooseBarColor( entity );
        }

    }

    private void LateUpdate()
    {
        if( _hoverTarget == null )
            return;
        PositionOver( _hoverTarget.Position );
    }

    private void ChooseBarColor( IEntity entity )
    {
        if( HealthFill == null ) return;
        if( entity is Actor actor ){
            HealthFill.color = actor.GetTeamID() == 0 ? AllyHealthColor : EnemyHealthColor;
        }
    }

    private void OnRArmHealth( StatisticChange change )
    {
        _RArmHP = change.Value;
        UpdateUI();
    }

    private void OnLArmHealth( StatisticChange change )
    {
        _LArmHP = change.Value;
        UpdateUI();
    }

    private void OnLegsHealth( StatisticChange change )
    {
        _legsHP = change.Value;
        UpdateUI();
    }

    private void OnTorsoHealth( StatisticChange change )
    {
         _torsoHP = change.Value;
        UpdateUI();
    }

    private float GetNormalizedHealth()
    {
        return ( _torsoHP + _legsHP + _LArmHP + _RArmHP ) / _maxHP;
    }

    private void UpdateUI()
    {
        if( HealthFill == null ) return;

        if( _torsoHP <= 0 ) {
            HealthFill.fillAmount = 0f;
            Destroy( gameObject, 1f );
            enabled = false;
        }

        HealthFill.fillAmount = GetNormalizedHealth();
    }
}
