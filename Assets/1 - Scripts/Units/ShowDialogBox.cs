using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;


public class ShowDialogBox : Unit
{
    [DoNotSerialize]
    public ControlInput Enter;

    [DoNotSerialize]
    public ControlOutput Exit;

    [DoNotSerialize]
    public ValueInput Title;

    [DoNotSerialize]
    public ValueInput Content;

    private GraphReference _graphRef;

    protected override void Definition()
    {

        Title = ValueInput<string>( nameof( Title ), string.Empty );
        Content = ValueInput<string>( nameof( Content ), string.Empty );

        Enter = ControlInput( nameof( Enter ), ( flow ) =>
        {
            _graphRef = flow.stack.AsReference();

            string stringTitle = flow.GetValue<string>( Title );
            string stringContent = flow.GetValue<string>( Content );

            SUIManager.Instance.UISystems.DialogBox.OnContinue += OnContinue;
            SUIManager.Instance.UISystems.DialogBox.Show( stringTitle, stringContent );
            return null;
        } );

        Exit = ControlOutput( nameof( Exit ) );
    }


    private void OnContinue()
    {
        Flow.New( _graphRef ).Invoke( Exit );
    }

}
