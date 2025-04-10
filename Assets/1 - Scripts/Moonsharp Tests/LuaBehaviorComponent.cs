using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using MoonSharp.Interpreter;
using UnityEditor;
using UnityEngine.AddressableAssets;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEngine.Localization;


[AttributeUsage(AttributeTargets.Class)]
public class LuaFieldType: Attribute
{
    public Type Type;
    public LuaFieldType( Type targetType )
    {
        this.Type = targetType;
    }
}

public interface ILuaField
{
    string GetName();
    void SetName( string name );
    object GetValue();
    string GetFieldTypeName();
}

[InlineProperty]
[Serializable]
public abstract class TypedLuaField<T>: ILuaField
{
    [ReadOnly]
    public string Name;

    public T Value;

    public string GetName()
    {
        return Name;
    }

    public string GetFieldTypeName()
    {
        return typeof( T ).Name;
    }

    public void SetName( string name )
    {
        Name = name;
    }

    public object GetValue()
    {
        return Value;
    }
}


[LuaFieldType( typeof(GameObject) )]
[Serializable]
public class LuaGameObject: TypedLuaField<GameObject>
{
}

[LuaFieldType( typeof( int ) )]
[Serializable]
public class LuaInt : TypedLuaField<int>
{
}

[LuaFieldType( typeof( float ) )]
[Serializable]
public class LuaFloat : TypedLuaField<float>
{
}

[LuaFieldType( typeof( string ) )]
[Serializable]
public class LuaSting : TypedLuaField<string>
{
}

[LuaFieldType( typeof( bool ) )]
[Serializable]
public class LuaBool : TypedLuaField<bool>
{
}

[LuaFieldType( typeof( ActorListAsset ) )]
[Serializable]
public class LuaActorListAsset : TypedLuaField<ActorListAsset>
{
}

[LuaFieldType( typeof( ActorListCollection ) )]
[Serializable]
public class LuaActorListAssetCollection : TypedLuaField<ActorListCollection>
{
}

[LuaFieldType( typeof( ActorReference[] ) )]
[Serializable]
public class LuaActorList : TypedLuaField<ActorReference[]>
{
}

[LuaFieldType( typeof( ActorReference ) )]
[Serializable]
public class LuaActorRef : TypedLuaField<ActorReference>
{
}

[LuaFieldType( typeof( Sprite ) )]
[Serializable]
public class LuaSprite : TypedLuaField<Sprite> { }


[System.Serializable]
public class DialogContent
{
    public LocalizedString Title;
    public LocalizedString Message;
}


[LuaFieldType( typeof( DialogContent ) )]
[Serializable]
public class LuaDialogContent: TypedLuaField<DialogContent>
{
}

public class SpriteDescriptor : SimpleReferenceDescriptor<Sprite> { }


public class LuaBehaviorComponent : MonoBehaviour
{
    private const string EXPOSED_FIELDS_SIGNATURE = "__exposed__";

    [HideReferenceObjectPicker]
    [ListDrawerSettings( HideAddButton = true, HideRemoveButton = true )]
    [SerializeReference]
    public List<ILuaField> LuaFields;

    [OnValueChanged(nameof( OnScriptAssetChanged ), InvokeOnUndoRedo = false )]
    public TextAsset ScriptAsset;

    [HideInInspector]
    [SerializeField]
    private TextAsset _oldScriptAsset;

    private LuaBehavior _luaBehavior;

    public void OnScriptAssetChanged( TextAsset changedTextAsset )
    {
        if( changedTextAsset == null )
        {
#if UNITY_EDITOR
            Undo.RecordObject( this, "Script asset cleared." );
            LuaFields = new List<ILuaField>();
            EditorUtility.SetDirty( this );
            EditorUtility.SetDirty( gameObject );
            Undo.FlushUndoRecordObjects();
#endif
            return;
        }
        else if( _oldScriptAsset != changedTextAsset )
        {
            AutoGenParams();
        }


        if( changedTextAsset != _oldScriptAsset )
            _oldScriptAsset = changedTextAsset;

    }

    [Button]
    public void AutoGenParams()
    {
        LuaBehaviorManager.SetupLuaEnv();

        if( ScriptAsset == null )
            return;
        Script script = new Script();
        script.DoString( ScriptAsset.text );


        //Find fields in LuaFields which don't exist in 


        var updatedLuaFields = new List<ILuaField>() { };
        var table = script.Globals.Get( "__exposed__" );

        if( !table.IsNil() )
        {
            foreach( var item in table.Table.Pairs )
            {
                Type desiredType = GetType( item.Value.String );
                if( desiredType == null )
                {
                    Debug.LogError($"Invalid requested type {item.Value.String}");
                    continue;
                }

                Type desiredFieldType  = FindTypesWithLuaFieldType( desiredType );
                if( desiredFieldType == null )
                {
                    Debug.LogError( $"Unsupported LuaField type {desiredType.Name}" );
                    continue;
                }

                var inst = Activator.CreateInstance( desiredFieldType ) as ILuaField;
                inst.SetName( item.Key.String );
                updatedLuaFields.Add( inst  );
            }
        }

        //Strip anything modified from existing lua fields
        var fieldsRetained = LuaFields.Where( a => updatedLuaFields.Any( x => x.GetName() == a.GetName() && x.GetFieldTypeName() == a.GetFieldTypeName() ) ).ToList();
        //Find anything new which doesn't exist in retained fields
        var newFields = updatedLuaFields.Where( a => !fieldsRetained.Any( x => x.GetName() == a.GetName() && x.GetFieldTypeName() == a.GetFieldTypeName() ) ).ToList();

        fieldsRetained.AddRange( newFields );

        LuaFields = fieldsRetained;

#if UNITY_EDITOR
        EditorUtility.SetDirty( this );
        EditorUtility.SetDirty( gameObject );
#endif
    }

    [Button]
    public void Execute()
    {
        if( !Application.isPlaying )
            return;
        _luaBehavior = new LuaBehavior( ScriptAsset, LuaFields );
    }

    public void Awake()
    {
        _luaBehavior = new LuaBehavior( ScriptAsset, LuaFields );
    }

    private static Dictionary<string, string> _map = new Dictionary<string, string>()
    {
        { "gameobject", typeof(GameObject).AssemblyQualifiedName },
        { "int", typeof(Int32).AssemblyQualifiedName },
        { "float", typeof(float).AssemblyQualifiedName },
        { "string", typeof(string).AssemblyQualifiedName },
        { "bool", typeof(bool).AssemblyQualifiedName },
        { "actorlistAsset", typeof(ActorListAsset).AssemblyQualifiedName },
        { nameof(ActorListCollection).ToLower(), typeof(ActorListCollection).AssemblyQualifiedName },
        { nameof(ActorListAsset).ToLower(), typeof(ActorListAsset).AssemblyQualifiedName },
        { "actorlist", typeof(ActorReference[]).AssemblyQualifiedName },
        { "actorref", typeof(ActorReference).AssemblyQualifiedName },
        { "sprite", typeof(Sprite).AssemblyQualifiedName },
        { "dialog", typeof(DialogContent).AssemblyQualifiedName }
    };

    private void InjectFields(Script script)
    {
        LuaFields.Do( x =>
        {
            script.Globals.Set( x.GetName(), DynValue.FromObject( script, x.GetValue() ) );
        } );
    }

    private static Type GetType(string typeName )
    {
        string typeToFind = typeName;

        string tryMapVal = null;
        if( _map.TryGetValue( typeName.ToLower(), out tryMapVal ) )
            typeToFind = tryMapVal;

        var qualifiedTypeCheck = Type.GetType( typeToFind );
        if( qualifiedTypeCheck != null )
            return qualifiedTypeCheck;

        // Iterate through all loaded assemblies
        foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
        {
            // Iterate through all types in the assembly
            foreach( var type in assembly.GetTypes() )
            {
                if( type.Name == typeToFind )
                {
                    return type;
                }
            }
        }
        return null;
    }

    private static Type FindTypesWithLuaFieldType( Type targetType )
    {
        var gameObjectType = targetType;

        // Iterate through all loaded assemblies
        foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
        {
            // Iterate through all types in the assembly
            foreach( var type in assembly.GetTypes() )
            {
                // Use Attribute.GetCustomAttribute to retrieve the specific attribute
                var attribute = (LuaFieldType) Attribute.GetCustomAttribute( type, typeof( LuaFieldType ) );

                if( attribute != null && attribute.Type == gameObjectType )
                {
                    return type;
                }
            }
        }

        return null;
    }
}
