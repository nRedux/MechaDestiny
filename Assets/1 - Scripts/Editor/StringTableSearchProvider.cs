using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Linq;

public class StringTableSearchProvider : ScriptableObject, ISearchWindowProvider
{
    private List<StringTableCollection> _items;
    private bool _tablesOnly = false;
    public StringTableSearchProvider(List<StringTableCollection> items, bool tablesOnly, System.Action<object> onSetIndexCallback )
    {
        _items = items;
        _onSetIndexCallback = onSetIndexCallback;
        _tablesOnly = tablesOnly;
    }

    private System.Action<object> _onSetIndexCallback;

    public List<SearchTreeEntry> CreateSearchTree( SearchWindowContext context )
    {
        List<SearchTreeEntry> searchList = new List<SearchTreeEntry>();

        var entry = new SearchTreeGroupEntry( new GUIContent( "Tables" ), 0 );
        searchList.Add( entry );
        List<string> groups = new List<string>();
        for( int i = 0; i < _items.Count; ++i )
        {

            if( _tablesOnly )
            {
                var item = new SearchTreeEntry( new GUIContent( _items[i].name ) );
                item.level = 1;
                item.userData = _items[i].name;
                searchList.Add( item );
            }
            else
            {
                entry = new SearchTreeGroupEntry( new GUIContent( _items[i].name ), 1 );
                searchList.Add( entry );
                var rowIterator = _items[i].GetRowEnumerator();

                foreach( var row in rowIterator )
                {
                    string itemName = row.KeyEntry.Key;
                    var item = new SearchTreeEntry( new GUIContent( itemName.ToString() ) );
                    item.level = 2;
                    item.userData = new Tuple<string, string>( row.KeyEntry.Key, _items[i].name );
                    searchList.Add( item );
                }
            }

        }
        
        return searchList;
    }

    public bool OnSelectEntry( SearchTreeEntry SearchTreeEntry, SearchWindowContext context )
    {
        //Tuple<string, string> data = (Tuple<string, string>) SearchTreeEntry.userData;
        _onSetIndexCallback?.Invoke( SearchTreeEntry.userData );
        return true;
    }
}
