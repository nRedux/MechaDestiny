using System.Collections.Generic;
using System.Linq;
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

    [Space]
    public TextAsset PostCombatScript;

    public List<Actor> GetStarterActorsCollection()
    {
        if( PlayerActors == null )
            return new List<Actor>();

        var result = PlayerActors.Select( x => x.GetDataCopySync() ).ToList();
        return result;
    }
}
