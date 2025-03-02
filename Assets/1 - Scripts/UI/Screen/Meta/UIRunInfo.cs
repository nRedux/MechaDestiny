using UnityEngine;

public enum UIRunInfoPage
{
    EmployeeInfo,
    MechInfo
}

public class UIRunInfo : MonoBehaviour
{
    public GameObject OpenButton;
    public GameObject MainWindow;

    public GameObject EmployeeInfoPage;
    public GameObject MechInfoPage;


    private void Awake()
    {
        ShowPage( UIRunInfoPage.EmployeeInfo );
        Close();
    }


    public void ShowPage( UIRunInfoPage page )
    {
        EmployeeInfoPage.Opt()?.SetActive( false );
        MechInfoPage.Opt()?.SetActive( false );

        switch( page )
        {
            case UIRunInfoPage.EmployeeInfo:
                EmployeeInfoPage.Opt()?.SetActive( true );
                break;
            case UIRunInfoPage.MechInfo:
                MechInfoPage.Opt()?.SetActive( true );
                break;

        }
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
