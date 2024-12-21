using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( menuName = "Engine/Entities/Mech" )]
public class MechAsset : DataProviderAsset<MechAsset, MechData>
{
    public override void SetupNewData( MechData newData )
    {
        newData.InitializeComponentData();
    }
}
 