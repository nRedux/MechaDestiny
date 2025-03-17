using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UIInventoryListException: System.Exception
{
    public UIInventoryListException() : base() { }
    public UIInventoryListException( string message) : base(message) { }
    public UIInventoryListException( string message, System.Exception innerException ) : base( message, innerException ) { }
}


public class UIInventoryList : UIItemList<UIItem, IItem>
{

    public override IEnumerable<IItem> FilterItems( IEnumerable<IItem> items )
    {
        return items.GroupBy( x => x.GetObjectID() ).Select( x => x.FirstOrDefault() );
    }

}
