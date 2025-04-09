using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
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
    [SerializeField]
    private ActorActionAsset[] Actions;

    public AIPersonality AI_Personality;

    public ActorActionAsset[] GetActions()
    {
        return Actions.NotNull().ToArray();
    }

    public override void SetupNewData( Actor newData )
    {
        newData.ID = this.name;

        var mechData = GetMechData();
        newData.StartPilotingMech( mechData );
        newData.AddSubEntity( newData.PilotedMech );
        newData.InitializeAIPersonality( AI_Personality );
        newData.Actions = new ActorAction[Actions.Length];
        for( int i = 0; i < Actions.Length; i++ )
        {
            if( Actions[i] == null )
                continue;
            newData.Actions[i] = Actions[i].GetData();
        }
    }

    private MechData GetMechData()
    {
        if( !MechReference.RuntimeKeyIsValid() )
            return null;
        return MechReference.GetDataCopySync();
    }

}
