using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class AttackActionResult : ActionResult
{
    public GfxActor Attacker;
    public MechComponentData AttackerMechComponent;
    public MechComponentData AttackerWeapon;

    public int Count = 1;
    public Transform _fireSource;
    public SequencePos SequencePosition = SequencePos.Unset;

    private Vector2Int _target;
    private GfxActor _targetAvatar;

    ActionResultStatus _status = ActionResultStatus.Running;


    public AttackActionResult( Vector2Int target, Transform fireSource )
    {
        _target = target;
        _fireSource = fireSource;

        var actor = UIManager.Instance.GetActorAtCell( target );
        _targetAvatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
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

        /*!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
         * TODO: Must make camera work with new system!!!
         * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! */
        //PS!!!!!!!!: The below method (StartAttackCamera) doesn't work out if attack actions executed immediately after one another. There doesn't end up bring a transition to listen for!

        if( this.SequencePosition == SequencePos.Start || this.SequencePosition == SequencePos.Unset )
        {
            Attacker.StartAttackCamera( () =>
            {
                camDone = true;
            }, Attacker, TargetAvatar );
        

            //OnComplete += () => { Attacker.StopAttackCamera(); };
            while( !camDone )
            {
                await Task.Yield();
            }
        }

        await Attacker.ExecuteAction( this, () =>
        {
            _status = ActionResultStatus.Finished;
            GameEngine.Instance.Game.CheckGameOver();
        } );

        if( this.SequencePosition == SequencePos.End || this.SequencePosition == SequencePos.Unset )
            Attacker.StopAttackCamera();
    }


    public override ActionResultStatus Update()
    {
        return _status;
    }

}
