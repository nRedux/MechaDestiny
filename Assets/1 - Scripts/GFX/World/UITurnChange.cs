using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;


public class UITurnChange : MonoBehaviour
{
    private const string CHANGE_ANIM = "TurnChange";
    private bool _animationComplete;

    public GameObject VisibleUIRoot;

    public LocalizedString PlayerTurnEntry;
    public LocalizedString EnemyTurnEntry;

    public LocalizeStringEvent TurnLocEvent;
    public LocalizeStringEvent PlayerLocEvent;

    public int CurrentTurn;

    private void Awake()
    {
        if( VisibleUIRoot == null )
            return;
        VisibleUIRoot.SetActive( false );
    }

    public void Refresh( int turn, bool isPlayer )
    {
        if( PlayerLocEvent != null )
        {
            var entry = isPlayer ? PlayerTurnEntry : EnemyTurnEntry;
            PlayerLocEvent.StringReference = entry;
        }

        CurrentTurn = turn;
        if( TurnLocEvent != null )
        {
            TurnLocEvent.RefreshString();
        }
    }

    internal async Task Run()
    {
        _animationComplete = false;
        Animator animator = GetComponent<Animator>();
        if( animator == null )
            return;

        animator.Play( CHANGE_ANIM, 0, 0 );
        var nextInfo = animator.GetNextAnimatorClipInfo( 0 );
        var currentInfo = animator.GetCurrentAnimatorClipInfo( 0 );

        while( true )
        {
            if( _animationComplete )
                break;
            await Task.Delay( 20 );
        }
    }


    public void Anim_End()
    {
        _animationComplete = true;
    }
}
