using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class RunMoonSharp : MonoBehaviour
{
    [TextArea( 5, 20 )]
    public string script;

    public int A;
    public int B;

    public TextAsset Script;
    public List<SpawnLocation> SpawnLocations;

    private Script _scriptObject = null;


    public static int Add( int a, int b )
    {
        return a + b;
    }

    const string UPDATE = "update";

    [Button]
    public void RunTest()
    {
       
    }

    public static void Log( string arg )
    {
        Debug.Log( arg );
    }
}
