using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This UI's intention is to be capable of displaying entity statistic information.
/// We don't however want to force a specific format.
/// </summary>

public class UIEntity : MonoBehaviour
{
    public IEntity Entity;

    public System.Action<IEntity> EntityAssigned;

    public virtual void AssignEntity( IEntity entity )
    {
        Entity = entity;
        EntityAssigned?.Invoke( entity );
    }

}
