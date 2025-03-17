using UnityEngine;

public enum UIRunInfoPage
{
    EmployeeInfo,
    MechInfo,
    InventoryInfo
}

public class UIRunInfo : Singleton<UIRunInfo>
{
    public GameObject OpenButton;
    public GameObject MainWindow;

    public UIEmployeePage EmployeeInfoPage;
    public UIMechsPage MechInfoPage;
    public UIInventoryPage InventoryPage;

    public UIMechSelector MechSelector;
    public UIOptionDialog OptionDialog;

    protected override void Awake()
    {
        base.Awake();
        ShowPage( UIRunInfoPage.EmployeeInfo );
        Close();

        MechSelector.Opt()?.gameObject.SetActive( false );
        OptionDialog.Opt()?.gameObject.SetActive( false );
    }

    public void ShowPage( UIRunInfoPage page )
    {
        EmployeeInfoPage.Opt()?.gameObject.SetActive( false );
        MechInfoPage.Opt()?.gameObject.SetActive( false );
        InventoryPage.Opt()?.gameObject.SetActive( false );

        switch( page )
        {
            case UIRunInfoPage.EmployeeInfo:
                EmployeeInfoPage.Opt()?.gameObject.SetActive( true );
                break;
            case UIRunInfoPage.MechInfo:
                MechInfoPage.Opt()?.gameObject.SetActive( true );
                break;
            case UIRunInfoPage.InventoryInfo:
                InventoryPage.Opt()?.gameObject.SetActive( true );
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

    public void UI_ShowInventoryInfo()
    {
        ShowPage( UIRunInfoPage.InventoryInfo );
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
