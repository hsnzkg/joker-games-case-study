using System;
using System.Collections;
using System.Collections.Generic;

namespace Project.Scripts.Observable
{
    public class ObservableList<T> : IReadOnlyList<T>
    {
        private readonly List<T> m_items;

        public int Count => m_items.Count;
        public T this[int index] => m_items[index];

        public event Action<IReadOnlyList<T>> OnChanged;
        public event Action<T, int> OnItemAdded;
        public event Action<T, int> OnItemRemoved;
        public event Action OnCleared;

        public ObservableList()
        {
            m_items = new List<T>();
        }

        public ObservableList(IEnumerable<T> collection)
        {
            m_items = new List<T>(collection);
        }

        public void Add(T item)
        {
            m_items.Add(item);
            OnItemAdded?.Invoke(item, m_items.Count - 1);
            OnChanged?.Invoke(this);
        }

        public void RemoveAt(int index)
        {
            var removed = m_items[index];
            m_items.RemoveAt(index);
            OnItemRemoved?.Invoke(removed, index);
            OnChanged?.Invoke(this);
        }

        public void Clear()
        {
            if (m_items.Count == 0)
                return;

            m_items.Clear();
            OnCleared?.Invoke();
            OnChanged?.Invoke(this);
        }

        public void ReplaceAll(IEnumerable<T> newItems)
        {
            m_items.Clear();
            m_items.AddRange(newItems);
            OnChanged?.Invoke(this);
        }

        public bool Contains(T item) => m_items.Contains(item);
        public int IndexOf(T item) => m_items.IndexOf(item);

        public IEnumerator<T> GetEnumerator() => m_items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}