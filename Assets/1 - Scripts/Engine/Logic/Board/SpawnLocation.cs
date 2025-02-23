using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SpawnLocation : MonoBehaviour
{

    public PlayerType PlayerType;

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
