using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static System.Func<T> DoInstantiate;

    public static T Instance {
        get
        {
            if( _instance == null)
            {
                if (DoInstantiate != null)
                {
                    DoInstantiate();
                }
                else
                {
                    //Debug.LogFormat("Creating default singleton setup for type: {0}", typeof(T).Name); 
                    GameObject singletonGO = new GameObject(typeof(T).Name + "_SINGLETON");
                    singletonGO.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    private static T _instance;

    public static bool InstanceExists
    {
        get
        {
            return _instance != null;
        }
    }

    public static T ExistingInstance
    {
        get
        {
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            //Destroyed?
            /*
            if( _instance == null )
            {
                _instance = this as T;
                return;
            }  
            */
            
            if( _instance != this )
                Destroy( gameObject );
            return;
        }

        _instance = this as T;
    }

	protected virtual void OnDestroy()
	{
        if( _instance == this )
		    _instance = null;
	}
}