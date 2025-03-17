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


public class UIMechList : UIItemList<UIMech, MechData>
{
}