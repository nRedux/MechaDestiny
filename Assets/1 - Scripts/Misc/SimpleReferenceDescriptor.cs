using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter;
using System.Collections.Generic;
using System;
using UnityEngine;


/// <summary>
/// A moonsharp type descriptor usable if we want to expose a type but none of it's public API
/// </summary>
/// <typeparam name="T">The type we wish to expose to moonsharp</typeparam>
public class SimpleReferenceDescriptor<T> : IUserDataDescriptor
{
    // A friendly name by which Lua scripts can refer to the type.
    public string Name => nameof( T );

    // The actual type that this descriptor wraps.
    public Type Type => typeof( T );

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
        //We never expose anything from these types for LUA scripts to access, always return nil
        return DynValue.Nil;
    }

    /// <summary>
    /// Allows Lua to assign values to object members. For this example, we don't support setting.
    /// </summary>
    public bool SetIndex( Script script, object obj, DynValue index, DynValue value, bool isNameIndex )
    {
        // Not allowing setting any members; always return false.
        return false;
    }

    /// <summary>
    /// Provides access to meta-members, like __tostring, if desired.
    /// </summary>
    public DynValue MetaIndex( Script script, object obj, string metaname )
    {
        // Never defined
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
        //Always an empty list
        get { return new List<string>(); }
    }

    // In this example, no meta index names are provided.
    public IEnumerable<string> MetaIndexNames
    {
        //Always empty list
        get { return new List<string>(); }
    }
}