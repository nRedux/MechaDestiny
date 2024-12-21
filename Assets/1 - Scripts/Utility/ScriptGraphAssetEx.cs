using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public static class ScriptGraphAssetEx
{
    public static void Run( this ScriptGraphAsset graph, System.Action onComplete )
    {
        VisualScriptingUtility.RunGraph( graph, onComplete );
    }
}
