using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu( menuName = "Engine/Entities/Moveable Map Object" )]
public class MapObjectAsset : DataProviderAsset<MapObjectAsset, MapObjectData>
{

    public LocalizedString DisplayName;

    public override void SetupNewData( MapObjectData newData )
    {
        
    }

}
