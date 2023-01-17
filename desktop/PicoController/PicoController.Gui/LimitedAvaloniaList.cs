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
        InnerAvaloniaList = new AvaloniaList<T>(Limit);
    }

    public T this[int index]
    {
        get
        {
            lock (SyncRoot)
            {
                return InnerAvaloniaList[index];
            }
        }

        set
        {
            lock (SyncRoot)
            {
                InnerAvaloniaList[index] = value;
            }
        }
    }

    T IReadOnlyList<T>.this[int index]
    {
        get
        {
            lock (SyncRoot)
            {
                return InnerReadOnly[index];
            }
        }
    }

    object? IList.this[int index]
    {
        get
        {
            lock (SyncRoot)
            {
                return InnerIList[index];
            }
        }

        set
        {
            lock (SyncRoot)
            {
                InnerIList[index] = value;
            }
        }
    }

    public int Limit { get; }

    private IList InnerIList => InnerAvaloniaList;
    private IReadOnlyList<T> InnerReadOnly => InnerAvaloniaList;
    private readonly AvaloniaList<T> InnerAvaloniaList;

    public int Count
    {
        get
        {
            lock (SyncRoot)
            {
                if (InnerAvaloniaList.Count <= Limit)
                {
                    return InnerAvaloniaList.Count;
                }

                InnerAvaloniaList.RemoveRange(Limit - 1, InnerAvaloniaList.Count - Limit);

                return InnerAvaloniaList.Count;
            }
        }
    }

    public bool IsReadOnly => InnerIList.IsReadOnly;

    public bool IsFixedSize => InnerIList.IsFixedSize;

    public bool IsSynchronized => true;

    public object SyncRoot => (InnerAvaloniaList as ICollection)!.SyncRoot;

    public event NotifyCollectionChangedEventHandler? CollectionChanged
    {
        add
        {
            lock (SyncRoot)
            {
                InnerAvaloniaList.CollectionChanged += value;
            }
        }

        remove
        {
            lock (SyncRoot)
            {
                InnerAvaloniaList.CollectionChanged -= value;
            }
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add
        {
            lock (SyncRoot)
            {
                InnerAvaloniaList.PropertyChanged += value;
            }
        }

        remove
        {
            lock (SyncRoot)
            {
                InnerAvaloniaList.PropertyChanged -= value;
            }
        }
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
        lock (SyncRoot)
        {
            LimitElements(1);
            InnerAvaloniaList.Add(item); 
        }
    }

    public int Add(object? value)
    {
        lock (SyncRoot)
        {
            LimitElements(1);
            return InnerIList.Add(value); 
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        lock (SyncRoot)
        {
            LimitElements(items.Count());
            InnerAvaloniaList.AddRange(items);
        }
    }

    public void Clear()
    {
        lock (SyncRoot)
        {
            InnerAvaloniaList.Clear();
        }
    }

    public bool Contains(T item)
    {
        lock (SyncRoot)
        {
            return InnerAvaloniaList.Contains(item);
        }
    }

    public bool Contains(object? value)
    {
        lock (SyncRoot)
        {
            return InnerIList.Contains(value);
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        lock (SyncRoot)
        {
            InnerAvaloniaList.CopyTo(array, arrayIndex);
        }
    }

    public void CopyTo(Array array, int index)
    {
        lock (SyncRoot)
        {
            InnerIList.CopyTo(array, index);
        }
    }

    Delegate[]? INotifyCollectionChangedDebug.GetCollectionChangedSubscribers()
    {
        lock (SyncRoot)
        {
            return ((INotifyCollectionChangedDebug)InnerAvaloniaList).GetCollectionChangedSubscribers();
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (SyncRoot)
        {
            return InnerAvaloniaList.GetEnumerator();
        }
    }

    public int IndexOf(T item) {
        lock (SyncRoot)
        {
            return InnerAvaloniaList.IndexOf(item);
        }
    }

    public int IndexOf(object? value) {
        lock (SyncRoot)
        {
            return InnerIList.IndexOf(value);
        }
    }

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
    {
        lock (SyncRoot)
        {
            InnerAvaloniaList.Move(oldIndex, newIndex);
        }
    }

    public void MoveRange(int oldIndex, int count, int newIndex) 
        {
        lock (SyncRoot)
        {
            InnerAvaloniaList.MoveRange(oldIndex, count, newIndex);
        }
    }

    public bool Remove(T item) 
    {
        lock (SyncRoot)
        {
            return InnerAvaloniaList.Remove(item);
        }
    }

    public void Remove(object? value)
    {
        lock (SyncRoot)
        {
            InnerIList.Remove(value);
        }
    }

    public void RemoveAll(IEnumerable<T> items)
        {
        lock (SyncRoot)
        {
            InnerAvaloniaList.RemoveAll(items);
        }
    }

    public void RemoveAt(int index)
        {
        lock (SyncRoot)
        {
            InnerAvaloniaList.RemoveAt(index);
        }
    }

    public void RemoveRange(int index, int count)
        {
        lock (SyncRoot)
        {
            InnerAvaloniaList.RemoveRange(index, count);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
        {
        lock (SyncRoot)
        {
            return InnerAvaloniaList.GetEnumerator();
        }
    }
}
