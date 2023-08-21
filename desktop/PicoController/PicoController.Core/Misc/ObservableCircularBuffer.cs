using CircularBuffer;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PicoController.Core.Misc
{
    public class ObservableCircularBuffer<T> : CircularBuffer<T>, INotifyCollectionChanged, IList<T>, IList
    {
        public int Count => Size;

        public bool IsReadOnly => false;

        public bool IsFixedSize => true;

        public bool IsSynchronized => false;

        public object SyncRoot => this;

#pragma warning disable CS8600 
#pragma warning disable CS8601 
        object? IList.this[int index] { get => this[index]; set => this[index] = (T)value; }
#pragma warning restore CS8601 
#pragma warning restore CS8600 

        public ObservableCircularBuffer(int capacity) : base(capacity)
        {
        }

        public ObservableCircularBuffer(int capacity, T[] items) : base(capacity, items)
        {
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        new public T this[int index]
        {
            get => base[index];
            set
            {
                var item = base[index];
                base[index] = value;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, item));
            }
        }

        public new void PushBack(T item)
        {
            base.PushBack(item);

            CollectionChanged?.Invoke(
                this,
                new(
                    NotifyCollectionChangedAction.Add,
                    item,
                    Size - 1));
        }

        new public void PushFront(T item)
        {
            base.PushFront(item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item, 0));
        }

        public new void PopBack()
        {
            base.PopBack();
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove));
        }
        public new void PopFront()
        {
            base.PopFront();
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove));
        }

        public new void Clear()
        {
            base.Clear();
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Size; i++)
            {
                if (this[i]?.Equals( item) ?? false)
                    return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            this[index] = item;
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            PushBack(item);
        }

        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public int Add(object? value)
        {
            Add((T)value!);
            return Size;
        }

        public bool Contains(object? value)
        {
            return IndexOf(value) > -1;
        }

        public int IndexOf(object? value)
        {
            return IndexOf((T)value!);
        }

        public void Insert(int index, object? value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object? value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }
}
