using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class UnityExtensions
{
    private static readonly Vector3 ZERO = new Vector3( 0, 0, 0 );
    public static T Duplicate<T>( this T unityObject ) where T: Object
    {
        if( unityObject == null )
            return null;

        var inst = Object.Instantiate<T>( unityObject );


        return inst;
    }

    public static GameObject Duplicate( this GameObject unityObject, Vector3 position )
    {
        if( unityObject == null )
            return null;

        var inst = Object.Instantiate( unityObject );
        inst.transform.position = position;

        return inst;
    }

    public static T GetOrAddComponent<T>( this GameObject gameObject ) where T: Component
    {
        return gameObject.GetComponent<T>().Opt() ?? gameObject.AddComponent<T>().Opt();
    }

    public static void DestroyChildren( this GameObject gameObject )
    {
        if( gameObject == null )
            return;
        Transform tr = gameObject.transform;
        for( int i = tr.childCount - 1; i >= 0; i-- )
        {
            var child = tr.GetChild( i );
            Object.Destroy( child.gameObject );
        }
    }

    public static void DestroyChildren( this Transform transform )
    {
        if( transform == null )
            return;
        for( int i = 0; i < transform.childCount; i++ )
        {
            var child = transform.GetChild( i );
            UnityEngine.Object.Destroy( child.gameObject );
        }
    }



    /**************************************************
     * 
     * 
     * I wrote this method below so I could implement easy on hover pop ups for hovering on the world map.
     * I need to get into being able to visualize things
     * 
     * I'm working toward moving to harvest patches, then having to fight enemies to clear the area to be harvesting.
     * 
     * DO IT!!!!!!!!!!!!!!!!!!!!
     * 
     * 
     **************************************************/

    /// <summary>
    /// This sets your screen position within canvas space.
    /// </summary>
    /// <param name="transform">The screen position</param>
    public static void UIPositionOverWorld( this Transform transform, Vector3 worldPosition )
    {
        var _rectTransform = transform.GetComponent<RectTransform>();
        var _canvasRectTransform = transform.GetComponentInParent<Canvas>().Opt()?.GetComponent<RectTransform>();
        _rectTransform.Opt()?.PositionOverWorld( _canvasRectTransform, worldPosition );
    }

    public static Vector2Int ToVector2Int( this Vector2 vec2 )
    {
        return new Vector2Int( Mathf.FloorToInt( vec2.x ), Mathf.FloorToInt( vec2.y ) );
    }

    public static Vector2Int ToVector2Int( this Vector3 vec3 )
    {
        return new Vector2Int( Mathf.FloorToInt( vec3.x ), Mathf.FloorToInt( vec3.z ) );
    }

    public static IEnumerable<TInterfaceType> GetInterfaceComponents<TInterfaceType>( this GameObject obj ) where TInterfaceType: class
    {
       return obj.GetComponents<Component>().Where( x => typeof(TInterfaceType).IsAssignableFrom( x.GetType() ) ).Select( x => x as TInterfaceType );
    }

    public static IEnumerable<TInterfaceType> GetInterfaceComponents<TInterfaceType>( this Component obj ) where TInterfaceType : class
    {
        return obj.gameObject.GetInterfaceComponents<TInterfaceType>();
    }
}
