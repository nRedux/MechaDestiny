using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( menuName = "Engine/Global Settings" )]
public class GlobalSettings:SingletonScriptableObject<GlobalSettings>
{
    public List<WeaponTypeInfo> WeaponTypes;

    [Space]
    public List<ActorReference> PlayerActors;

    [Space]
    public GameObject DefaultCompDestroyExplosion;

    public GameObject AttackCameraPrefab;

    [Space]
    public MechComponentReference[] TestMechComponentInventory;

    [Space]
    public ActorReference TestInventory;

    public ActorCollection GetStarterActorsCollection()
    {
        if( PlayerActors == null )
            return new ActorCollection();

        var result = new ActorCollection();
        PlayerActors.Do( x => result.Add( x.GetDataCopySync() ) );
        return result;
    }
}
