using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static UnityEngine.EventSystems.EventTrigger;
using System.Linq;
using UnityEngine.UIElements;

[System.Serializable]
public class GfxAttachment
{
    public Transform Transform;
}

public enum ComponentDestroyEffect
{
    Destroy,
    VersionSwitch
}

public enum GFXComponentType
{
    Weapon,
    MechPart
}



public class GfxComponent : MonoBehaviour
{
    public GFXComponentType ComponentType;

    public string ActionAnimation;

    /// <summary>
    /// Graphics for attack
    /// </summary>
    [ShowIf( nameof( ShowIfWeapon ) )]
    [Tooltip( "The Effect to use if this is a weapon and it fires AOE. Effect is spawned in each affected cell." )]
    public GameObjectReference ActionEffect;

    /// <summary>
    /// Graphics for aoe effect on cells
    /// </summary>
    ///     [ShowIf( nameof( ShowIfWeapon ) )]
    [ShowIf( nameof( ShowIfWeapon ) )]
    [Tooltip( "The Effect to use if this is a weapon and it fires AOE. Effect is spawned in each affected cell." )]
    public GameObject AOEGraphics;

    [Tooltip("Decides how to effect this part of the mech when it's destroyed. Destroy - hides the object. Version switch - Will make the appearance change.")]
    public ComponentDestroyEffect DestroyEffect;

    [ShowIf( nameof( ShowIfWeapon ) )]
    [Tooltip( "The location where fire effects eminate from" )]
    public Transform FirePoint;
    [Tooltip( "Attachment slots to be able to attach dynamically" )]
    public GfxAttachment[] AttachmentSlots;

    [Tooltip( "Reference to the explosion script which handles death" )]
    public MechComponentDestroyed ExplosionEffect;

    [TitleGroup( "Explosion Points" )]
    [ShowIf( nameof( ShowIfMechPart ) )]
    public Renderer SkinnedMeshRenderer;

    [ReadOnly]
    public List<Transform> SurfacePoints = new List<Transform>();

    [TitleGroup( "Explosion Points" )]
    public int NumPoints = 10;

    /// <summary>
    /// Runtime mech component data
    /// </summary>
    [HideInInspector]
    public MechComponentData ComponentData;

    /// <summary>
    /// Starting rotation
    /// </summary>
    [HideInInspector]
    public Quaternion IdentityRotation;


    private StatisticWatcher _healthWatcher;
    private bool _destroyed = false;
    private Material[] _materials;
    private Color[] _materialColors;

    [TitleGroup( "Explosion Points" )]
    [Button]
    public void CreateSurfacePoints()
    {
        ClearSurfacePoints();
        
        if( SkinnedMeshRenderer == null )
            return;

        List<Transform> combinedPoints = new List<Transform>();

        combinedPoints.AddRange( GameUtils.CreateEmptiesOnVerticesRecursive( SkinnedMeshRenderer.gameObject, NumPoints, .2f ) );

        SurfacePoints = combinedPoints;
    }


    [TitleGroup( "Explosion Points" )]
    [Button]
    public void ClearSurfacePoints()
    {
        if( SurfacePoints == null || SurfacePoints.Count == 0 )
            return;
        foreach( var empty in SurfacePoints )
        {
            if( empty == null )
                continue;
            if( Application.isPlaying )
                Destroy( empty.gameObject );
            else
                DestroyImmediate( empty.gameObject );
        }

        SurfacePoints.Clear();
    }

    public bool ShowIfMechPart()
    {
        return ComponentType == GFXComponentType.MechPart;
    }

    public bool ShowIfWeapon()
    {
        return ComponentType == GFXComponentType.Weapon;
    }


    private void Awake()
    {
        IdentityRotation = transform.localRotation;

        if( SkinnedMeshRenderer != null )
        {
            _materials = SkinnedMeshRenderer.materials;
            _materialColors = _materials.Select( x => x.GetColor("_BaseColor") ).ToArray();
        }
    }

    private void OnDestroy()
    {
        if( _healthWatcher != null )
            _healthWatcher.Stop();
    }


    private void WatchStatistics()
    {
        _healthWatcher = new StatisticWatcher( ComponentData.GetStatistic( StatisticType.Health ), OnHealthChange );
    }

    private void OnHealthChange( StatisticChange change )
    {
        if( change.Value <= 0 )
        {
            Debug.Log( this.ComponentData.ID );
            DoExplosion();
        }
    }

    private void ComponentDestroyed()
    {

        switch( DestroyEffect )
        {
            case ComponentDestroyEffect.Destroy:
                if( _healthWatcher != null )
                    _healthWatcher.Stop();
                if( _destroyed )
                    return;
                SkinnedMeshRenderer.enabled = false;

                break;

            case ComponentDestroyEffect.VersionSwitch:
                if( _materials == null )
                {
                    Debug.LogError("_materials is null");
                    return;
                }
                for( int i = 0; i < _materials.Length; i++ )
                {
                    _materials[i].color = _materialColors[i] * .5f;
                }
                break;
        }

    }

    private void DoExplosion()
    {
        StartCoroutine( DoExplosionRoutine() );
    }

    IEnumerator DoExplosionRoutine()
    {
        if(ExplosionEffect == null )
        {
            ComponentDestroyed();
            yield break;
        }
        //Done here just to sync the destruction
        ComponentData.DestroyAttachments();

        ExplosionEffect.Begin();
        yield return StartCoroutine( ExplosionEffect.AwaitComplete() );
        ComponentDestroyed();
    }

    internal void Initialize( MechComponentData data )
    {
        this.ComponentData = data;
        WatchStatistics();
        //Debug.Log( "Mech component initialized", gameObject );
    }

    internal Vector3 GetFirePosition()
    {
        return FirePoint.transform.position;
    }

    internal Vector3 GetFireForward()
    {
        return FirePoint.transform.forward;
    }

    public async void CreateShotEffect( AttackActionResult result )
    {
        var effectAsset = await ActionEffect.GetAssetAsync();
        if( effectAsset == null )
        {
            Debug.LogError("Error creating attack effect. Failed loading asset.");
            return;
        }

        var effectComp = effectAsset.GetComponent<ActionEffect>();

        ActionEffect effectInstance = null;
        if( effectComp != null )
        {
            effectInstance = Instantiate<ActionEffect>( effectComp );
        }
        else
        {
            Debug.LogError( "No ActionEffect component." );
            return;
        }

        result.AOEGraphics = AOEGraphics;

        effectInstance.transform.position = GetFirePosition();
        effectInstance.transform.forward = GetFireForward();
        effectInstance.Run( result, FirePoint );
    }

}
