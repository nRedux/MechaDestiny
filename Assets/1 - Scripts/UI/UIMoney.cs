using UnityEngine;

public class UIMoney : UICurrency
{
    private void Awake()
    {
        Events.Instance.AddListener<MoneyChangeEvent>( OnMoneyChanged );
    }

    private void Start()
    {
        var money = RunManager.Instance.RunData.Inventory.GetItem<Money>() as ICurrency;
        UpdateAmount( money.GetAmount() );
    }

    private void OnMoneyChanged( MoneyChangeEvent e )
    {
        UpdateAmount( e.NewAmount );
    }
}
