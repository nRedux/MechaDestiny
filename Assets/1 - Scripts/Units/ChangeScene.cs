using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


public class ChangeScene : Unit
{
    [DoNotSerialize]
    public ControlInput Enter;

    [DoNotSerialize]
    public ControlOutput FadeComplete;

    [DoNotSerialize]
    public ValueInput Scene;

    [DoNotSerialize]
    public ValueInput WarmupScene;


    private GraphReference _graphRef;

    protected override void Definition()
    {

        Scene = ValueInput<string>( nameof( Scene ), string.Empty );
        WarmupScene = ValueInput<bool>( nameof( WarmupScene ), false );
        FadeComplete = ControlOutput( nameof( FadeComplete ) );
        Enter = ControlInput( nameof( Enter ), ( flow ) =>
        {
            string sceneName = flow.GetValue<string>( Scene );
            bool doWarmup = flow.GetValue<bool>( WarmupScene );
            _graphRef = flow.stack.AsReference();

            SceneLoadManager.LoadScene( sceneName, true, doWarmup, TransitionDone, null );
                return null;
        } );

    }


    private void TransitionDone()
    {
        Flow.New( _graphRef ).Invoke( FadeComplete );
    }

}
