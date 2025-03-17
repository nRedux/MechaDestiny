using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnvironmentStableEvent 
{

}

public class Bootstrapper : Singleton<Bootstrapper>
{

    /*
     * I have to trust myself. This class was built to guarantee consistency in execution both in the editor and in builds.
     * Bootstrapper loading all essential classes and guaranteeing everything we rely on for execution in any state has been
     * completed is essential to being able to freely test.
     * 
     * Things we are loading in those classes have an unknown amount of time before completion.  This class will have to be able
     * to guarantee things are complete.
     */

    //Critial execution stages.
    //Loading user data
        //Starts loading as soon as it can.

    //This class should require that certain functions be completed by bootstrapper and wait for them before it signals standard execution of 
    //engine logic should proceed.


    protected override void Awake()
    {
        //We load out bootstrap scene which contains all important structural objects we need
    }

    private const string BOOTSTRAP_SCENE = "Bootstrap";

    private void LoadBootstrap()
    {
        var op = SceneManager.LoadSceneAsync( BOOTSTRAP_SCENE );
        op.completed += BootstrapLoaded;
    }

    private void BootstrapLoaded( AsyncOperation op )
    {

    }

}
