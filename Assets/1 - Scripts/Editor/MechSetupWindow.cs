using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using Unity.Cinemachine;
using UnityEditor.Events;
using FMODUnity;

public class MechSetupWindow: EditorWindow
{
    private const string EventPath = "SFX/Combat/Mech Sounds/Foot Step";

    private GameObject _root;

    private const int PADDING = 10;

    [MenuItem( "Examples/My Editor Window" )]
    public static void ShowExample()
    {
        MechSetupWindow wnd = GetWindow<MechSetupWindow>();
        wnd.titleContent = new GUIContent( "Mech Setup Wizard" );
    }

    private void OnGUI()
    {
        //_root = EditorGUILayout.ObjectField( _root, typeof( GameObject ), false ) as GameObject;

        Rect buttonsArea = new Rect( PADDING, PADDING, 150, 110 );
        GUILayout.BeginArea( buttonsArea );
        GUILayout.BeginVertical();

        if( Selection.activeTransform == null )
            GUI.enabled = false;

        if( GUILayout.Button("Setup ROOT") )
        {
            SetupRoot( Selection.activeTransform );
        }

        if( GUILayout.Button( "Setup Torso" ) )
        {
            SetupTorso( Selection.activeTransform );
        }
        if( GUILayout.Button( "Setup Arm" ) )
        {
            SetupTorso( Selection.activeTransform );
        }
        if( GUILayout.Button( "Setup Legs" ) )
        {
            SetupTorso( Selection.activeTransform );
        }

        GUI.enabled = true;

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void SetupBase()
    {
        SetupAnimator();
    }

    private void SetupAnimator()
    {
        var animator = _root.GetOrAddComponent<Animator>();

        //Optionally, create an animator controller asset and assign it to the mech.
    }

    private void SetupRoot(Transform tForm)
    {
        var gfxActor = Undo.AddComponent<GfxActor>( tForm.gameObject );
        gfxActor.AnimatorMoveSpeed.Name = "WalkSpeed";

        if( GlobalSettings.Instance.AttackCameraPrefab != null )
        {
            var atkCamPrefabInst = PrefabUtility.InstantiatePrefab( GlobalSettings.Instance.AttackCameraPrefab ) as GameObject;
            gfxActor.AttackCamera = atkCamPrefabInst.Opt()?.GetComponent<AttackCamera>();
        }

        var animator = Undo.AddComponent<Animator>( tForm.gameObject );

        var cineCamEvents = Undo.AddComponent<CinemachineCameraEvents>( tForm.gameObject );
        cineCamEvents.EventTarget = null;
        UnityEventTools.AddPersistentListener<ICinemachineMixer, ICinemachineCamera>( cineCamEvents.BlendFinishedEvent, gfxActor.AttackCameraRunning );
        EditorUtility.SetDirty( gfxActor );

        var capsule = Undo.AddComponent<CapsuleCollider>( tForm.gameObject );
        capsule.center = new Vector3( 0f, .5f, 0f );
        capsule.radius = .4f;
        capsule.height = 1f;

        var mechAudio = Undo.AddComponent<MechAudio>( tForm.gameObject );

        var fmodEmitter = Undo.AddComponent<StudioEventEmitter>( tForm.gameObject );
        mechAudio.Footsteps = fmodEmitter;
        fmodEmitter.EventReference = EventReference.Find( EventPath );
    }

    private void SetupArm( Transform tForm )
    {
        var gfxComp = Undo.AddComponent<GfxComponent>( tForm.gameObject );
        var destroyedComp = Undo.AddComponent<MechComponentDestroyed>( tForm.gameObject );
        destroyedComp.ExplosionPrefab = GlobalSettings.Instance.DefaultCompDestroyExplosion;

        gfxComp.NumPoints = 10;
        gfxComp.ComponentType = GFXComponentType.MechPart;
        gfxComp.AttachmentSlots = new GfxAttachment[0];
        gfxComp.DestroyEffect = ComponentDestroyEffect.Destroy;
        gfxComp.ExplosionEffect = destroyedComp;
    }

    private void SetupTorso( Transform tForm )
    {
        var gfxComp = Undo.AddComponent<GfxComponent>( tForm.gameObject );

        gfxComp.NumPoints = 10;
        gfxComp.ComponentType = GFXComponentType.MechPart;
        gfxComp.DestroyEffect = ComponentDestroyEffect.VersionSwitch;
        gfxComp.AttachmentSlots = new GfxAttachment[0];
    }

    private void SetupLeg( Transform tForm )
    {
        var gfxComp = Undo.AddComponent<GfxComponent>( tForm.gameObject );
        var destroyedComp = Undo.AddComponent<MechComponentDestroyed>( tForm.gameObject );
        destroyedComp.ExplosionPrefab = GlobalSettings.Instance.DefaultCompDestroyExplosion;

        gfxComp.NumPoints = 10;
        gfxComp.ComponentType = GFXComponentType.MechPart;
        gfxComp.AttachmentSlots = new GfxAttachment[0];
        gfxComp.DestroyEffect = ComponentDestroyEffect.Destroy;
        gfxComp.ExplosionEffect = destroyedComp;
    }

}
