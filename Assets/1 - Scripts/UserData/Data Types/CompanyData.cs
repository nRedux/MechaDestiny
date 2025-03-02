using System.Collections.Generic;
using UnityEngine;



public class CompanyData
{

    //Every character in your employ
    public ActorCollection Employees
    {
        get; 
        set;
    } = new ActorCollection();

    public List<MechData> Mechs
    {
        get;
        set;
    } = new List<MechData>();
}
