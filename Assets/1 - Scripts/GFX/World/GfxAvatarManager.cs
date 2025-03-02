using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class GfxAvatarManager
{

    public System.Action<GfxActor> ActorCreated;


    private Dictionary<Actor, GfxActor> _actors = new Dictionary<Actor, GfxActor>();


    public async Task<GfxActor> CreateGfxActor( Actor actor, System.Action<GfxActor> onReady )
    {
        if( _actors.ContainsKey( actor ) )
            return _actors[actor];
        try
        {
            var avatarGO = await actor.Avatar.GetAssetAsync( );

            var gfxActor = avatarGO.GetComponent<GfxActor>();
            if( gfxActor == null )
            {
                Debug.LogError( $"Failed to Create GfxActor. No GfxActor component {avatarGO.name}" );
                return null;
            }

            var avatarInstance = GameObject.Instantiate<GameObject>( avatarGO );
            var gfxActorInstance = avatarInstance.GetComponent<GfxActor>();
            avatarInstance.name += "__" + actor.GetTeamID().ToString();

            BuildMech( actor.GetMechData(), gfxActorInstance );
            gfxActorInstance.Initialize( actor );
            gfxActorInstance.CalculateBounds();

            _actors.Add( actor, gfxActorInstance );
            ActorCreated?.Invoke( gfxActorInstance );
            onReady?.Invoke( gfxActorInstance );

            return gfxActorInstance;
        }
        catch( System.Exception e )
        {
            Debug.LogException( e );
            Debug.LogError( "Failed to create GFX Actor" );
        }

        return null;
    }

    public async Task<GfxActor> CreateGfxActor( MechData mech, System.Action<GfxActor> onReady )
    {
        try
        {
            var avatarGO = await mech.Avatar.GetAssetAsync();

            var gfxActor = avatarGO.GetComponent<GfxActor>();
            if( gfxActor == null )
            {
                Debug.LogError( $"Failed to Create GfxActor. No GfxActor component {avatarGO.name}" );
                return null;
            }

            var avatarInstance = GameObject.Instantiate<GameObject>( avatarGO );
            var gfxActorInstance = avatarInstance.GetComponent<GfxActor>();

            BuildMech( mech, gfxActorInstance );
            gfxActorInstance.CalculateBounds();

            ActorCreated?.Invoke( gfxActorInstance );
            onReady?.Invoke( gfxActorInstance );

            return gfxActorInstance;
        }
        catch( System.Exception e )
        {
            Debug.LogException( e );
            Debug.LogError( "Failed to create GFX Actor" );
        }

        return null;
    }


    public void BuildMech( MechData mechData, GfxActor gfxActor )
    {
        //Build attachments
        BuildComponent( mechData.LeftArm, gfxActor.LeftArm );
        BuildComponent( mechData.RightArm, gfxActor.RightArm );

        //Make sure the data is linked between graphics and the runtime data.
        gfxActor.Torso.Opt()?.Initialize( mechData.Torso );
        gfxActor.Legs.Opt()?.Initialize( mechData.Legs );
        gfxActor.LeftArm.Opt()?.Initialize( mechData.LeftArm );
        gfxActor.RightArm.Opt()?.Initialize( mechData.RightArm );
    }


    private void OnMechHealthChange( StatisticChange change )
    {
        throw new NotImplementedException();
    }

    public async void BuildComponent( MechComponentData component, GfxComponent gfxComponent )
    {
        if( gfxComponent == null )
        {
            Debug.LogError($"{nameof(gfxComponent)} argument cannot be null.");
            return;
        }

        if( component == null )
            return;
        for( int i = 0; i < component.Attachments.Length; i++ )
        {
            var attachment = component.Attachments[i];

            if( attachment.Data == null )
                continue;

            if( !attachment.Data.Model.RuntimeKeyIsValid() )
                continue;

            var attachmentModel = await attachment.Data.Model.GetAssetAsync();

            var gfxInstance = attachmentModel.GetComponent<GfxComponent>();
            if( gfxInstance == null )
            {
                Debug.LogError( $"Failed to Create GfxActor. No GfxComponent {attachmentModel.name}" );
                return;
            }

            var avatarInstance = GameObject.Instantiate<GameObject>( attachmentModel );
            attachment.Data.ModelInstance = avatarInstance.GetComponent<GfxComponent>();
            attachment.Data.ModelInstance.Initialize( attachment.Data );

            attachment.Data.ModelInstance.transform.SetParent( gfxComponent.AttachmentSlots[i].Transform );
            attachment.Data.ModelInstance.transform.localPosition = Vector3.zero;
            attachment.Data.ModelInstance.transform.localRotation = Quaternion.identity;
        }
    }


    public GfxActor GetAvatar( Actor actor )
    {
        if( !_actors.ContainsKey( actor ) )
            return null;
        return _actors[actor];
    }

    /*
    public IEnumerator DoDestroySequenceAllDeadActors()
    {
        yield return CoroutineUtils.Instance.StartCoroutine( DestroySequenceAllDeadActors() );
    }

    public IEnumerator DestroySequenceAllDeadActors()
    {
        //Hacky check to make sure anything which needs to die visually does.
        var dead = GameEngine.Instance.Game.Teams.SelectMany( x =>
        {
            var members = x.GetMembers();
            return members.Where( y => y.GetMechData().IsTorsoDestroyed() );
        } ).ToList();

        GfxActor lastDead = null;
        for( int i = 0; i < dead.Count; i++ )
        {
            var avatar = GameEngine.Instance.AvatarManager.GetAvatar( dead[i] );
            if( avatar == null )
                continue;
            lastDead = avatar;
            
            if( i == dead.Count - 1 )
            {
                yield return lastDead.DoDeathSequence();
            }
            else
            {
                CoroutineUtils.BeginCoroutine( lastDead.DoDeathSequence() );
            }
        }
    }*/
}