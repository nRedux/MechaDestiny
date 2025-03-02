using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( menuName = "Engine/Global Settings" )]
public class GlobalSettings:SingletonScriptableObject<GlobalSettings>
{

    public List<WeaponTypeInfo> WeaponTypes;

    public List<ActorReference> PlayerActors;

    public ActorCollection GetStarterActorsCollection()
    {
        if( PlayerActors == null )
            return new ActorCollection();

        var result = new ActorCollection();
        PlayerActors.Do( x => result.Add( x.GetDataCopySync() ) );
        return result;
    }
}
