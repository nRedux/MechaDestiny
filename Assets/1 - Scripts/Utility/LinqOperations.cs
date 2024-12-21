using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LinqOperations
{
    public static void Do<TSource>( this IEnumerable<TSource> collection, System.Action<TSource> action )
    {
        if( action == null )
            return;

        if( collection == null )
            return;

        foreach( var item in collection )
        {
            action.Invoke( item );
        }
    }

    public static bool IfAny<TSource>( this IEnumerable<TSource> collection, System.Func<TSource, bool> condition )
    {
        if( condition == null )
            return false;

        if( collection == null )
            return false;

        foreach( var item in collection )
        {
            if( condition.Invoke( item ) )
                return true;
        }
        return false;
    }

    public static IEnumerable<TSource> NonNull<TSource>( this IEnumerable<TSource> source ) where TSource: class
    {
        if( source == null )
            return null;

        return source.Where( x => x != null );
    }

    public static IEnumerable<UnityEngine.Object> NonNull( this IEnumerable<UnityEngine.Object> source )
    {
        if( source == null )
            return null;

        return source.Where( x => x.Opt() != null );
    }

    public static TSource Opt<TSource>( this TSource unityObject ) where TSource : UnityEngine.Object
    {
        if( unityObject == null )
            return null;
        return unityObject;
    }

    public static int Count<TSource>( this IEnumerable<TSource> collection, System.Func<TSource, bool> predicate )
    {
        if( predicate == null )
            throw new ArgumentNullException( "Null predicate" );

        int count = 0;
        collection.Do( x => { count += predicate( x ) ? 1 : 0; } );
        return count;
    }


    public static TSource Random<TSource>( this IEnumerable<TSource> collection )
    {
        int randomSkip = UnityEngine.Random.Range( 0, collection.Count() );
        return collection.Skip( randomSkip ).FirstOrDefault();
    }

    public static List<TSource> Random<TSource>( this IEnumerable<TSource> collection, int count )
    {
        List<TSource> source = collection.ToList();
        List<TSource> result = new List<TSource>();
        for( int i = 0; i < count; i++ )
        {
            if( source.Count == 0 )
                return result;

            var rand = source.Random();
            source.Remove( rand );
            result.Add( rand );
        }
        return result;
    }

}
