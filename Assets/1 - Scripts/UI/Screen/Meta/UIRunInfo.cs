using UnityEngine;

public enum UIRunInfoPage
{
    EmployeeInfo,
    MechInfo
}

public class UIRunInfo : Singleton<UIRunInfo>
{
    public GameObject OpenButton;
    public GameObject MainWindow;

    public GameObject EmployeeInfoPage;
    public UIMechsPage MechInfoPage;


    protected override void Awake()
    {
        base.Awake();
        ShowPage( UIRunInfoPage.EmployeeInfo );
        Close();
    }

    public void ShowPage( UIRunInfoPage page )
    {
        EmployeeInfoPage.Opt()?.SetActive( false );
        MechInfoPage.Opt()?.gameObject.SetActive( false );

        switch( page )
        {
            case UIRunInfoPage.EmployeeInfo:
                EmployeeInfoPage.Opt()?.SetActive( true );
                break;
            case UIRunInfoPage.MechInfo:
                MechInfoPage.Opt()?.gameObject.SetActive( true );
                break;
        }
    }

    public void SelectMech( MechData data )
    {
        ShowPage( UIRunInfoPage.MechInfo );
        MechInfoPage.Opt()?.SelectMech( data );
    }

    public void UI_ShowEmployeeInfo()
    {
        ShowPage( UIRunInfoPage.EmployeeInfo );
    }

    public void UI_ShowMechsInfo()
    {
        ShowPage( UIRunInfoPage.MechInfo );
    }

    public void Close()
    {
        MainWindow.Opt()?.SetActive( false );
        OpenButton.Opt()?.SetActive( true );
    }


    public void Open()
    {
        ShowPage( UIRunInfoPage.EmployeeInfo );
        MainWindow.Opt()?.SetActive( true );
        OpenButton.Opt()?.SetActive( false );
    }
}
