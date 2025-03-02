using UnityEngine;

public class MechRenderer : MonoBehaviour
{
    private GfxAvatarManager _avatarManager;

    public Transform Target;

    private GfxActor _actor = null;

    public async void StartRenderingMech( MechData mechData )
    {
        PrepareForNewMech();

        if( _avatarManager == null )
            _avatarManager = new GfxAvatarManager();

        var actor = await _avatarManager.CreateGfxActor( mechData, null );
        actor.transform.position = Target.position;
    }


    private void PrepareForNewMech()
    {
        if( _actor == null )
            return;

        Destroy( _actor.gameObject );
        _actor = null;
    }



}
