using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UIActorCollectionException: System.Exception
{
    public UIActorCollectionException() : base() { }
    public UIActorCollectionException(string message) : base(message) { }
    public UIActorCollectionException( string message, System.Exception innerException ) : base( message, innerException ) { }
}


public class UIActorCollection : UIItemList<UIActor, Actor>
{
    private void OnEnable()
    {

    }

    public void RefreshElementsContent()
    {
        GetUIs().Do( x => x.RefreshContent() );
    }
}
