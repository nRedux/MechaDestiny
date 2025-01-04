using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System;

[CustomEditor(typeof(SpawnLocation), isFallback = true)]
public class SpawnLocationEditor : OdinEditor
{
    private void OnSceneGUI()
    {
        foreach( var target in this.targets )
        {
            if( target is SpawnLocation blocker )
            {
                if( blocker.transform.hasChanged )
                {
                    GameEngine engine = FindFirstObjectByType<GameEngine>();
                    if( engine == null )
                        return;

                    Vector3 position = blocker.transform.position;


                    position.x = Mathf.Round( position.x - .5f) + .5f;
                    position.z = Mathf.Round( position.z - .5f ) + .5f;
                    blocker.transform.position = position;

                    blocker.transform.hasChanged = false;
                }
            }
        }
    }


}
