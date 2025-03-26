using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class IStorageException: Exception
{
    public IStorageException() : base() { }
    public IStorageException(string message) : base(message) { }
    public IStorageException(string message, Exception inner) : base(message, inner) { }
}


[JsonObject]
public interface IStorage
{
    int GetCapacity();

    int GetCount();

    void AddItem( IItem item );

    void RemoveItem( IItem item );

    AddStoreItemReason CanAddItem( IItem item );

    RemoveStoreItemReason CanRemoveItem( IItem item );

    IEnumerable<IItem> GetItems<ItemType>() where ItemType : IItem;

    IEnumerable<IItem> GetItems( object itemID );

    IEnumerable<IItem> GetItems( bool includeCurrency );

    IItem GetItem<ItemType>(  ) where ItemType : IItem;
}


public enum AddStoreItemReason
{
    CanStore,
    ItemNull,
    ItemNonStorable,
    AlreadyStored
}

public enum RemoveStoreItemReason
{
    CanRemove,
    ItemNull,
    ItemNotInContainer
}


[JsonObject]
public class StorageContainer: IStorage
{

    [JsonProperty]
    private List<IItem> _items = new List<IItem>();

    [JsonConstructor]
    public StorageContainer() { }

    public void AddItem( IItem item )
    {
        if( item == null )
            throw new IStorageException( "Cannot store null items" );
        if( item.Storage != null )
        {
            item.Storage.RemoveItem( item );
        }

        if( ProcessCurrency( item ) )
            return;

        _items.Add( item );
        item.SetStorage( this );
    }


    /// <summary>
    /// If the item is a currency and we already have an entry for currency, merge the new currency item into the existing one.
    /// </summary>
    /// <param name="item">An item interface</param>
    /// <returns>If the item WAS a currency AND was merged into an existing currency, returns true - otherwise - returns false.</returns>
    public bool ProcessCurrency( IItem item )
    {
        if( item is ICurrency addedCurrency )
        {
            var existing = _items.Where(x => x.GetType() == addedCurrency.GetType() ).FirstOrDefault() as ICurrency;
            if( existing != null )
            {
                existing.Add( addedCurrency.GetAmount() );
                return true;
            }
        }
        return false;
    }

    public void RemoveItem( IItem item )
    {
        if( _items.Remove( item ) )
            item.SetStorage( null );
    }

    public IEnumerable<IItem> GetItems<ItemType>() where ItemType: IItem
    {
        return _items.Where( x => typeof( ItemType ).IsAssignableFrom(x.GetType()) );
    }

    public IEnumerable<IItem> GetItemsStrict<ItemType>() where ItemType : IItem
    {
        return _items.Where( x => x.GetType() == typeof( ItemType ) );
    }

    public AddStoreItemReason CanAddItem( IItem item )
    {
        if( item == null )
            return AddStoreItemReason.ItemNull;
        if( !item.Storable )
            return AddStoreItemReason.ItemNonStorable;
        if( item.Storage != null )
            return AddStoreItemReason.AlreadyStored;
        return AddStoreItemReason.CanStore;
    }

    public RemoveStoreItemReason CanRemoveItem( IItem item )
    {
        if( item == null )
            return RemoveStoreItemReason.ItemNull;
        if( item.Storage != this || !_items.Contains( item ) )
            return RemoveStoreItemReason.ItemNotInContainer;
        return RemoveStoreItemReason.CanRemove;
    }

    public int GetCapacity()
    {
        return int.MaxValue;
    }

    public int GetCount()
    {
        return _items.Count;
    }

    public IItem GetItem<ItemType>() where ItemType : IItem
    {
        return GetItemsStrict<ItemType>().FirstOrDefault();
    }

    public IEnumerable<IItem> GetItems( object itemID )
    {
        return _items.Where( x => x.GetObjectID().Equals( itemID ) );
    }

    public IEnumerable<IItem> GetItems( bool includeCurrency )
    {
        return includeCurrency ? _items : _items.Where( x => !(x is ICurrency) );
    }
}
