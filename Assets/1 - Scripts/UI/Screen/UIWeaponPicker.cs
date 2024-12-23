using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIWeaponPicker : UIPanel
{

    public UIWeaponPickerOption OptionPrefab;
    public Transform OptionsRoot;
    public System.Action<IEntity> ActiveWeaponPicked;

    private PickActiveWeaponEvent _event;


    protected override void Awake()
    {
        base.Awake();

    }


    private void OnDestroy()
    {

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
        if( entity == currentActive )
            EventSystem.current.SetSelectedGameObject( instance.gameObject );
        instance.transform.SetParent( OptionsRoot, false );
        instance.Picked += OnPick;
        instance.Initialize( entity );
        return instance;
    }


    private void OnPick( UIWeaponPickerOption option )
    {
        ActiveWeaponPicked?.Invoke( option.Entity );
        Hide();
    }


    private void Refresh( MechData MechData )
    {
        OptionsRoot.DestroyChildren();
        List<IEntity> weapons = MechData.FindWeaponEntities( MechData, ( c ) => !c.IsBroken() );
        weapons.Do( x => CreateOption( x, MechData.ActiveWeapon ) );
    }
}
