using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

/*
[CustomEditor(typeof(PoissonField))]
public class PoissonFieldEditor : OdinEditor
{
    private PoissonField triangulatePoints;



    protected override void OnEnable()
    {
        base.OnEnable();
        triangulatePoints = target as PoissonField;

        //Hide the main GOs move/rot/scale handle
        Tools.hidden = true;
    }



    protected override void OnDisable()
    {
        base.OnDisable();
        //Un-hide the main GOs move/ rot / scale handle
        Tools.hidden = false;
    }



    private void OnSceneGUI()
    {
        //So you we cant click on anything else in the scene
        HandleUtility.AddDefaultControl(0);

        //Move the constrains points
        List<Vector3> constraints = triangulatePoints.constraints;

        if (constraints != null)
        {
            for (int i = 0; i < constraints.Count; i++)
            {
                Vector3 newPos = MovePoint(constraints[i]);

                constraints[i] = newPos;
            }
        }
    }



    private Vector3 MovePoint(Vector3 pos)
    {
        if (Tools.current == Tool.Move)
        {
            //Check if we have moved the point
            EditorGUI.BeginChangeCheck();

            //Get the new position and display it
            pos = Handles.PositionHandle(pos, Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                //Save the new value
                EditorUtility.SetDirty(target);

                triangulatePoints.GenerateTriangulation();
            }
        }

        return pos;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        //Update when changing value in inspector
        EditorGUI.BeginChangeCheck();
        base.DrawDefaultInspector();
        if ( EditorGUI.EndChangeCheck() )
        {
            triangulatePoints.GenerateTriangulation();

            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Triangulate points"))
        {
            triangulatePoints.GenerateTriangulation();

            //Will not work because the classes in the triangle is not set to serializable 
            EditorUtility.SetDirty(target);
        }
    }
}
*/