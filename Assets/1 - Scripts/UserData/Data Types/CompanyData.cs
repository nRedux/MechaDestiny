using System.Collections.Generic;
using UnityEngine;


public class CompanyData
{
    //Every character in your employ
    public List<Actor> Employees
    {
        get; 
        set;
    } = new List<Actor>();

    public List<MechData> Mechs
    {
        get;
        set;
    } = new List<MechData>();
}
