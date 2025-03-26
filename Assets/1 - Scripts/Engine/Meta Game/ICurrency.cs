using System;
using UnityEngine;

public interface ICurrency
{
    /// <summary>
    /// Get the amount of this currency you have.
    /// </summary>
    /// <returns>The amount of the currency</returns>
    int GetAmount();

    /// <summary>
    /// Are you able to afford this amount of this currency?
    /// </summary>
    /// <param name="amount">The amount of the currency you want to spend</param>
    /// <returns>True if can be afforded, false otherwise</returns>
    bool CanAfford( int amount );

    void Add( int amount );

    /// <summary>
    /// Deduct this amount of currency from the amount you have.
    /// </summary>
    /// <param name="amount">The amount you want to spend.</param>
    void Spend( int amount, System.Action OnSuccess, System.Action onFailure );

    string GetDisplayName();
    string GetDescription();
}

public interface IBuyable
{
    int GetCost();
}

public interface ISellable
{
    int GetValue();
}

