using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;



public class SpawnLocation : MonoBehaviour
{
    public PlayerType PlayerType;
    public TextAsset ScriptForActor;
    public int Priority;

    public void UseMe( Actor actor, Game game )
    {
        actor.SpawnPriority = Priority;
        actor.SetPosition( this.transform.position.ToVector2Int(), game );

        if( ScriptForActor != null ) {
            Dictionary<string, object> props = new Dictionary<string, object> { { "thisActor", actor } };
            actor.LuaScript = new LuaBehavior( ScriptForActor, properties: props );
            //actor.LuaScript.AssignActor( actor );
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float baseCubeHeight = .05f;
        float wireCubeHeight = 1f;

        Color oldGizmosColor = Gizmos.color;
        Color spawnLocColor = PlayerType == PlayerType.Ally ? MechEditorProjectPreferences.Settings.AllySpawnColor : MechEditorProjectPreferences.Settings.EnemySpawnColor;
        Gizmos.color = spawnLocColor;
        Gizmos.DrawCube( transform.position + Vector3.up * baseCubeHeight * .5f, new Vector3( 1, baseCubeHeight, 1f ) );
        Gizmos.DrawWireCube( transform.position + Vector3.up * wireCubeHeight * .5f, new Vector3( 1, wireCubeHeight, 1f ) );

        Gizmos.color = oldGizmosColor;
    }
#endif

}
