using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

public class UIWeaponPicker : UIPanel
{

    public UIWeaponPickerOption OptionPrefab;
    public Transform OptionsRoot;
    public System.Action<IEntity> ActiveWeaponPicked;

    private PickActiveWeaponEvent _event;
    private UIFindAttackTargetRequest _tempAttackOverlay;

    private bool _pauseMoveAndAttackOverlays = true;
    public bool PauseMoveAttackReqsOptionEnter
    {
        get
        {
            return _pauseMoveAndAttackOverlays;
        }
        set
        {
            _pauseMoveAndAttackOverlays = value;
        }
    }

    private IRequestGroupHandle _pausedRequests;

    protected override void Awake()
    {
        base.Awake();

    }


    private void OnDestroy()
    {
        if( _pausedRequests != null )
        {
            _pausedRequests.Undo();
            _pausedRequests = null;
        }
    }


    public void StartPick( MechData mech, System.Action<IEntity> onPick )
    {
        this.ActiveWeaponPicked = onPick;
        Refresh( mech );
        Show();
    }


    public override void Hide(  )
    {
        base.Hide();
        _event = null;
        ActiveWeaponPicked = null;
    }


    public UIWeaponPickerOption CreateOption( IEntity entity, IEntity currentActive )
    {
        if( entity == null )
            throw new System.ArgumentNullException("Entity cannot be null");
        if( OptionsRoot == null )
        {
            Debug.LogError("OptionsRoot is null.");
            return null;
        }

        UIWeaponPickerOption instance = Instantiate<UIWeaponPickerOption>( OptionPrefab );
        if( PauseMoveAttackReqsOptionEnter )
        {
            instance.PointerEntered += OptionPointerEntered;
            instance.PointerExited += OptionPointerExited;
            instance.Clicked += OptionClicked;
        }

        if( entity == currentActive )
            EventSystem.current.SetSelectedGameObject( instance.gameObject );
        instance.transform.SetParent( OptionsRoot, false );
        instance.Picked += OnPick;
        instance.Initialize( entity );
        return instance;
    }

    private void OptionPointerEntered( UIWeaponPickerOption opt )
    {
        _pausedRequests = UIManager.Instance.PauseRequests( new System.Type[] { typeof( UIFindMoveTargetRequest ), typeof( UIFindAttackTargetRequest ) } );
        _tempAttackOverlay = AttackHelper.ShowWeaponOverlay( UIManager.Instance.ActiveActor, opt.Entity as MechComponentData );
    }


    private void OptionPointerExited( UIWeaponPickerOption opt )
    {
        _tempAttackOverlay?.Cancel();
        _tempAttackOverlay = null;
        if( _pausedRequests != null )
        {
            _pausedRequests.Undo();
            _pausedRequests = null;
        }
    }

    private void OptionClicked( UIWeaponPickerOption opt )
    {
        /*Delaying to avoid any race condition where the button component hasn't had it's click callback invoked yet at this time.
        The button component, on this gameobject, changes the active weapon in it's click callback. Resuming an attack overlay will refresh it to latest
        active weapon, but if the active weapon hasn't been updated yet it would show the old state.
        It's possible this code could run, resume the paused requests, and they would display an overlay based on the previously selected weapon.
        Refer to the comment labeled "A." below this.*/
        CoroutineUtils.DoDelayUnityFrameEnd( () =>
        {
            //Delay this as well so we have continuity of overlay - this prevents potential flicker.
            _tempAttackOverlay?.Cancel();
            _tempAttackOverlay = null;

            //A.The below undo resumes anything paused. If there was a find attack target request, it would resume and refresh it's grid overlay. We want to make sure the new active weapon is set first.
            if( _pausedRequests != null )
            {
                _pausedRequests.Undo();
                _pausedRequests = null;
            }
        } );
    }

    private void OnPick( UIWeaponPickerOption option )
    {
        ActiveWeaponPicked?.Invoke( option.Entity );
        Hide();
    }


    private void Refresh( MechData MechData )
    {
        OptionsRoot.DestroyChildren();
        var weapons = MechData.FindFunctionalWeapons();
        weapons.Do( x => CreateOption( x, MechData.ActiveWeapon ) );
    }
}
