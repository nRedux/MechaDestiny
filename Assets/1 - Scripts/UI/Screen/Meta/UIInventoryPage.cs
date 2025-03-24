using System.Linq;
using UnityEngine;

public class UIInventoryPage : MonoBehaviour
{
    public UIItemFull SelectedItem;
    public UIInventoryList InventoryList;

    private UIItem _selectedItem;

    private void Awake()
    {
        if( InventoryList != null )
            InventoryList.Clicked += item => ActorClicked((UIItem) item);
    }

    private void Start()
    {
        InventoryList.Refresh( RunManager.Instance.RunData.Inventory.GetItems().ToList() );
        if( InventoryList.Collection != null )
            SelectItem( InventoryList.GetUIs().FirstOrDefault() );
        else
            SelectItem( (UIItem) null );
    }

    private void ActorClicked( UIItem mech )
    {
        SelectItem( mech );
    }

    public void SelectItem( IItem mechData )
    {
        var ui = InventoryList.GetUIs().FirstOrDefault( x => x.Item == mechData );
        SelectItem( ui );
    }

    private void SelectItem( UIItem item )
    {
        _selectedItem = item;

        if( _selectedItem != null && _selectedItem != null )
        {
            SelectedItem.gameObject.SetActive( true );
            SelectedItem.Refresh( item.Item );
        }
        else
        {
            SelectedItem.gameObject.SetActive( false );
        }
    }
}
