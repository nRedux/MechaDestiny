using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(LuaBehaviorComponent))]
public class LuaBehaviorComponentEditor : OdinEditor
{

    protected override void OnEnable()
    {
        TextAssetMonitor.TextAssetModified += OnTextAssetModified;
    }

    protected override void OnDisable()
    {
        TextAssetMonitor.TextAssetModified -= OnTextAssetModified;
    }

    private void OnTextAssetModified()
    {
        var luaComp = this.target as LuaBehaviorComponent;
        luaComp.AutoGenParams();
    }
}
