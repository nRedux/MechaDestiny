using Newtonsoft.Json;
using UnityEngine;

public class MoneyChangeEvent : GameEvent
{
    public int Change;
    public int NewAmount;

    public MoneyChangeEvent( int change, int newAmount )
    {
        Change = change;
        NewAmount = newAmount;
    }
}

[JsonObject]
public class Money : ICurrency, IItem
{
    public const string OBJECT_ID = "MONEY_ITEM_OBJECT_ID";

    [JsonProperty]
    private int _amount = 0;

    [JsonProperty]
    private bool _doEvents = false;

    [JsonProperty]
    private IStorage _storage = null;

    public IStorage Storage => _storage;

    public bool Storable => true;

    [JsonConstructor]
    public Money() 
    {
    }

    public Money( bool doEvents, int amount )
    {
        _amount = amount;
        _doEvents = doEvents;
    }

    public void Add( int change )
    {
        _amount += Mathf.Abs( change );
        if( _doEvents )
            Events.Instance.Raise( new MoneyChangeEvent( change, _amount ) );
    }

    public bool CanAfford( int amount )
    {
        return _amount >= amount;
    }

    public int GetAmount()
    {
        return _amount;
    }

    public string GetDescription()
    {
        return "It's money.";
    }

    public string GetDisplayName()
    {
        return "Credits";
    }

    public object GetObjectID()
    {
        return OBJECT_ID;
    }

    public string GetObjectName()
    {
        return GetDisplayName();
    }

    public Sprite GetPortrait()
    {
        return GlobalSettings.Instance.MoneySprite;
    }

    public void SetStorage( IStorage storage )
    {
        _storage = storage;
    }

    public void Spend( int cost, System.Action OnSuccess, System.Action onFailure )
    {
        if( !CanAfford( cost ) )
            onFailure?.Invoke();
        _amount -= cost;
        Events.Instance.Raise( new MoneyChangeEvent( cost, _amount ) );
        OnSuccess?.Invoke();
    }
}