using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UIMechInfo : UIEntity
{
    public UIMechComponent Torso;
    public UIMechComponent Legs;
    public UIMechComponent LeftArm;
    public UIMechComponent RightArm;

    private MechData _mechData;

    /// <summary>
    /// Expects an actor. Not sure
    /// </summary>
    /// <param name="entity"></param>
    public override void AssignEntity( IEntity entity )
    {
        if( entity == null )
            return;

        MechData mechData = null;
        if( entity is MechData argMechData )
        {
            mechData = argMechData;
        }
        else if( entity is Actor actor )
        {
            mechData = actor.GetSubEntities()[0] as MechData;
        }

        this._mechData = mechData;
        base.AssignEntity( entity );
        Torso.AssignEntity( mechData.Torso );
        Legs.AssignEntity( mechData.Legs );
        LeftArm.AssignEntity( mechData.LeftArm );
        RightArm.AssignEntity( mechData.RightArm );
    }

}
