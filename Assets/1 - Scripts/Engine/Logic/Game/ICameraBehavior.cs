using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public interface ICameraBehavior
{
    public Task Begin();

    public Task End();

}


public class AttackCameraBehavior : ICameraBehavior
{

    private Actor _actor;
    SmartPoint _target;
    private GfxActor _avatar;

    public AttackCameraBehavior( Actor actor, SmartPoint target )
    {
        _actor = actor;
        _avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        _target = target;
    }

    public async Task Begin()
    {
        bool camDone = false;
        _avatar.StartAttackCamera( () =>
        {
            camDone = true;
        }, _avatar, _target );

        while( !camDone )
        {
            await Task.Yield();
        }
    }

    public async Task End()
    {
        _avatar.StopAttackCamera();
        await Task.Yield();
    }
}