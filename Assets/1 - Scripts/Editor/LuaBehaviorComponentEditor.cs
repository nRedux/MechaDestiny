using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(LuaBehaviorComponent))]
public class LuaBehaviorComponentEditor : OdinEditor
{

    protected override void OnEnable()
    {
        DoAutoGenParams();
        TextAssetMonitor.TextAssetModified += OnTextAssetModified;
    }

    protected override void OnDisable()
    {
        TextAssetMonitor.TextAssetModified -= OnTextAssetModified;
    }

    private void OnTextAssetModified()
    {
        DoAutoGenParams();
    }

    private void DoAutoGenParams() 
    {
        var luaComp = this.target as LuaBehaviorComponent;
        luaComp.AutoGenParams();
    }
}
