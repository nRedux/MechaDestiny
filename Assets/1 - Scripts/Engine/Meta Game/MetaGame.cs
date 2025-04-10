using System.Linq;
using MoonSharp.Interpreter;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Tables;

public class MetaGame
{
    
    public static void ShowDialogBox( string title, string content, DynValue onContinue )
    {
        var callback = onContinue.Function.GetDelegate<System.Action>();
        SUIManager.Instance.UISystems.DialogBox.OnContinue = () => { callback.Invoke(); };
        SUIManager.Instance.UISystems.DialogBox.Show( null, title, content );
    }

    public static void ShowDialogBox( ActorReference actor, string title, string content, DynValue onContinue )
    {
        var asset = actor.GetAssetSync();
        
        var callback = onContinue.Function.GetDelegate<System.Action>();
        SUIManager.Instance.UISystems.DialogBox.OnContinue = () => { callback.Invoke(); };
        SUIManager.Instance.UISystems.DialogBox.Show( asset.PortraitImage, title, content );
    }

    public static void ShowDialogBox( Sprite speakerSprite, string title, string content, DynValue onContinue )
    {
        var callback = onContinue.Function.GetDelegate<System.Action>();
        SUIManager.Instance.UISystems.DialogBox.OnContinue = () => { callback.Invoke(); };
        SUIManager.Instance.UISystems.DialogBox.Show( speakerSprite, title, content );
    }

    public static void ChangeScene( string scene, bool warmupScene, DynValue onContinue )
    {
        var callback = onContinue.Function.GetDelegate<System.Action>();
        SceneLoadManager.LoadScene( scene, true, warmupScene, () => { callback.Invoke(); }, null );
    }

    public static void SetCombatEnemies( ActorListAsset actorList )
    {
        var actors = actorList.GetActors();
        var actorsData = actors.Select( x =>
        {
            if( x == null )
                return null;
            if( !x.RuntimeKeyIsValid() )
                return null;
            return x.GetDataCopySync();
        } ).NonNull().ToList();

        RunManager.Instance.RunData.CombatEnemies = actorsData;
    }

    public static void SetCombatMoneyReward( int amount )
    {
        RunManager.Instance.RunData.CombatMoneyReward = amount;
    }

}
