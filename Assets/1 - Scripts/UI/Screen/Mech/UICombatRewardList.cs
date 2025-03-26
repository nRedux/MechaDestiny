using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UICombatRewardList: UIItemList<UIItem, IItem>
{
    public override IEnumerable<IItem> FilterItems( IEnumerable<IItem> items )
    {
        return items.GroupBy( x => x.GetObjectID() ).Select( x => x.FirstOrDefault() );
    }

}
