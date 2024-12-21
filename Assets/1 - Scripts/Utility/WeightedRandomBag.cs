using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEngine;
using Edgeflow;

public interface IWeightedBagItem
{
    int Weight { get; }
}

public class WeightedRandomBagItem<T>
{
    public double accumulatedWeight = 1f;
    public T item;
}

#if UNITY_EDITOR
public interface IListenBagChanges<T>
{
    void OnBeforeBagCollectionChanged(CollectionChangeInfo info, T collection);
    void OnAfterBagCollectionChanged(CollectionChangeInfo info, T collection);

}
#endif

public class WeightedRandomBag<TBagItem, TDataType> where TBagItem : WeightedRandomBagItem<TDataType>, new()
{

#if UNITY_EDITOR
    [OnCollectionChanged("BeforeChanges", "AfterChanges")]
#endif
    [LabelText("$EntriesInspectorName")]
    public List<TBagItem> entries = new List<TBagItem>();
    private double accumulatedWeight;
    public static System.Random rand = new System.Random();

    private List<TBagItem> _preEntries = new List<TBagItem>();

    [NonSerialized]
    [HideInInspector]
    public string EntriesInspectorName = string.Empty;

#if UNITY_EDITOR
    private void AfterChanges(CollectionChangeInfo info, InspectorProperty property)
    {
        var listener = property.SerializationRoot.ValueEntry.WeakSmartValue as IListenBagChanges<WeightedRandomBag<TBagItem, TDataType>>;

        if (listener == null) return;

        listener.OnAfterBagCollectionChanged(info, property.Parent.ValueEntry.WeakSmartValue as WeightedRandomBag<TBagItem, TDataType>);
    }

    private void BeforeChanges(CollectionChangeInfo info, InspectorProperty property)
    {
        var listener = property.SerializationRoot.ValueEntry.WeakSmartValue as IListenBagChanges<WeightedRandomBag<TBagItem, TDataType>>;

        if (listener == null) return;

        listener.OnBeforeBagCollectionChanged(info, property.Parent.ValueEntry.WeakSmartValue as WeightedRandomBag<TBagItem, TDataType>);
    }
#endif

    public void Add(float weight, TDataType entry )
    {
        var item = new TBagItem();
        item.item = entry;
        entries.Add( item );
    }

    public TDataType GetRandom(System.Random random = null)
    {
        accumulatedWeight = 0;
        foreach (var entry in entries)
        {
            accumulatedWeight += entry.accumulatedWeight;
        }
        double r = rand.NextDouble() * accumulatedWeight;

        foreach (var entry in entries)
        {
            if (entry.accumulatedWeight >= r)
            {
                return entry.item;
            }
            r -= entry.accumulatedWeight;
        }
        return default(TDataType); //should only happen when there are no entries
    }

    public TBagItem GetRandomBagItem( System.Random random = null )
    {
        accumulatedWeight = 0;
        foreach( var entry in entries )
        {
            accumulatedWeight += entry.accumulatedWeight;
        }
        double r = rand.NextDouble() * accumulatedWeight;

        foreach( var entry in entries )
        {
            if( entry.accumulatedWeight >= r )
            {
                return entry;
            }
            r -= entry.accumulatedWeight;
        }
        return null; //should only happen when there are no entries
    }
}