using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class UISelector<TDataSource, TUIOptionType> : UIPanel where TUIOptionType : UISelectorItemOption<TDataSource> where TDataSource : class
{

    public TUIOptionType OptionPrefab;
    public Transform OptionsRoot;
    public System.Action<TDataSource> OptionSelected;

    private PickActiveWeaponEvent _event;


    protected override void Awake()
    {
        base.Awake();
    }


    private void OnDestroy()
    {

    }


    public void StartPick( List<TDataSource> optionData, TDataSource selected, System.Action<TDataSource> onPick )
    {
        this.OptionSelected = onPick;
        Refresh( optionData, selected );
        Show();
    }


    public override void Hide()
    {
        base.Hide();
        _event = null;
        OptionSelected = null;
    }


    public TUIOptionType CreateOption( TDataSource entity, bool selected )
    {
        if( entity == null )
            throw new System.ArgumentNullException( "Entity cannot be null" );
        if( OptionsRoot == null )
        {
            Debug.LogError( "OptionsRoot is null." );
            return null;
        }

        TUIOptionType instance = Instantiate<TUIOptionType>( OptionPrefab );
        if( selected )
            EventSystem.current.SetSelectedGameObject( instance.gameObject );
        instance.transform.SetParent( OptionsRoot, false );
        instance.Initialize( entity, OnPick );
        return instance;
    }

    //public delegate void ItemPickedDelegate( UISelectorItemOption<TDataSource> result );

    private void OnPick( UISelectorItemOption<TDataSource> option )
    {
        var optionSelected = OptionSelected;
        Hide();
        optionSelected?.Invoke( option.DataSource );
    }


    protected void Refresh( List<TDataSource> optionData, TDataSource selected )
    {
        OptionsRoot.DestroyChildren();
        optionData.Do( x => CreateOption( x, x == selected ) );
    }
}