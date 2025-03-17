using Pathfinding;
using UnityEngine;

public class UIItemFull : MonoBehaviour
{
    public UIItemMechComponent ItemMechComponentUI;

    public void Refresh( IItem item )
    {
        ItemMechComponentUI.Opt()?.gameObject.SetActive( false );
        
        if( item is MechComponentData mechCompData )
        {
            ItemMechComponentUI.Opt()?.gameObject.SetActive( true );
            ItemMechComponentUI.Opt()?.Refresh( mechCompData );
        }
    }
}
