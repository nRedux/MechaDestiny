using System.Linq;
using UnityEngine;

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    static T _instance = null;
    public static T Instance
    {
        get
        {
            bool init = false;
            if( !_instance )
            {
                _instance = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
                init = true;
            }
            if( _instance == null )
            {
                string typeName = typeof( T ).Name;
                var allLoaded = Resources.LoadAll( "", typeof( T ) );
                var selected = allLoaded.FirstOrDefault();
                if( selected == null )
                {
                    Resources.UnloadUnusedAssets();
                    Debug.LogErrorFormat( $"{typeof(T).Name}" + "::Instance: No asset loadable for singleton of type {0}", typeName );
                    return null;
                }
                Debug.LogFormat( $"{typeof( T ).Name}" + "::Instance: {0} singleton loaded for type {1}", selected.name, typeName );
                _instance = selected as T;
                init = true;
                Resources.UnloadUnusedAssets();
            }
            if( init )
            {
                var initInst = _instance as SingletonScriptableObject<T>;
                initInst.Initialize();
            }
            return _instance;
        }
    }

    protected virtual void Initialize()
    {

    }

    public static void DestroyInstance()
    {
        _instance = null;
    }
}
