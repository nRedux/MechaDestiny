using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.Events;


public class UIMechComponent : UIEntity
{

    private MechComponentData _mechComponent = null;

    private StatisticWatcher _healthWatcher = null;

    private bool _isDestroyed = false;

    private Animator _animator;

    public UnityEvent Destroyed;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }


    public override void AssignEntity( IEntity entity )
    {
        _mechComponent = entity as MechComponentData;
        if( _mechComponent == null )
        {
            Debug.LogError("IEntity is not a MechComponentData");
            return;
        }

        StartListeningHealthChanges( entity );

        base.AssignEntity( entity );
    }


    private void StartListeningHealthChanges( IEntity entity )
    {
        if( _healthWatcher != null )
        {
            _healthWatcher.Stop();
            _healthWatcher = null;
        }

        _healthWatcher = new StatisticWatcher( entity.GetStatistic( StatisticType.Health ), OnHealthChange );
    }


    private void OnHealthChange( StatisticChange change )
    {
        if( !_isDestroyed && change.Value < 0 )
        {
            _isDestroyed = true;
            PlayDestroyedAnimation();
        }
    }


    private void PlayDestroyedAnimation()
    {
        if( !_animator ) 
            return;
        _animator.Play( "Destroyed", 0, 0f );
    }

}
