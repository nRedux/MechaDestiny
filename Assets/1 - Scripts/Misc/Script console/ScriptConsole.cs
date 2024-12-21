using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;

namespace Edgeflow.UI
{
    public class UIScriptConsole : MonoBehaviour
    {
        public enum ContentType
        {
            Normal,
            Success,
            Warning,
            Error
        }

        [SerializeField]
        private Text output = null;
        [SerializeField]
        private InputField inputField = null;

        private ScriptConsole consoleLogic = new ScriptConsole();
        private StringBuilder stringBuilder = new StringBuilder();
        private bool skipNextInputChange = false;

        private void Awake()
        {
            ScriptEngine.PlayerTools.ToConsole = s => { AddResult( s ); };

            /*
            consoleLogic.AddHistory("poop");
            consoleLogic.AddHistory("poop1");
            consoleLogic.AddHistory("poop2");
            consoleLogic.AddHistory("poop3");
            consoleLogic.AddHistory("poop4");
            */

            //SetupHistoryListener();

        }

        public void SetupEventListener()
        {
            //Events.Instance.AddListener<ConsoleMessageEvent>( OnConsoleMessageEvent );
        }
        /*
        private void OnConsoleMessageEvent( ConsoleMessageEvent e )
        {
            //Debug.Log("UIScriptConsole is Responding to ConsoleMessageEvent");
            AddResult( e.message, e.severity, e.forwardToUnityConsole );

            if( e.showConsole )
                gameObject.SetActive( true );
        }

        private void SetupHistoryListener()
        {
            inputField.onValueChanged.AddListener( OnInputChange );
            var inputHistory = inputField.GetComponent<UIInputHistory>();
            if( inputHistory == null )
                return;
            inputHistory.OnNextHistoryInput += OnNextHistoryReq;
            inputHistory.OnPrevHistoryInput += OnPrevHistoryReq;
        }
        */
        private void OnInputChange( string change )
        {
            if( skipNextInputChange )
            {
                skipNextInputChange = false;
                return;
            }
            //consoleLogic.SetLatestUserText(change);
        }

        private void OnEnable()
        {
            inputField.Select();
        }

        private void OnNextHistoryReq()
        {
            skipNextInputChange = true;
            inputField.text = consoleLogic.NextHistory();
        }

        private void OnPrevHistoryReq()
        {
            skipNextInputChange = true;
            inputField.text = consoleLogic.PreviousHistory();
        }

        public void ExecuteInput( string input )
        {
            consoleLogic.ResetHistory();
            Debug.Log( "Submit received: " + input );
            ScriptEngine.ReplicateNextCommand = true;
            ScriptEngine.Instance.Execute( input, OnExecuteSuccess, OnExecuteError );
        }

        public void OnSubmitButtonClick()
        {
            consoleLogic.AddHistory( inputField.text );
            AddResult( inputField.text );

            ExecuteInput( inputField.text );
            inputField.text = string.Empty;
        }

        private void OnExecuteSuccess( string result )
        {
            AddResult( result, ContentType.Success );
        }

        private void OnExecuteError( ScriptEngine.ScriptError error )
        {
            AddResult( error.Error, ContentType.Error );
        }

        public void AddResult( string input, ContentType type = ContentType.Normal, bool forwardToUnityConsole = false )
        {
            /*
            switch (type)
            {
                case ContentType.Normal:
                    stringBuilder.Append(input + "\n");
                    if (forwardToUnityConsole)
                        Debug.Log(input);
                    break;
                case ContentType.Warning:
                    stringBuilder.Append("<color=orange>" + input + "</color>\n");
                    if (forwardToUnityConsole)
                        Debug.LogWarning(input);
                    break;
                case ContentType.Error:
                    stringBuilder.Append("<color=red>" + input + "</color>\n");
                    if (forwardToUnityConsole)
                        Debug.LogError(input);
                    break;
                case ContentType.Success:
                    consoleLogic.SetLatestUserText(input);
                    stringBuilder.Append("<color=green>" + input + "</color>\n");
                    if (forwardToUnityConsole)
                        Debug.Log(input);
                    break;
            }

            output.text = stringBuilder.ToString();
            */
        }
    }

    public class ScriptConsole
    {
        private List<string> history = new List<string>();
        private int historyLocation = 0;
        private string latestModified = string.Empty;

        public void AddHistory( string entry )
        {
            latestModified = string.Empty;
            this.history.Add( entry );
        }

        public void ResetHistory()
        {
            historyLocation = -1;
        }

        public string NextHistory()
        {
            historyLocation += 1;
            int upperBound = Mathf.Max( history.Count - 1, 0 );
            historyLocation = Mathf.Clamp( historyLocation, 0, upperBound );
            if( historyLocation >= history.Count )
                return string.Empty;
            return history[historyLocation];
        }

        public string PreviousHistory()
        {
            if( historyLocation <= 0 )
            {
                return latestModified;
            }

            historyLocation -= 1;

            if( historyLocation >= history.Count )
                return string.Empty;
            return history[historyLocation];
        }

        public void SetLatestUserText( string latest )
        {
            latestModified = latest;
        }
    }
}