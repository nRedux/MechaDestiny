using UnityEngine;
using UnityEditor;

public class MechSetupWindow: EditorWindow
{
    private GameObject _root;

    [MenuItem( "Examples/My Editor Window" )]
    public static void ShowExample()
    {
        MechSetupWindow wnd = GetWindow<MechSetupWindow>();
        wnd.titleContent = new GUIContent( "Mech Setup Wizard" );
    }

    private void OnGUI()
    {
        _root = EditorGUILayout.ObjectField( _root, typeof( GameObject ), false ) as GameObject;

        bool doSetup = GUILayout.Button( "Setup" );
        if( doSetup )
        {

        }
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

}
