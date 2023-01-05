using Avalonia.Collections;
using Avalonia.Diagnostics;
using FluentAvalonia.Core;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PicoController.Gui;

public class LimitedAvaloniaList<T> : IAvaloniaList<T>, IList, INotifyCollectionChangedDebug
{
	public LimitedAvaloniaList(int limit)
	{
        Limit = limit;
        InnerAvaloniaList = new AvaloniaList<T>();
    }

    public T this[int index]
    {
        get => InnerAvaloniaList[index];
        set => InnerAvaloniaList[index] = value;
    }

    T IReadOnlyList<T>.this[int index] => InnerReadOnly[index];

    object? IList.this[int index] 
    {
        get => InnerIList[index];
        set => InnerIList[index] = value;
    }

    public int Limit { get; }

    private IList InnerIList => InnerAvaloniaList;
    private IReadOnlyList<T> InnerReadOnly => InnerAvaloniaList;
    private readonly AvaloniaList<T> InnerAvaloniaList;

    public int Count => InnerAvaloniaList.Count;

    public bool IsReadOnly => InnerIList.IsReadOnly;

    public bool IsFixedSize => InnerIList.IsFixedSize;

    public bool IsSynchronized => InnerIList.IsSynchronized;

    public object SyncRoot => (InnerAvaloniaList as ICollection)!.SyncRoot;

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add => InnerAvaloniaList.CollectionChanged += value;
        remove => InnerAvaloniaList.CollectionChanged -= value;
    }
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => InnerAvaloniaList.PropertyChanged += value;
        remove => InnerAvaloniaList.PropertyChanged -= value;
    }

    private void LimitElements(int itemsToAddCount)
    {
        var newCount = Count + itemsToAddCount;
        if (newCount > Limit)
        {
            InnerAvaloniaList.RemoveRange(0, newCount - Limit);
        }
    }

    public void Add(T item)
    {
        LimitElements(1);
        InnerAvaloniaList.Add(item);
    }

    public int Add(object? value)
    {
        LimitElements(1);
        return InnerIList.Add(value);
    }

    public void AddRange(IEnumerable<T> items)
    {
        LimitElements(items.Count());
        InnerAvaloniaList.AddRange(items);
    }

    public void Clear() => InnerAvaloniaList.Clear();

    public bool Contains(T item) => InnerAvaloniaList.Contains(item);

    public bool Contains(object? value) => InnerIList.Contains(value);

    public void CopyTo(T[] array, int arrayIndex) => InnerAvaloniaList.CopyTo(array, arrayIndex);

    public void CopyTo(Array array, int index) => InnerIList.CopyTo(array, index);

    Delegate[]? INotifyCollectionChangedDebug.GetCollectionChangedSubscribers() =>
        ((INotifyCollectionChangedDebug)InnerAvaloniaList).GetCollectionChangedSubscribers();

    public IEnumerator<T> GetEnumerator() => InnerAvaloniaList.GetEnumerator();

    public int IndexOf(T item) => InnerAvaloniaList.IndexOf(item);

    public int IndexOf(object? value) => InnerIList.IndexOf(value);

    public void Insert(int index, T item)
    {
        LimitElements(1);

        InnerAvaloniaList.Insert(index, item);
    }

    public void Insert(int index, object? value)
    {
        LimitElements(1);

        InnerIList.Insert(index, value);
    }

    public void InsertRange(int index, IEnumerable<T> items)
    {
        var count = items.Count();
        LimitElements(count);

        InnerAvaloniaList.InsertRange(index, items);
    }

    public void Move(int oldIndex, int newIndex)
        => InnerAvaloniaList.Move(oldIndex, newIndex);

    public void MoveRange(int oldIndex, int count, int newIndex) 
        => InnerAvaloniaList.MoveRange(oldIndex, count, newIndex);

    public bool Remove(T item) 
        => InnerAvaloniaList.Remove(item);

    public void Remove(object? value)
        => InnerIList.Remove(value);

    public void RemoveAll(IEnumerable<T> items)
        => InnerAvaloniaList.RemoveAll(items);

    public void RemoveAt(int index)
        => InnerAvaloniaList.RemoveAt(index);

    public void RemoveRange(int index, int count)
        => InnerAvaloniaList.RemoveRange(index, count);

    IEnumerator IEnumerable.GetEnumerator()
        => InnerAvaloniaList.GetEnumerator();
}
