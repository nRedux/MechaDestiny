using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Engine/Entities/Actor List")]
public class ActorListAsset: ScriptableObject
{
    [SerializeField]
    private List<ActorReference> _actors;

    public List<ActorReference> GetActors()
    {
        return new List<ActorReference>(_actors );
    }
}

[Serializable]
public class ActorListCollection
{
    public ActorListAsset[] Lists = new ActorListAsset[] { };
}