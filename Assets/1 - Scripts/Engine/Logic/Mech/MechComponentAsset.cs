using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using System.Runtime.Serialization;



[System.Serializable]
public class AttachmentSlot
{
    public AttachmentSlotTypeReference SlotType;

    //TODO: Bad naming - this would be the installed component even if modified by the player during a run
    public MechComponentReference DefaultComponent;

    [HideInInspector]
    public MechComponentData Data;


    /// <summary>
    /// Initialize using default installed component
    /// </summary>
    public void Initialize( IEntity parent )
    {
        if( Data == null )
            return;
        var asset = GetDefaultComponentAsset();
        if( asset == null )
        {
            Debug.LogError( "Invalid default component reference." );
            return;
        }

        this.Data = asset.GetDataCopy();
        this.Data.SetParent( parent );
        this.Data.Initialize();
    }


    /// <summary>
    /// Get the asset referenced in our DefaultComponent field.
    /// </summary>
    /// <returns>A MechComponentAsset or null if the reference is invalid.</returns>
    private MechComponentAsset GetDefaultComponentAsset()
    {
        if( !DefaultComponent.RuntimeKeyIsValid() )
            return null;
        return DefaultComponent.GetAssetSync();
    }
}


[CreateAssetMenu( menuName = "Engine/Entities/Mech Component" )]
public class MechComponentAsset : DataProviderAsset<MechComponentAsset, MechComponentData>
{

    public LocalizedString DisplayName;

    public override void SetupNewData( MechComponentData newData )
    {
        newData.Initialize();
        newData.ID = DisplayName.TryGetLocalizedString();
    }

}
