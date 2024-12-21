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


    public override void AssignEntity( IEntity entity )
    {
        if( entity == null )
            return;

        var actor = entity as Actor;
        var mechData = actor.GetSubEntities()[0] as MechData;

        this._mechData = mechData;
        base.AssignEntity( entity );
        Torso.AssignEntity( mechData.Torso );
        Legs.AssignEntity( mechData.Legs );
        LeftArm.AssignEntity( mechData.LeftArm );
        RightArm.AssignEntity( mechData.RightArm );
    }

}
