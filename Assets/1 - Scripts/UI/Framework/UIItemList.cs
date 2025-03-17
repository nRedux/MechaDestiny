using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public interface IUIItem<TDataType>
{
    System.Action<IUIItem<TDataType>> Clicked { get; set; }

    void Refresh( TDataType data );
}

//public IUIListFilter


public class UIItemList<TUIItemType, TDataType> : MonoBehaviour where TUIItemType : MonoBehaviour, IUIItem<TDataType>
{
    public Object DisplayTarget;
    public TUIItemType ItemPrefab;

    public System.Action<IUIItem<TDataType>> Clicked;

    public IEnumerable<TDataType> Collection { get => _collection; }

    private IEnumerable<TDataType> _collection;

    public List<TUIItemType> GetUIs()
    {
        return transform.GetComponentsInChildren<TUIItemType>().ToList();
    }

    public virtual IEnumerable<TDataType> FilterItems( IEnumerable<TDataType> items )
    {
        return items;
    }

    private void Update()
    {
    }

    public void Refresh( IEnumerable<TDataType> collection )
    {
        if( collection == null )
            throw new System.ArgumentNullException( $"{nameof( collection )} cannot be null." );
        DeleteEntries();
        _collection = FilterItems( collection );
        _collection.Do( x =>
        {
            AddEntry( x );
        } );
    }

    public void DeleteEntries()
    {
        if( DisplayTarget is UIGrid grid )
        {
            grid.DestroyRows();
        }
        else if( DisplayTarget is GameObject gameObj )
        {
            gameObj.DestroyChildren();
        }
        else
            throw new UIActorCollectionException( $"{nameof( DisplayTarget )} invalid type" );
    }


    public void AddEntry( TDataType data )
    {
        if( DisplayTarget == null )
            throw new UIActorCollectionException( $"{nameof( DisplayTarget )} not set." );

        TUIItemType instance = NewUI( data );

        if( DisplayTarget is UIGrid grid )
        {
            grid.Add( instance.transform );
        }
        else if( DisplayTarget is GameObject gameObj )
        {
            instance.transform.SetParent( gameObj.transform, false );
        }
        else
            throw new UIActorCollectionException( $"{nameof( DisplayTarget )} invalid type" );
    }


    private TUIItemType NewUI( TDataType data )
    {
        if( ItemPrefab == null )
            throw new UIMechListException( $"{this.GetType().Name}: {nameof( ItemPrefab )} is null. Cannot create mech UI instances." );
        var newUI = Instantiate<TUIItemType>( ItemPrefab );
        var uiAsItem = newUI as IUIItem<TDataType>;
        uiAsItem.Refresh( data );
        uiAsItem.Clicked += ItemUIClicked;

        return newUI;
    }


    private void ItemUIClicked( IUIItem<TDataType> actor )
    {
        Clicked?.Invoke( actor );
    }

}
