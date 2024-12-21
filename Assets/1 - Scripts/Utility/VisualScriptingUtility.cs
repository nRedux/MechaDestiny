using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using System.Linq;

public static class VisualScriptingUtility
{

    public static GameObject RunGraph( ScriptGraphAsset asset, System.Action onFinished )
    {
        var go = new GameObject();
        go.name = asset.name + "_NarrativeGraph";
        Object.DontDestroyOnLoad( go );
        var exe = go.AddComponent<NarrativeExecution>();
        var graph = go.AddComponent<ScriptMachine>();
        var m = graph as Machine<FlowGraph, ScriptGraphAsset>;
        m.nest.SwitchToMacro( asset );

        exe.OnFinished += () =>
        {
            onFinished?.Invoke();
            GameObject.Destroy( go );
        };

        return go;
    }

    public static List<T> GetVariablesOfType<T>( ScriptGraphAsset graphAsset )
    {
        try
        {
            return graphAsset.graph.variables.Where( x => x.value is T ).Select( x => (T) x.value ).ToList();
        }
        catch
        {
            return new List<T>();
        }
    }

    public static Dictionary<string, object> CollectVariables( this ScriptMachine scriptMachine )
    {
        Dictionary<string, object> varData = new Dictionary<string, object>();
        var variables = Variables.Graph( scriptMachine.GetReference() );

        Debug.Log( $"Collecting variables from Bolt graph {scriptMachine.name}" );
        foreach( var variable in variables )
        {
            Debug.Log( $"{variable.name}: {variable.value.ToString()}" );
            varData.Add( variable.name, variable.value );
        }
        return varData;
    }

    public static void ReplaceVariables( this ScriptMachine scriptMachine, Dictionary<string, object> replacementVars )
    {
        if( replacementVars == null )
            return;

        Debug.Log( $"Replacing variables in activtor {scriptMachine.name}" );
        var variables = Variables.Graph( scriptMachine.GetReference() );

        foreach( var variable in variables )
        {
            object replacementVal = null;
            if( replacementVars.TryGetValue( variable.name, out replacementVal ) )
            {
                variable.value = replacementVal;
                Debug.Log( $"Replacing {variable.name}: New Value {variable.value.ToString()}" );
            }
            else
            {
                Debug.Log( $"No replacement for variable {variable.name}" );
            }
        }
        Debug.Log( $"Finished replacement for activtor {scriptMachine.name} variables." );
    }
}
