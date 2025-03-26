using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class UICombatRewardList: UIItemList<UIItem, IItem>
{
    public override IEnumerable<IItem> FilterItems( IEnumerable<IItem> items )
    {
        return items.GroupBy( x => x.GetObjectID() ).Select( x => x.FirstOrDefault() );
    }


    /// <summary>
    /// Have the player take all the items in the list
    /// </summary>
    public void PlayerTakeAllItems()
    {
        var items = GetUIs();
        var playerInventory = RunManager.Instance.RunData.Inventory;
        items.Do( x => playerInventory.AddItem( x.Item ) );
        DeleteItems();
    }
}
