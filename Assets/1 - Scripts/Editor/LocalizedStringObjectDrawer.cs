using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Linq;

[ CustomPropertyDrawer( typeof( LocalizedStringObject ) ) ]
public class LocalizedStringObjectDrawer : PropertyDrawer
{
    private StringTableCollection _table = null;
    private StringTableEntry _localizationEntry = null;
    private string newKey = null;
    private string newKeyTable = null;

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        EditorStyles.textArea.wordWrap = true;
        EditorGUI.BeginProperty( position, label, property );

        var key = property.FindPropertyRelative( "Key" );
        var table = property.FindPropertyRelative( "Table" );
        var content = property.FindPropertyRelative( "Content" );
        var id = property.FindPropertyRelative( "Id" );

        var tables = LocalizationEditorSettings.GetStringTableCollections().ToList();

        position.height = EditorGUIUtility.singleLineHeight;
        
        if( EditorGUI.DropdownButton( position, new GUIContent( key.stringValue ), FocusType.Keyboard, EditorStyles.popup ) )
        {
            SearchWindow.Open( new SearchWindowContext( GUIUtility.GUIToScreenPoint( Event.current.mousePosition ) ), new StringTableSearchProvider( tables, false, (x) =>
            {
                Tuple<string, string> data = (Tuple<string, string>) x;
                newKey = data.Item1;
                newKeyTable = data.Item2;
            } ) );
        }


        //If a thing has been clicked in the search window.
        if( newKey != null && newKey != key.stringValue )
        {
            var locEntry = LocateTableEntry( newKey );
            key = property.FindPropertyRelative( "Key" );
            key.stringValue = newKey;
            table.stringValue = newKeyTable;
            id.longValue = locEntry.KeyId;
            newKey = null;
            newKeyTable = null;
            if( key.serializedObject.ApplyModifiedProperties() )
            {
                LocateTableEntryFromID( id, content, false );
                property.serializedObject.Update();
                GUI.changed = true;
            }
        }

        LocateTableEntryFromID( id, content, true );

        if( _localizationEntry != null && _localizationEntry.KeyId != id.longValue )
        {
            id.longValue = _localizationEntry.KeyId;
            id.serializedObject.ApplyModifiedProperties();
        }
        if( _localizationEntry != null && _localizationEntry.Key != key.stringValue )
        {
            key.stringValue = _localizationEntry.Key;
            key.serializedObject.ApplyModifiedProperties();
        }
        
        if( _localizationEntry == null )
        {
            EditorGUI.EndProperty();
            return;
        }
        var textAreaStyle = GUI.skin.textArea;
        float contentHeight = textAreaStyle.CalcHeight( new GUIContent( _localizationEntry.Value ), EditorGUIUtility.currentViewWidth ) + 5;
        var textRect = new Rect( position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight * 5 );

        EditorGUI.BeginChangeCheck();
        _localizationEntry.Value = EditorGUI.TextArea( textRect, _localizationEntry.Value, EditorStyles.textArea );
        if( EditorGUI.EndChangeCheck() )
        {
            EditorUtility.SetDirty( _table );
            var stringTable = _table.GetTable( new LocaleIdentifier( "en" ) );
            EditorUtility.SetDirty( stringTable );
        }

        EditorGUI.EndProperty();
    }

    private StringTableEntry LocateTableEntry(string keyValue)
    {
        var tables = LocalizationEditorSettings.GetStringTableCollections().ToList();
        var locale = LocalizationEditorSettings.GetLocale( "en" );
        var table = tables.Where( x => x.GetRowEnumerator().Where( y => y.KeyEntry.Key == keyValue ).FirstOrDefault() != null ).FirstOrDefault();
        if( table == null )
            return null;

        var enumerator = table.GetRowEnumerator();
        foreach( var e in enumerator )
        {
            if( e.KeyEntry.Key == keyValue )
            {
                for( int i = 0; i < e.TableEntriesReference.Length; ++i )
                {
                    if( e.TableEntriesReference[i].Code == "en" )
                    {
                        return e.TableEntries[i];
                    }
                }
                break;
            }
        }
        return null;
    }

    private void LocateTableEntryFromID( SerializedProperty Id, SerializedProperty content, bool refresh )
    {
        if( refresh && _localizationEntry != null )
            return;
        long idValue = Id.longValue;
        var tables = LocalizationEditorSettings.GetStringTableCollections().ToList();
        var locale = LocalizationEditorSettings.GetLocale( "en" );
        var table = tables.Where( x => x.GetRowEnumerator().Where( y => y.KeyEntry.Id == idValue ).FirstOrDefault() != null ).FirstOrDefault();
        if( table == null )
            return;

        var enumerator = table.GetRowEnumerator();
        foreach( var e in enumerator )
        {
            if( e.KeyEntry.Id == idValue )
            {
                for( int i = 0; i < e.TableEntriesReference.Length; ++i )
                {
                    if( e.TableEntriesReference[i].Code == "en" )
                    {
                        _localizationEntry = e.TableEntries[i];
                        _table = table;
                        break;
                    }
                }
                break;
            }
        }
    }

    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        var textAreaStyle = GUI.skin.textArea;
        string content = string.Empty;
        if( _localizationEntry != null )
        {
            content = _localizationEntry.Value;
        }
        float contentHeight = EditorGUIUtility.singleLineHeight * 2 + 5 + EditorGUIUtility.singleLineHeight * 5;// textAreaStyle.CalcHeight( new GUIContent( content ), EditorGUIUtility.currentViewWidth );
        return contentHeight;
    }
}
