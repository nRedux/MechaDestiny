using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;


public class UIItem : MonoBehaviour, IUIItem<IItem>
{
    const string NO_PILOT_NAME = "----";

    public TMP_Text Name;
    public TMP_Text Count;


    private IItem _item;

    private Button _button;

    public IItem Item
    {
        get => _item;
    }

    public Action<IUIItem<IItem>> Clicked { get; set; }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.Opt()?.onClick.AddListener( OnClicked );
    }


    private void OnClicked()
    {
        Clicked?.Invoke( this );
    }


    public void Refresh( IItem item )
    {
        if( item == null )
            throw new System.ArgumentNullException( $"Argument '{nameof( item )}' cannot be null" );
        _item = item;
        RefreshContent();
    }

    public void RefreshContent()
    {
        if( Name != null )
            Name.text = _item.GetDisplayName();

        if( Count != null )
            Count.text = _item.Count().ToString();
    }
}
