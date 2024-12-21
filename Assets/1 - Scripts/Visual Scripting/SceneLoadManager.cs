using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoadManager
{

    public static void LoadScene( string scene, bool fadeScreen, bool doSceneWarmup, System.Action onFadeComplete, System.Action onLoadComplete )
    {
        
        Debug.Log( $"SceneLoadManager::LoadScene: Loading scene {scene}. fadeScreen: {fadeScreen}" );
        var suiManager = SUIManager.Instance;
        if( suiManager != null )
        {
            fadeScreen = suiManager.Fader != null;
        }
       
        CoroutineUtils.BeginCoroutine( PerformSceneLoad( scene, onFadeComplete, () =>
        {
            RunManager.Instance.SetScene( scene, doSceneWarmup );
            onLoadComplete?.Invoke();
        }, 
        fadeScreen ) );
    }

    private static IEnumerator PerformSceneLoad(string scene, System.Action onFadeComplete, System.Action onLoadComplete, bool fadeScreen = true)
    {
        object waitObject = null;
        System.Action onFadeFinished = () => {
            waitObject = null;
        };
        if( fadeScreen )
        {
            var suiManager = SUIManager.Instance;
            if( suiManager != null )
            {
                suiManager.Fader.Show(1f, onFadeFinished );
            }
        }
        if (fadeScreen)
        {
            waitObject = new object();
            yield return new WaitWhile(() => { return waitObject != null; });
        }

        var loadAsync = SceneManager.LoadSceneAsync(scene);

        bool finished = false;
        loadAsync.completed += (a) => { 
            finished = true;
            //Didn't used to fire. Was the Coroutine itself ( CoroutineUtils.DelayUnityFrameEnd ) not the coroutine runner CoroutineUtils.DoDelayUnityFrameEnd
            CoroutineUtils.DoDelayUnityFrameEnd( () => {
                onLoadComplete?.Invoke();
                onLoadComplete = null;
            }, 2 );
        };
        while (!finished )
        {
            yield return null;
        }
        Debug.Log( "SceneLoadManager: Scene load complete" );

        waitObject = new object();
        if( fadeScreen )
        {
            var suiManager = SUIManager.Instance;
            if( suiManager != null )
            {
                suiManager.Fader.Hide( 1f, onFadeFinished );
            }
        }
        if( fadeScreen)
            yield return new WaitWhile(() => { return waitObject != null; });
        onLoadComplete?.Invoke();

        if( fadeScreen )
            onFadeComplete?.Invoke();
    }
}
