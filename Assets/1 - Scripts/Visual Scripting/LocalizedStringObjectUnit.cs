using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

[UnitTitle("Localized String Mx")]
[UnitCategory("Edgeflow/Dialog/String")]
public class LocalizedStringObjectUnit : Unit
{
    
    [Inspectable]
    [UnitHeaderInspectable]
    [SerializeAs( "LocalizedString" )]
    public LocalizedStringObject String;


    [PortLabelHidden]
    public ValueOutput Out;

    protected override void Definition()
    {
        Out = ValueOutput<string>("Out", f => String.LocalizedValue);
    }
}