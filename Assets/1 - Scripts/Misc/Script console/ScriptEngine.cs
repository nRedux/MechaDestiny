using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Reflection;

using MoonSharp.Interpreter;
using System.IO;

public class ScriptConsoleMethod : Attribute
{
    public string HelpText
    {
        get; private set;
    }

    public Type CallSignature
    {
        get; private set;
    }

    public ScriptConsoleMethod( Type callSignature, string helpText = null )
    {
        this.CallSignature = callSignature;

        if( !string.IsNullOrEmpty( helpText ) )
            this.HelpText = helpText;
        else
            this.HelpText = "No description available";
    }
}

public class ScriptEngine : Singleton<ScriptEngine>
{

    public static bool ReplicateNextCommand
    {
        get; set;
    }

    public static class PlayerTools
    {
        public static System.Action<string> ToConsole;
        public static List<ScriptMethod> ConsoleMethods = null;

        [ScriptConsoleMethod( typeof( System.Action ) )]
        public static void Exit()
        {
            //Bootstrap.Instance.NetManager.LeaveMatch();
        }

        [ScriptConsoleMethod( typeof( System.Action ) )]
        public static void SimDisconnect()
        {
            //PhotonNetwork.Disconnect();
        }

        [ScriptConsoleMethod( typeof( System.Action<int, int> ) )]
        public static void SetHullPoints( int player, int value )
        {
            //throw new ScriptRuntimeException( "NEEDS TO BE REFACTORED." );
            //The following is the old code
            /*
            if (value < 0){
				throw new ScriptRuntimeException("Hullpoint value cannot be negative.");
			}

			SetStatisticCommand modHull = new SetStatisticCommand(StatisticType.HullPoints, CommandPhase.Action, (sbyte)value);

			Player p = GameManager.Instance.GetPlayers()[player];
			CommandResult res = null;
			GameManager.Instance.masterGameState.ExecuteCommandOnPlayer(modHull, p, 0);
            */
        }

        [ScriptConsoleMethod( typeof( System.Action<int, int> ) )]
        public static void SetShieldPoints( int player, int value )
        {
            //throw new ScriptRuntimeException( "NEEDS TO BE REFACTORED." );
            //The following is the old code

            /*
            if (value < 0)
			{
				throw new ScriptRuntimeException("Hullpoint value cannot be negative.");
			}

			SetStatisticCommand modShields = new SetStatisticCommand(StatisticType.ShieldPoints, CommandPhase.Action, (sbyte)value);

			Player p = GameManager.Instance.GetPlayers()[player];
			CommandResult res = null;
			GameManager.Instance.masterGameState.ExecuteCommandOnPlayer(modShields, p, 0);
            */
        }


        const string DEF_SAVE_KEY = "STATE_SAVE_DEF_PATH";
        const string DEF_LOAD_KEY = "STATE_LOAD_DEF_PATH";
        [ScriptConsoleMethod( typeof( System.Action ), "Save the game to a file to be reloaded later." )]
        public static void SaveGameState()
        {
            /*
            string defaultPath = PlayerPrefs.GetString( DEF_SAVE_KEY, "" );
            if( string.IsNullOrEmpty( defaultPath ) )
            {
                defaultPath = PlayerPrefs.GetString( DEF_LOAD_KEY, "" );
            }

            string initialName = StateToolUtility.Instance.DefaultSaveName;
            string path = StandaloneFileBrowser.SaveFilePanel( "Save Game State", defaultPath, initialName, "ssf" );
            string basePath = path.Replace( Path.GetFileName( path ), "" );
            PlayerPrefs.SetString( "STATE_SAVE_DEF_PATH", basePath );
            StateToolUtility.Instance.SaveState( path );
            */
        }

        [ScriptConsoleMethod( typeof( System.Action ), "Reload the state file whos path is specified in the StateUtilityTool -> stateReloadFilePath" )]
        public static void LoadSavedState()
        {
            //StateToolUtility.Instance.LoadData();
        }

        [ScriptConsoleMethod( typeof( System.Action<int> ), "Load a specific state in the loaded game state. (See LoadSavedState())" )]
        public static void ReloadState( int stateID )
        {
            //StateToolUtility.Instance.LoadState( stateID );
        }

        [ScriptConsoleMethod( typeof( System.Action<string, int> ) )]
        public static void PlayCard( string cardname, int playerIndex )
        {
            //var player = GameManager.Instance.GetPlayers()[playerIndex];
            //RuntimeCardManager.Instance.GetCardAsset( cardname );

        }

        [ScriptConsoleMethod( typeof( System.Action<int> ) )]
        public static void CompareState( int stateEventID )
        {
            //StateToolUtility.Instance.DoExternalCompare( stateEventID );
        }

        [ScriptConsoleMethod( typeof( System.Action<int> ) )]
        public static void ViewState( int state )
        {
            //StateToolUtility.Instance.ViewState( state );
        }

        [ScriptConsoleMethod( typeof( System.Action ) )]
        public static void GetOtherClientState()
        {
            //Bootstrap.Instance.NetManager.DoRequestGameState();
        }


        [ScriptConsoleMethod( typeof( System.Action ) )]
        public static void Help()
        {
            Debug.LogFormat( "Printing help on {0} entries", ConsoleMethods.Count );
            foreach( var method in ConsoleMethods )
            {
                var parameterInfos = method.method.GetParameters();
                StringBuilder sb = new StringBuilder();
                sb.Append( "(" );
                for( int i = 0; i < parameterInfos.Length; ++i )
                {
                    var p = parameterInfos[i];
                    if( p.IsOptional )
                        sb.Append( "[" );

                    if( i != 0 )
                        sb.Append( " ," );
                    sb.Append( string.Format( "{0} {1}", p.ParameterType.Name, p.Name ) );

                    if( p.IsOptional )
                        sb.Append( "]" );
                }
                sb.Append( ")" );
                string message = string.Format( "{0}{2} - {1}\n", method.method.Name, method.attr.HelpText, sb.ToString() );
                ToConsole( message );
            }
        }

    }

    public enum ScriptErrorType
    {
        Internal,
        Sytax,
        RuntimeError
    }

    public string GetMethodName( MethodInfo methodInfo )
    {
        return methodInfo.DeclaringType.FullName + "." + methodInfo.Name;
    }

    public class ScriptError
    {
        public ScriptErrorType Type { get; private set; }
        public string Error { get; private set; }

        public ScriptError( ScriptErrorType type, string error )
        {
            Type = type;
            Error = error;
        }
    }

    Script script;
    List<ScriptMethod> methodList = new List<ScriptMethod>();

    public class ScriptMethod
    {
        public MethodInfo method;
        public ScriptConsoleMethod attr;
    }

    protected override void Awake()
    {
        base.Awake();
        script = new Script();
        var methods = typeof( PlayerTools ).GetMethods( BindingFlags.Static | BindingFlags.Public );

        foreach( var m in methods )
        {
            object[] attrs = m.GetCustomAttributes( typeof( ScriptConsoleMethod ), false );

            if( attrs.Length == 0 || attrs[0] == null )
                continue;

            var attr = attrs[0] as ScriptConsoleMethod;
            if( attr == null )
                continue;
            methodList.Add( new ScriptMethod { attr = attr, method = m } );

            var del = Delegate.CreateDelegate( attr.CallSignature, m );
            script.Globals[m.Name] = del;
        }

        //Alphabetic sort
        methodList.Sort( ( a, b ) => { return a.method.Name.CompareTo( b.method.Name ); } );
        PlayerTools.ConsoleMethods = methodList;

    }

    // Update is called once per frame
    public void Execute( string code, System.Action<string> onSuccess, System.Action<ScriptError> onError )
    {
        try
        {
            var res = script.DoString( code );
            if( res.IsNil() )
            {
                if( onSuccess != null )
                    onSuccess( "Executed successfuly" );
            }
            else
            {
                if( !string.IsNullOrEmpty( res.String ) )
                {
                    onSuccess( res.String );
                }
            }
        }
        catch( InternalErrorException ex )
        {
            if( onError != null )
            {
                onError( new ScriptError( ScriptErrorType.Internal, ex.Message ) );
            }
        }
        catch( SyntaxErrorException ex )
        {
            if( onError != null )
            {
                onError( new ScriptError( ScriptErrorType.Sytax, ex.Message ) );
            }
        }
        catch( ScriptRuntimeException ex )
        {
            if( onError != null )
            {
                onError( new ScriptError( ScriptErrorType.RuntimeError, ex.Message ) );
            }
        }
        finally
        {
            ReplicateNextCommand = false;
        }
    }
}
