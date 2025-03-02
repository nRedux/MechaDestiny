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

        _actor = await _avatarManager.CreateGfxActor( mechData, null );
        _actor.transform.position = Target.position;
        _actor.transform.Rotate( Vector3.up * 180f );
    }


    private void PrepareForNewMech()
    {
        if( _actor == null )
            return;

        Destroy( _actor.gameObject );
        _actor = null;
    }



}
