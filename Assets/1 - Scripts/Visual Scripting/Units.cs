using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Units : MonoBehaviour
{
   
    public static void ChangedScene( bool initialState )
    {
        SceneLoadManager.LoadScene( "SampleScene", true, initialState, null, null );
    }

}
