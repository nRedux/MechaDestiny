using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu( menuName = "Engine/Entities/Mech" )]
public class MechAsset : DataProviderAsset<MechAsset, MechData>
{

    public LocalizedString DisplayName;

    public override void SetupNewData( MechData newData )
    {
        newData.InitializeComponentData();
    }
}
 