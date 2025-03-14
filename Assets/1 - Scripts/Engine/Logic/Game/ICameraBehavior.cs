using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static UnityEditor.SceneView;

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

    private AvatarCamera _camera;


    public AttackCameraBehavior( Actor actor, SmartPoint target )
    {
        _actor = actor;
        _avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        _target = target;

        _camera = _avatar.GetCamera( "attack" );
        
    }

    public async Task Begin()
    {
        bool camDone = false;
        if( _camera != null )
            camDone = true;

        _camera.Activate( () =>
        {
            camDone = true;
        }, _avatar.transform, _target );

        while( !camDone )
        {
            await Task.Yield();
        }
    }

    public async Task End()
    {
        _camera.Deactivate();
        await Task.Yield();
    }
}

public class SpecialCameraBehavior : ICameraBehavior
{

    private Actor _actor;
    SmartPoint _target;
    private GfxActor _avatar;

    private AvatarCamera _camera;

    public SpecialCameraBehavior( Actor actor, SmartPoint target )
    {
        _actor = actor;
        _avatar = GameEngine.Instance.AvatarManager.GetAvatar( actor );
        _target = target;

        _camera = _avatar.GetCamera( "special" );
    }

    public async Task Begin()
    {
        bool camDone = false;

        _camera.Activate( () =>
        {
            camDone = true;
        }, _avatar.transform, _target );

        while( !camDone )
        {
            await Task.Yield();
        }
    }

    public async Task End()
    {
        _camera.Deactivate();
        await Task.Yield();
    }
}