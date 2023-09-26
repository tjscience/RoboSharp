using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.ObjectModel;

namespace RoboSharp
{
    /// <summary>
    /// Collection object that will return a new <see cref="ImmutableList{T}"/> on any proposed changes to the underlying collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks></remarks>
    internal sealed class ImmutableList<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ICollection<T>, ICollection, IList<T>, IList
    {
        /// <summary>
        /// Get a new Empty collection
        /// </summary>
        public static ImmutableList<T> Empty => new ImmutableList<T>(new List<T>());

        private static ImmutableList<T> New(List<T> collection) => new ImmutableList<T>(collection);

        private ImmutableList(List<T> collection) { ItemCollection = collection; }

        private List<T> ItemCollection { get;}

        /// <inheritdoc/>
        public int Count => ItemCollection.Count;

        /// <inheritdoc/>
        public object SyncRoot => this;

        /// <inheritdoc/>
        public bool IsSynchronized => true;

        /// <inheritdoc/>    
        public T this[int index] => ItemCollection[index];

        #region < Enumeration >

        public bool Contains(T item)
        {
            return ItemCollection.Contains(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ItemCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)ItemCollection).GetEnumerator();
        }

        #endregion

        #region < Insert & Removal >

        public ImmutableList<T> Clear()
        {
            return Empty;
        }

        public ImmutableList<T> Insert(int index, T item)
        {
            var items = new List<T>(ItemCollection);
            items.Insert(index, item);
            return New(items);
        }

        public ImmutableList<T> InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection is null) throw new ArgumentNullException(nameof(collection));
            var items = new List<T>(ItemCollection);
            items.InsertRange(index, collection);
            return New(items);
        }

        public ImmutableList<T> Add(T item)
        {
            return Insert(ItemCollection.Count, item);
        }

        public ImmutableList<T> AddRange(IEnumerable<T> collection)
        {
            return InsertRange(ItemCollection.Count, collection);
        }

        public ImmutableList<T> Remove(T item)
        {
            var items = new List<T>(ItemCollection);
            items.Remove(item);
            return New(items);
        }

        public ImmutableList<T> RemoveRange(int index, int count)
        {
            var items = new List<T>(ItemCollection);
            items.RemoveRange(index, count);
            return New(items);
        }

        public ImmutableList<T> RemoveAt(int index)
        {
            var items = new List<T>(ItemCollection);
            items.RemoveAt(index);
            return New(items);
        }

        public ImmutableList<T> RemoveAll(Predicate<T> match)
        {
            var items = new List<T>(ItemCollection);
            items.RemoveAll(match);
            return New(items);
        }

        public ImmutableList<T> Replace(int index, T newItem, out bool replaced)
        {

            if (index < 0 || index > ItemCollection.Count - 1) throw new ArgumentOutOfRangeException(nameof(index));
            var items = new List<T>(ItemCollection);
            items[index] = newItem;
            replaced = true;
            return New(items);
        }

        public ImmutableList<T> Replace(T oldItem, T newItem, out bool replaced)
        {
            var i = ItemCollection.IndexOf(oldItem);
            replaced = i >= 0;
            if (!replaced) return this;
            return Replace(i, newItem, out replaced);
        }

        public ImmutableList<T> Replace(int index, IEnumerable<T> collection, out bool replaced)
        {
            if (index < 0 || index >= ItemCollection.Count) throw new ArgumentOutOfRangeException(nameof(index));
            var itemsToReplace = ItemCollection.GetRange(index, ItemCollection.Count - index - 1);
            var newItems = collection.ToList();
            int additionalItems = newItems.Count - itemsToReplace.Count;
            var items = new List<T>(ItemCollection);
            
            items.RemoveRange(index, itemsToReplace.Count);
            if (additionalItems > 0 | index == items.Count)
                items.AddRange(collection);
            else
                items.InsertRange(index, newItems.GetRange(0, itemsToReplace.Count));

            replaced = true;
            return New(items);
        }

        #endregion

        #region < Sort & Move >

        public ImmutableList<T> Sort()
        {
            var items = new List<T>(ItemCollection);
            items.Sort();
            return New(items);
        }

        public ImmutableList<T> Sort(Comparison<T> comparison)
        {
            var items = new List<T>(ItemCollection);
            items.Sort(comparison);
            return New(items);
        }

        public ImmutableList<T> Sort(IComparer<T> comparer)
        {
            var items = new List<T>(ItemCollection);
            items.Sort(comparer);
            return New(items);
        }

        public ImmutableList<T> Sort(int index, int count, IComparer<T> comparer)
        {
            var items = new List<T>(ItemCollection);
            items.Sort(index, count, comparer);
            return New(items);
        }

        public ImmutableList<T> Reverse()
        {
            var items = new List<T>(ItemCollection);
            items.Reverse();
            return New(items);
        }

        public ImmutableList<T> Reverse(int index, int count)
        {
            var items = new List<T>(ItemCollection);
            items.Reverse(index,count);
            return New(items);
        }

        #endregion

        #region < Other List Methods >

        #region < CopyTo >

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)"/>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            ItemCollection.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])"/>
        public void CopyTo(T[] array)
        {
            ItemCollection.CopyTo(array);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            ItemCollection.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)ItemCollection).CopyTo(array, index);
        }

        #endregion

        #region < IndexOf >

        /// <inheritdoc cref="List{T}.IndexOf(T)"/>
        public int IndexOf(T item) => ItemCollection.IndexOf(item);

        /// <inheritdoc cref="List{T}.IndexOf(T, int)"/>
        public int IndexOf(T item, int index) => ItemCollection.IndexOf(item, index);

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)"/>
        public int IndexOf(T item, int index, int count) => ItemCollection.IndexOf(item, index, count);

        #endregion

        #region < Find >

        /// <inheritdoc cref="List{T}.Find(Predicate{T})"/>
        public T Find(Predicate<T> match) => ItemCollection.Find(match);

        /// <inheritdoc cref="List{T}.FindAll(Predicate{T})"/>
        public List<T> FindAll(Predicate<T> match) => ItemCollection.FindAll(match);

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
        public int FindIndex(Predicate<T> match) => ItemCollection.FindIndex(match);

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})"/>
        public int FindIndex(int startIndex, Predicate<T> match) => ItemCollection.FindIndex(startIndex, match);

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})"/>
        public int FindIndex(int startIndex, int count, Predicate<T> match) => ItemCollection.FindIndex(startIndex, count, match);

        /// <inheritdoc cref="List{T}.FindLast(Predicate{T})"/>
        public T FindLast(Predicate<T> match) => ItemCollection.FindLast(match);

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})"/>
        public int FindLastIndex(Predicate<T> match) => ItemCollection.FindLastIndex(match);

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})"/>
        public int FindLastIndex(int startingIndex, Predicate<T> match) => ItemCollection.FindLastIndex(startingIndex, match);

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})"/>
        public int FindLastIndex(int startingIndex, int count, Predicate<T> match) => ItemCollection.FindLastIndex(startingIndex, count, match);

        /// <inheritdoc cref="List{T}.Exists(Predicate{T})"/>
        public bool Exists(Predicate<T> match) => ItemCollection.Exists(match);

        #endregion

        /// <inheritdoc cref="List{T}.ForEach(Action{T})"/>
        public void ForEach(Action<T> action) => ItemCollection.ForEach(action);

        /// <inheritdoc/>
        public List<T> GetRange(int index, int count) => ItemCollection.GetRange(index, count);

        /// <inheritdoc cref="List{T}.TrueForAll(Predicate{T})"/>
        public bool TrueForAll(Predicate<T> match)
        {
            return ItemCollection.TrueForAll(match);
        }

        #region < LastIndex >

        /// <inheritdoc cref="List{T}.LastIndexOf(T)"/>
        public int LastIndexOf(T item) => ItemCollection.LastIndexOf(item);

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)"/>
        public int LastIndexOf(T item, int index) => ItemCollection.LastIndexOf(item, index);

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)"/>
        public int LastIndexOf(T item, int index, int count) => ItemCollection.LastIndexOf(item, index, count);

        #endregion

        #region < BinarySearch >

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})"/>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) => ItemCollection.BinarySearch(index, count, item, comparer);

        /// <inheritdoc cref="List{T}.BinarySearch(T)"/>
        public int BinarySearch(T item) => ItemCollection.BinarySearch(item);
        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})"/>
        public int BinarySearch(T item, IComparer<T> comparer) => ItemCollection.BinarySearch(item, comparer);

        #endregion

        #endregion

        #region < IList >

        T IList<T>.this[int index] { get => this[index]; set => throw new NotImplementedException(); }
        void IList<T>.Insert(int index, T item) => throw new NotSupportedException();
        void IList<T>.RemoveAt(int index) => throw new NotSupportedException();

        void ICollection<T>.Add(T item) => throw new NotImplementedException();
        void ICollection<T>.Clear() => throw new NotImplementedException();
        bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
        bool ICollection<T>.IsReadOnly => true;

        bool IList.IsFixedSize => true;
        bool IList.IsReadOnly => true;
        object IList.this[int index] { get => this[index]; set => throw new NotImplementedException(); }
        int IList.Add(object value) => throw new NotImplementedException();
        void IList.Clear() => throw new NotImplementedException();
        void IList.Insert(int index, object value) => throw new NotImplementedException();
        void IList.Remove(object value) => throw new NotImplementedException();
        void IList.RemoveAt(int index) => throw new NotImplementedException();
        bool IList.Contains(object value) 
        {
            if (value is T obj) return Contains(obj);
            return false;
        }
        int IList.IndexOf(object value)
        {
            if (value is T obj) return IndexOf(obj);
            return -1;
        }

        #endregion
    }
}
