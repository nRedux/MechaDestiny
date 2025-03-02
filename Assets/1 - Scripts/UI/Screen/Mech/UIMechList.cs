using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UIMechListException: System.Exception
{
    public UIMechListException() : base() { }
    public UIMechListException( string message) : base(message) { }
    public UIMechListException( string message, System.Exception innerException ) : base( message, innerException ) { }
}

public class UIMechList: MonoBehaviour
{
    public Object DisplayTarget;
    public UIMech MechPrefab;

    public System.Action<UIMech> MechClicked;

    public List<MechData> MechCollection { get => _collection; }

    private List<MechData> _collection;

    public List<UIMech> GetActorUIs()
    {
        return transform.GetComponentsInChildren<UIMech>().ToList();
    }


    private void Update()
    {
        if( Input.GetKeyDown( KeyCode.U ) )
            Refresh( RunManager.Instance.RunData.CompanyData.Mechs );
    }

    private void OnEnable()
    {
        Refresh( RunManager.Instance.RunData.CompanyData.Mechs );
    }

    public void Refresh(List<MechData> collection )
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


    public void AddEntry( MechData mechData )
    {
        if( DisplayTarget == null )
            throw new UIActorCollectionException( $"{nameof( DisplayTarget )} not set." );

        UIMech instance = NewUIMech( mechData );

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


    private UIMech NewUIMech( MechData mechData )
    {
        if( MechPrefab == null )
            throw new UIMechListException( $"{nameof( MechPrefab )} is null. Cannot create actor instances." );
        var newMechUI = Instantiate<UIMech>( MechPrefab );
        newMechUI.Refresh( mechData );
        newMechUI.Clicked += ActorUIClicked;

        return newMechUI;
    }

    private void ActorUIClicked( UIMech actor )
    {
        MechClicked?.Invoke( actor );
    }

}
