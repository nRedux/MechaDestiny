using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu( menuName = "Engine/Create/SUI Manager" )]
public class SUIManager : SingletonScriptableObject<SUIManager>
{

    //public EncounterState EncounterState;

    public UISystems UIPrefab;



    public UISystems UISystems { get; private set; }

    public UIScreenFader Fader { 
        get
        {
            if( UISystems == null )
                return null;
            return UISystems.Fader;
        }
    }

    protected override void Initialize()
    {
        if( !Application.isPlaying )
            return;
        InstantiateUI();
    }


    private void InstantiateUI()
    {
        if( UIPrefab == null )
            return;
        UISystems = Instantiate<UISystems>( UIPrefab );
        var uiSystemCanvas = UISystems.gameObject.GetComponent<Canvas>();
        uiSystemCanvas.sortingOrder = 1000;

        DontDestroyOnLoad( UISystems.gameObject );
    }


    /// <summary>
    /// Will be called from visual script graphs. Don't believe reference counter. This method shows a dialog box to present narrative info to the player.
    /// </summary>
    /// <param name="title">The title of the speaker.</param>
    /// <param name="text">The spoken text.</param>
    public static void ShowDialog(string title, string text)
    {
        Instance.UISystems.DialogBox.Show( null, title, text);
    }

}
