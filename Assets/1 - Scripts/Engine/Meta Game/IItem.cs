using System.Linq;
using UnityEngine;

public interface IItem
{

    /// <summary>
    /// The container this item is inside
    /// </summary>
    IStorage Storage { get; }

    /// <summary>
    /// Can this item type be stored in an IStorage?
    /// </summary>
    bool Storable { get; }

    void SetStorage( IStorage storage );

    string GetObjectName();

    Sprite GetPortrait();

    string GetDisplayName();

    string GetDescription();

    object GetObjectID();

}

public static class ItemExtensions
{
    public static int Count( this IItem item )
    {
        if( item.Storage == null )
            return 1;
        return item.Storage.GetItems( item.GetObjectID() ).Count();
    }
}
