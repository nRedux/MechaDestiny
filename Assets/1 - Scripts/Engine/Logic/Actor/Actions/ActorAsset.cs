using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Internal;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Engine/Entities/Actor")]
public class ActorAsset : DataProviderAsset<ActorAsset, Actor>
{

    [Title("UI")]
    public Sprite PortraitImage;
    public LocalizedString DisplayName;

    public MechAssetReference MechReference;
    public ActorActionAsset[] Actions;

    public override void SetupNewData( Actor newData )
    {
        newData.ID = this.name;
        newData.Actions = new ActorAction[Actions.Length];
        newData.AddSubEntity( GetMechData() );
        
        for( int i = 0; i < Actions.Length; i++ )
        {
            if( Actions[i] == null )
                continue;
            newData.Actions[i] = Actions[i].GetData();
        }
    }


    public Actor GetData()
    {
        Actor newActor = Json.Clone<Actor>( Data );
        newActor.ID = this.name;
        newActor.Actions = new ActorAction[Actions.Length];
        newActor.AddSubEntity( GetMechData() );

        for( int i = 0; i < Actions.Length; i++ )
        {
            if( Actions[i] == null )
                continue;
            newActor.Actions[i] = Actions[i].GetData();
        }
        return newActor;
    }

    private MechData GetMechData()
    {
        if( !MechReference.RuntimeKeyIsValid() )
            return null;
        return MechReference.GetDataSync();
    }

}
