using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering.HighDefinition;

public class AttackActionResult : ActionResult
{
    public GfxActor Attacker;
    public MechComponentData AttackerMechComponent;
    public MechComponentData AttackerWeapon;
    public bool Evaded = false;

    public int Count = 1;
    public Transform _fireSource;

    //Has default, AI isn't set up with custom props when attacking
    public ResultDisplayProps DisplayProps = new ResultDisplayProps() { DoArmEnd = true, DoArmStart = true, IsSequenceEnd = true, IsSequenceStart = true };

    public List<Vector2Int> AffectedAOECells = new List<Vector2Int>();
    public GameObject AOEGraphics = null;

    private ActionResultStatus _status = ActionResultStatus.Running;

    private ICameraBehavior _camera;


    public AttackActionResult()
    {
    }

    public AttackActionResult( GfxActor attacker, SmartPoint target, MechComponentData weapon )
    {
        Attacker = attacker;
        AttackerWeapon = weapon;
        if( AttackerWeapon != null && AttackerWeapon.IsBroken() )
            Debug.Log( "Attacking with broken weapon" );
        AttackerMechComponent = AttackerWeapon.GetParent() as MechComponentData;
        Count = AttackerWeapon.GetStatisticValue( StatisticType.ShotCount );
        Target = target;

        _camera = new AttackCameraBehavior( attacker.Actor, target );
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
        if( this.DisplayProps.IsSequenceStart )
        {
            await _camera.Begin();
        }

        if( Evaded )
        {
            UIInfoPopups.Instance.CreatePop( "Evaded", Target.GfxActor.GetTorsoPosition() );
        }

        

        await Attacker.ExecuteAction( this, () =>
        {
            _status = ActionResultStatus.Finished;
            GameEngine.Instance.Game.CheckGameOver();
        } );

        if( this.DisplayProps.IsSequenceEnd )
            await _camera.End();
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
