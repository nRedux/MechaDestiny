using UnityEngine;

public class UIActorCollectionException: System.Exception
{
    public UIActorCollectionException() : base() { }
    public UIActorCollectionException(string message) : base(message) { }
    public UIActorCollectionException( string message, System.Exception innerException ) : base( message, innerException ) { }
}

public class UIActorCollection: MonoBehaviour
{
    public Object DisplayTarget;
    public UIActor ActorPrefab;

    public System.Action<UIActor> ActorClicked;

    private ActorCollection _collection;


    private void Update()
    {
        if( Input.GetKeyDown( KeyCode.U ) )
            Refresh( RunManager.Instance.RunData.CompanyData.Employees );
    }

    private void OnEnable()
    {
        Refresh( RunManager.Instance.RunData.CompanyData.Employees );
    }

    public void Refresh(ActorCollection collection )
    {
        if( collection == null )
            throw new System.ArgumentNullException( $"{nameof(collection)} cannot be null.");
        DeleteEntries();
        _collection = collection;
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


    public void AddEntry( ActorCollectionEntry entry )
    {
        if( DisplayTarget == null )
            throw new UIActorCollectionException( $"{nameof( DisplayTarget )} not set." );

        UIActor instance = NewUIActor( entry.Actor );

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


    private UIActor NewUIActor( Actor actor )
    {
        if( ActorPrefab == null )
            throw new UIActorCollectionException( $"{nameof( ActorPrefab )} is null. Cannot create actor instances." );
        var newActorUI = Instantiate<UIActor>( ActorPrefab );
        newActorUI.Refresh( actor );
        newActorUI.Clicked += ActorUIClicked;

        return newActorUI;
    }

    private void ActorUIClicked( UIActor actor )
    {
        ActorClicked?.Invoke( actor );
    }

}
