using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
    bool IsPhysical { get; }
    List<IEntity> GetSubTargets();
}
