using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(BoardBlocker))]
public class BoardBlockerEditor : OdinEditor
{
    private void OnSceneGUI()
    {
        foreach( var target in this.targets )
        {
            if( target is BoardBlocker blocker )
            {
                if( blocker.transform.hasChanged )
                {
                    GameEngine engine = FindObjectOfType<GameEngine>();
                    if( engine == null )
                        return;

                    Vector3 euler = blocker.transform.eulerAngles;
                    euler.z = euler.x = 0;
                    euler.y = ((int)euler.y / 90) * 90f;
                    blocker.transform.eulerAngles = euler;
                    

                    engine.UpdateBoardData();

                    blocker.transform.hasChanged = false;
                }
            }
        }
    }


}
