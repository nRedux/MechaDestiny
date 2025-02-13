using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using TMPro;

public class AttackActionResult : ActionResult
{
    public GfxActor Attacker;
    public MechComponentData AttackerMechComponent;
    public MechComponentData AttackerWeapon;
    public bool Evaded = false;

    public int Count = 1;
    public Transform _fireSource;
    public SequencePos SequencePosition = SequencePos.Both;
    //Has default, AI isn't set up with custom props when attacking
    public ResultDisplayProps DisplayProps = new ResultDisplayProps() { DoArmEnd = true, DoArmStart = true, IsSequenceEnd = true, IsSequenceStart = true };

    public List<Vector2Int> AffectedAOECells = new List<Vector2Int>();
    public GameObject AOEGraphics = null;

    private ActionResultStatus _status = ActionResultStatus.Running;


    public AttackActionResult()
    {
    }

    public AttackActionResult( GfxActor attacker, SmartPoint target, MechComponentData weapon )
    {
        Attacker = attacker;
        AttackerWeapon = weapon;
        AttackerMechComponent = AttackerWeapon.GetParent() as MechComponentData;
        Count = AttackerWeapon.GetStatisticValue( StatisticType.ShotCount );
        Target = target;
    }

    public string GetAnimationParam()
    {
        string prefix = string.Empty;
        switch( AttackerMechComponent.Type )
        {
            case MechComponentType.LeftArm:
                prefix = "L_";
                break;
            case MechComponentType.RightArm:
                prefix = "R_";
                break;
            default:
                throw new System.Exception("Bad setup!");
        }

        return prefix + AttackerWeapon.ModelInstance.ActionAnimation;
    }


    public override async void Start()
    {
        bool camDone = false;

        if( this.DisplayProps.IsSequenceStart )
        {
            Attacker.StartAttackCamera( () =>
            {
                camDone = true;
            }, Attacker, Target );
        

            //OnComplete += () => { Attacker.StopAttackCamera(); };
            while( !camDone )
            {
                await Task.Yield();
            }
        }

        if( Evaded )
        {
            UIDamageNumbers.Instance.CreatePop( "Evaded", Target.Position );
        }

        await Attacker.ExecuteAction( this, () =>
        {
            _status = ActionResultStatus.Finished;
            GameEngine.Instance.Game.CheckGameOver();
        } );

        if( this.DisplayProps.IsSequenceEnd )
            Attacker.StopAttackCamera();
    }


    public override ActionResultStatus Update()
    {
        return _status;
    }

    internal bool IsAOE()
    {
        return AttackerWeapon.IsAOE();
    }
}
