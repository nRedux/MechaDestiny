using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu( menuName = "Engine/Global Settings" )]
public class GlobalSettings:SingletonScriptableObject<GlobalSettings>
{

    public List<WeaponTypeInfo> WeaponTypes;
}
