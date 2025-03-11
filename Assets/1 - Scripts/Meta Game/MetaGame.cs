using MoonSharp.Interpreter;
using Unity.VisualScripting;
using UnityEngine;

public class MetaGame
{
    
    public static void ShowDialogBox( string title, string content, DynValue onContinue )
    {
        var callback = onContinue.Function.GetDelegate<System.Action>();
        SUIManager.Instance.UISystems.DialogBox.OnContinue = () => { callback.Invoke(); };
        SUIManager.Instance.UISystems.DialogBox.Show( title, content );
    }

    public static void ChangeScene( string scene, bool warmupScene, DynValue onContinue )
    {
        var callback = onContinue.Function.GetDelegate<System.Action>();
        SceneLoadManager.LoadScene( scene, true, warmupScene, () => { callback.Invoke(); }, null );
    }


}
