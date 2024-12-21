using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class ActorAssetEditor : Editor
{
    SerializedProperty _actions;
    private ReorderableList _list;
 
    private void OnEnable()
    {
        _actions = serializedObject.FindProperty( nameof( ActorMoveAsset ) );
        _list = new ReorderableList( serializedObject, _actions, true, true, true, true );
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        _list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

}
