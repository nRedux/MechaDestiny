using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;

[System.Serializable]
public class ActorReference : DataProviderReference<ActorAsset, Actor>
{
    public ActorReference( string guid ) : base( guid ) { }
}

public class ActorReferenceDescriptor : SimpleReferenceDescriptor<ActorReference> { }

// A complete implementation of IUserDataDescriptor for MyCustomClass.
public class SimpleReferenceDescriptor<T> : IUserDataDescriptor
{
    // A friendly name by which Lua scripts can refer to the type.
    public string Name => nameof( T );

    // The actual type that this descriptor wraps.
    public Type Type => typeof( T );

    // A dictionary mapping member names to delegate functions that perform the underlying method calls.
    //private readonly Dictionary<string, Func<Script, MyCustomClass, DynValue[], DynValue>> _members;

    public SimpleReferenceDescriptor()
    {

    }

    /// <summary>
    /// Provides access to properties and methods.
    /// </summary>
    /// <param name="script">The MoonSharp script context.</param>
    /// <param name="obj">The instance of MyCustomClass.</param>
    /// <param name="index">The key/index being accessed.</param>
    /// <param name="isNameIndex">True if the index is a named member.</param>
    /// <returns>A DynValue representing the property or a callable function.</returns>
    public DynValue Index( Script script, object obj, DynValue index, bool isNameIndex )
    {
        /*
        // We only support name-based indexing (i.e. table field access).
        if( !isNameIndex || index.Type != DataType.String )
            return DynValue.Nil;

        string memberName = index.String;

        // Directly expose the "Message" property.
        if( memberName.Equals( "Message", StringComparison.OrdinalIgnoreCase ) )
        {
            return DynValue.NewString( ( (MyCustomClass) obj ).Message );
        }

        // If it's one of our defined methods, return a callback.
        if( _members.TryGetValue( memberName, out var method ) )
        {
            // Create a closure that captures the object reference.
            return DynValue.NewCallback( ( context, args ) =>
            {
                return method( context.GetScript(), (MyCustomClass) obj, args );
            } );
        }
        */

        // If the member isn't found, return Nil.
        return DynValue.Nil;
    }

    /// <summary>
    /// Allows Lua to assign values to object members. For this example, we don't support setting.
    /// </summary>
    public bool SetIndex( Script script, object obj, DynValue index, DynValue value, bool isNameIndex )
    {
        // Not allowing setting any members; return false.
        return false;
    }

    /// <summary>
    /// Provides access to meta-members, like __tostring, if desired.
    /// </summary>
    public DynValue MetaIndex( Script script, object obj, string metaname )
    {
        // No meta-members are implemented in this example.
        return DynValue.Nil;
    }

    /// <summary>
    /// Converts the object to a string.
    /// </summary>
    public string AsString( object obj )
    {
        return obj.ToString();
    }

    /// <summary>
    /// Checks if the descriptor considers the provided type compatible.
    /// </summary>
    public bool IsTypeCompatible( Type type )
    {
        if( type == null )
            throw new ArgumentNullException( nameof( type ) );

        // Check if the provided type can be assigned a value of the type this descriptor represents.
        return type.IsAssignableFrom( this.Type );
    }

    public bool IsTypeCompatible( Type type, object obj )
    {
        // If obj is null, we need to decide whether null is acceptable
        // Here, we consider value types (non-nullable) as incompatible with null.
        if( obj == null )
            return !type.IsValueType || Nullable.GetUnderlyingType( type ) != null;

        // For non-null obj, check if it's an instance of the given type.
        return type.IsInstanceOfType( obj );
    }


    // Expose the names of the members (for introspection, etc.).
    public IEnumerable<string> MemberNames
    {
        get { return new List<string>(); }
    }

    // In this example, no meta index names are provided.
    public IEnumerable<string> MetaIndexNames
    {
        get { return new List<string>(); }
    }
}