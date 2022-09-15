using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Collections;
using System.ComponentModel;

namespace RoboSharp
{
    /// <summary>
    /// This is a thread-safe list whose underlying collection is immutable.
    /// <br/> - When enumerating against the list, you will receive an immutable collection that will not reflect changes if this list is modified by another thread during that iteration.
    /// <br/> - The <see cref="INotifyCollectionChanged"/> event can be used to listen for when the collection changes.
    /// <br/> - <see cref="PropertyChanged"/> is only implemented for the <see cref="ReadOnlyList"/> property of this class, in order to allow safe data-binding for WPF applications.
    /// </summary>
    /// <typeparam name="T">Type of object the list will contain</typeparam>
    /// <remarks>
    /// This class has replaced the obsolete <see cref="ObservableList{T}"/>, which has since been marked as obsolete.
    /// </remarks>
    public partial class ConcurrentList<T> : INotifyCollectionChanged, IList<T>, ICollection<T>, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IList, INotifyPropertyChanged
    {
        #region < Constructors >

        ///<inheritdoc cref="List{T}()"/>
        public ConcurrentList() 
        { }

        ///<inheritdoc cref="List{T}(IEnumerable{T})"/>
        public ConcurrentList(params T[] collection)
        {
            if (collection?.Any() ?? false)
                InternalList = InternalList.AddRange(collection);
        }

        ///<inheritdoc cref="List{T}(IEnumerable{T})"/>
        public ConcurrentList(IEnumerable<T> collection)
        {
            if (collection?.Any() ?? false)
                InternalList = InternalList.AddRange(collection);
        }

        /// <summary>
        /// Convert a List into an ObservableList
        /// </summary>
        /// <param name="list"></param>
        public ConcurrentList(List<T> list)
        {
            if (list?.Any() ?? false)
                InternalList = InternalList.AddRange(list);
        }

        #endregion

        #region < Properties >

        /// <summary>
        /// The lock this object uses to ensure thread-safety
        /// </summary>
        private object LockObject { get; } = new object();

        /// <summary>
        /// The internal list this object uses to provide the collection functionality
        /// </summary>
        private ImmutableList<T> InternalList { 
            get => InternalListField ;
            set { 
                if (InternalListField != value)
                {
                    InternalListField = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReadOnlyList)));
                }
            }
        } 
        private ImmutableList<T> InternalListField = ImmutableList<T>.Empty;

        /// <summary>
        /// Provides a ReadOnly list that can be bound to with WPF, notifications via PropertyChanged
        /// </summary>
        public IReadOnlyList<T> ReadOnlyList => InternalList;

        /// <summary>
        /// set TRUE to only enable <see cref="NotifyCollectionChangedAction.Reset"/> notifications, as opposed to full notifications. (WPF Compatibility)
        /// </summary>
        public bool ResetNotificationsOnly { get; set; } = false;

        /// <inheritdoc/>
        public int Capacity => InternalList.Count;
        /// <inheritdoc/>
        public int Count => InternalList.Count;
        /// <inheritdoc/>
        public object SyncRoot => this;
        /// <inheritdoc/>
        public bool IsSynchronized => true;
        /// <inheritdoc/>
        bool ICollection<T>.IsReadOnly => ((ICollection<T>)InternalList).IsReadOnly;
        /// <inheritdoc/>
        bool IList.IsReadOnly => false;

        #endregion

        #region < Events >

        /// <summary> Notifies when the <see cref="ReadOnlyList"/> property is updated. </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary> This event fires whenever the List's array is updated. </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raise the <see cref="CollectionChanged"/> event with a status of <see cref="NotifyCollectionChangedAction.Reset"/>
        /// </summary>
        public void NotifyCollectionChanged()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Raise the <see cref="CollectionChanged"/> event. <br/>
        /// <para/>
        /// Override this method to provide post-processing of Added/Removed items within derived classes.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (ResetNotificationsOnly && e.Action != NotifyCollectionChangedAction.Reset) e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            // This Syncronization code was taken from here: https://stackoverflow.com/a/54733415/12135042
            // This ensures that the CollectionChanged event is invoked from the proper thread, no matter where the change came from.
            if (CollectionChanged != null)
            {
                NotifyCollectionChangedEventArgs ResetArgs = null;
                bool isWPF = false;
                foreach (NotifyCollectionChangedEventHandler handler in CollectionChanged.GetInvocationList())
                {
                    if (!ResetNotificationsOnly)
                    {
                        // Check for a WPF Control that only accepts the 'RESET' signal
                        var target = handler.Target?.GetType()?.ToString() ?? string.Empty;
                        isWPF = target.StartsWith("System.Windows.Data.");
                        if (isWPF) ResetArgs = ResetArgs ?? (e.Action == NotifyCollectionChangedAction.Reset ? e : new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }

                    if (SynchronizationContext.Current == _synchronizationContext)
                    {
                        // Execute the CollectionChanged event on the current thread
                        handler(this, isWPF ? ResetArgs : e);
                    }
                    else
                    {
                        // Raises the CollectionChanged event on the appropriate thread
                        var syncContext = _synchronizationContext ?? SynchronizationContext.Current;
                        syncContext.Send((callback) => handler(this, isWPF ? ResetArgs : e), null);
                    }
                }
            }
        }
        private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        #region < Alternate methods for OnCollectionChanged + reasoning why it wasn't used >

        /*
         * This standard method cannot be used because RoboQueue is adding results onto the ResultsLists as they complete, which means the events may not be on the original thread that RoboQueue was constructed in.
         * protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);
         */

        // --------------------------------------------------------------------------------------------------------------

        /*
         * This code was taken from here: https://www.codeproject.com/Articles/64936/Threadsafe-ObservableImmutable-Collection
         * It works, but its a bit more involved since it loops through all handlers.
         * This was not used due to being unavailable in some targets. (Same reasoning for creating new class instead of class provided by above link)
         */

        //        /// <summary>
        //        /// Raise the <see cref="CollectionChanged"/> event. <para/>
        //        /// Override this method to provide post-processing of Added/Removed items within derived classes.
        //        /// </summary>
        //        /// <param name="e"></param>
        //        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        //        {
        //            var notifyCollectionChangedEventHandler = CollectionChanged;

        //            if (notifyCollectionChangedEventHandler == null)
        //                return;

        //            foreach (NotifyCollectionChangedEventHandler handler in notifyCollectionChangedEventHandler.GetInvocationList())
        //            {
        //#if NET40_OR_GREATER
        //                var dispatcherObject = handler.Target as DispatcherObject;

        //                if (dispatcherObject != null && !dispatcherObject.CheckAccess())
        //                {
        //                    dispatcherObject.Dispatcher.Invoke(handler, this, e);
        //                }
        //                else
        //#endif
        //                    handler(this, e); // note : this does not execute handler in target thread's context
        //            }
        //        }

        // --------------------------------------------------------------------------------------------------------------
        #endregion

        #endregion

        #region < Conversions >

        /// <summary>
        /// Provide access to the underlying list using explicit casting <br/>
        /// NOTE: Any changes done to the list will NOT be raise observation notifications!
        /// </summary>
        /// <param name="collection">the Observable list to convert</param>
        public static explicit operator List<T>(ConcurrentList<T> collection) => collection.ToList();

        /// <summary>
        /// Cast the list into a new <see cref="ConcurrentList{T}"/> by wrapping the list using <see cref="ConcurrentList{T}.ConcurrentList(List{T})"/>
        /// NOTE: Any changes done to the list after this point will not raise the notifications, but changes done using the new object will raise notifications.
        /// </summary>
        /// <param name="collection">the list to wrap</param>
        public static explicit operator ConcurrentList<T>(List<T> collection) => new ConcurrentList<T>(collection);

        #endregion

        #region < List{T} Methods >

        #region < Add >

        ///<inheritdoc cref="List{T}.Add(T)"/>
        public virtual void Add(T item)
        {
            lock (LockObject)
            {
                InternalList = InternalList.Add(item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, InternalList.Count - 1));
            }
        }

        ///<inheritdoc cref="System.Collections.Generic.List{T}.AddRange(IEnumerable{T})"/>
        public virtual void AddRange(IEnumerable<T> collection)
        {
            lock (LockObject)
            {
                if (collection == null || collection.Count() == 0) return;
                var items = collection.ToArray();
                InternalList = InternalList.AddRange(items);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
            }
        }

        #endregion

        /// <inheritdoc cref="List{T}.AsReadOnly"/>
        public IReadOnlyCollection<T> AsReadOnly() => new ReadOnlyCollection<T>(this);

        #region < BinarySearch >

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})"/>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) => InternalList.BinarySearch(index, count, item, comparer);

        /// <inheritdoc cref="List{T}.BinarySearch(T)"/>
        public int BinarySearch(T item) => InternalList.BinarySearch(item);

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})"/>
        public int BinarySearch(T item, IComparer<T> comparer) => InternalList.BinarySearch(item, comparer);

        #endregion

        ///<inheritdoc cref="List{T}.Clear"/>
        public virtual void Clear()
        {
            lock (LockObject)
            {
                InternalList = InternalList.Clear();
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <inheritdoc/>
        public bool Contains(T item) => InternalList.Contains(item);

        /// <inheritdoc cref="List{T}.ConvertAll{TOutput}(Converter{T, TOutput})"/>
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            return InternalList.ToList().ConvertAll<TOutput>(converter);
        }

        #region < CopyTo >

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)"/>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            InternalList.CopyTo(index, array, arrayIndex, count);
        }

        /// <inheritdoc cref="List{T}.CopyTo(T[])"/>
        public void CopyTo(T[] array)
        {
            InternalList.CopyTo(array);
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            InternalList.CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)InternalList).CopyTo(array, index);
        }

        #endregion

        /// <inheritdoc cref="List{T}.Exists(Predicate{T})"/>
        public bool Exists(Predicate<T> match) => InternalList.Exists(match);

        #region < Find >

        /// <inheritdoc cref="List{T}.Find(Predicate{T})"/>
        public T Find(Predicate<T> match) => InternalList.Find(match);

        /// <inheritdoc cref="List{T}.FindAll(Predicate{T})"/>
        public List<T> FindAll(Predicate<T> match) => InternalList.FindAll(match);

        /// <inheritdoc cref="List{T}.FindIndex(Predicate{T})"/>
        public int FindIndex(Predicate<T> match) => InternalList.FindIndex(match);

        /// <inheritdoc cref="List{T}.FindIndex(int, Predicate{T})"/>
        public int FindIndex(int startIndex, Predicate<T> match) => InternalList.FindIndex(startIndex, match);

        /// <inheritdoc cref="List{T}.FindIndex(int, int, Predicate{T})"/>
        public int FindIndex(int startIndex, int count, Predicate<T> match) => InternalList.FindIndex(startIndex, count, match);

        /// <inheritdoc cref="List{T}.FindLast(Predicate{T})"/>
        public T FindLast(Predicate<T> match) => InternalList.FindLast(match);

        /// <inheritdoc cref="List{T}.FindLastIndex(Predicate{T})"/>
        public int FindLastIndex(Predicate<T> match) => InternalList.FindLastIndex(match);

        /// <inheritdoc cref="List{T}.FindLastIndex(int, Predicate{T})"/>
        public int FindLastIndex(int startingIndex, Predicate<T> match) => InternalList.FindLastIndex(startingIndex, match);

        /// <inheritdoc cref="List{T}.FindLastIndex(int, int, Predicate{T})"/>
        public int FindLastIndex(int startingIndex, int count, Predicate<T> match) => InternalList.FindLastIndex(startingIndex, count, match);

        #endregion

        /// <inheritdoc cref="List{T}.ForEach(Action{T})"/>
        public void ForEach(Action<T> action) => InternalList.ForEach(action);

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => InternalList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public List<T> GetRange(int index, int count) => InternalList.GetRange(index, count);

        #region < IndexOf >

        /// <inheritdoc cref="List{T}.IndexOf(T)"/>
        public int IndexOf(T item) => InternalList.IndexOf(item);

        /// <inheritdoc cref="List{T}.IndexOf(T, int)"/>
        public int IndexOf(T item, int index) => InternalList.IndexOf(item, index);

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)"/>
        public int IndexOf(T item, int index, int count) => InternalList.IndexOf(item, index, count);

        #endregion

        #region < Insert >

        ///<inheritdoc cref="List{T}.Insert(int, T)"/>
        ///<remarks> Generates <see cref="CollectionChanged"/> event for item that was added and item that was shifted ( Event is raised twice ) </remarks>
        public virtual void Insert(int index, T item)
        {
            lock (LockObject)
            {
                InternalList = InternalList.Insert(index, item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index: index));
            }
        }

        ///<inheritdoc cref="List{T}.InsertRange(int, IEnumerable{T})"/>
        ///<remarks> Generates <see cref="CollectionChanged"/> event for items that were added and items that were shifted ( Event is raised twice )</remarks>
        public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            lock (LockObject)
            {
                
                if (collection == null || collection.Count() == 0) return;
                var items = collection.ToArray();
                InternalList = InternalList.InsertRange(index, items);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems: items, index));
            }
        }

        #endregion

        #region < LastIndex >

        /// <inheritdoc cref="List{T}.LastIndexOf(T)"/>
        public int LastIndexOf(T item) => InternalList.LastIndexOf(item);

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int)"/>
        public int LastIndexOf(T item, int index) => InternalList.LastIndexOf(item, index);

        /// <inheritdoc cref="List{T}.LastIndexOf(T, int, int)"/>
        public int LastIndexOf(T item, int index, int count) => InternalList.LastIndexOf(item, index, count);

        #endregion

        #region < Remove >

        ///<inheritdoc cref="List{T}.Remove(T)"/>
        public virtual bool Remove(T item)
        {
            lock (LockObject)
            {
                if (!InternalList.Contains(item)) return false;

                int i = InternalList.IndexOf(item);
                if ( i >= 0 )
                {
                    InternalList = InternalList.Remove(item);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, i));
                    return true;
                }
                else
                    return false;
            }
        }

        ///<inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        public virtual int RemoveAll(Predicate<T> match)
        {
            lock (LockObject)
            {
                List<T> removedItems = InternalList.FindAll(match);
                int ret = removedItems.Count;
                if (ret > 0)
                {
                    InternalList = InternalList.RemoveAll(match);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
                }
                return ret;
            }
        }

        ///<inheritdoc cref="List{T}.RemoveAt(int)"/>
        public virtual void RemoveAt(int index)
        {
            lock (LockObject)
            {
                T item = InternalList[index];
                InternalList = InternalList.RemoveAt(index);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index: index));
            }
        }

        ///<inheritdoc cref="List{T}.RemoveRange(int,int)"/>
        public virtual void RemoveRange(int index, int count)
        {
            lock (LockObject)
            {
                var removedItems = InternalList.GetRange(index, count).ToArray();
                if (removedItems.Length > 0)
                {
                    InternalList = InternalList.RemoveRange(index, count);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, index));
                }
            }
        }

        #endregion

        #region < Reverse >

        ///<inheritdoc cref="List{T}.Reverse()"/>
        public virtual void Reverse()
        {
            PerformMove(InternalList.Reverse);
        }

        ///<inheritdoc cref="List{T}.Reverse(int, int)"/>
        public virtual void Reverse(int index, int count)
        {
            PerformMove(() => InternalList.Reverse(index, count));
        }

        #endregion

        #region < Sort >

        ///<inheritdoc cref="List{T}.Sort()"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort()
        {
            PerformMove(() => InternalList.Sort());
        }

        ///<inheritdoc cref="List{T}.Sort(Comparison{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(Comparison<T> comparison)
        {
            PerformMove(() => InternalList.Sort(comparison));
        }

        ///<inheritdoc cref="List{T}.Sort(IComparer{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(IComparer<T> comparer)
        {
            PerformMove(() => InternalList.Sort(comparer));
        }

        ///<inheritdoc cref="List{T}.Sort(int, int, IComparer{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(int index, int count, IComparer<T> comparer)
        {
            PerformMove(() => InternalList.Sort(index, count, comparer));
        }

        #endregion

        /// <summary>
        /// Get or Set the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to Get or Set.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public T this[int index]
        {
            get => InternalList[index];
            set
            {
                if (index >= 0 && index < Count)
                {
                    Replace(index, value);
                }
                else if (index == Count)
                {
                    Add(value);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        /// <inheritdoc cref="List{T}.ToArray"/>
        public T[] ToArray() => InternalList.ToArray();

        /// <inheritdoc cref="List{T}.TrueForAll(Predicate{T})"/>
        public bool TrueForAll(Predicate<T> match)
        {
            return InternalList.TrueForAll(match);
        }

        #endregion

        #region < New Methods >

        /// <remarks>
        /// Per <see cref="INotifyCollectionChanged"/> rules, generates <see cref="CollectionChanged"/> event for every item that has moved within the list. <br/>
        /// </remarks>
        /// <param name="MoveAction">Action to perform that will rearrange items in the list - should not add, remove or replace!</param>
        private void PerformMove(Func<ImmutableList<T>> MoveAction)
        {
            lock (LockObject)
            {
                //Store Old List Order
                var OldIndexList = InternalList;

                //Perform the move
                this.InternalList = MoveAction.Invoke();

                var movedItems = InternalList.Where(item => InternalList.IndexOf(item) != OldIndexList.IndexOf(item)).ToArray();

                //Generate the event
                if (ResetNotificationsOnly)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                else
                {
                    foreach (T obj in movedItems)
                    {
                        int oldIndex = OldIndexList.IndexOf(obj);
                        int newIndex = this.IndexOf(obj);
                        if (oldIndex != newIndex)
                        {
                            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItem: obj, newIndex, oldIndex));
                        }
                    }
                }
                //OldIndexList no longer needed
                OldIndexList = null;
                movedItems = null;
            }
        }

        #region < Replace >

        /// <summary>
        /// Replace an item in the list.
        /// </summary>
        /// <param name="itemToReplace">Search for this item in the list. If found, replace it. If not found, return false. <paramref name="newItem"/> will not be added to the list. </param>
        /// <param name="newItem">This item will replace the <paramref name="itemToReplace"/>. If <paramref name="itemToReplace"/> was not found, this item does not get added to the list.</param>
        /// <returns>True if the <paramref name="itemToReplace"/> was found in the list and successfully replaced. Otherwise false.</returns>
        public virtual bool Replace(T itemToReplace, T newItem)
        {
            lock (LockObject)
            {
                if (!this.Contains(itemToReplace)) return false;
                return Replace(this.IndexOf(itemToReplace), newItem);
            }
        }

        /// <summary>
        /// Replace an item in the list
        /// </summary>
        /// <param name="index">Index of the item to replace</param>
        /// <param name="newItem">This item will replace the item at the specified <paramref name="index"/></param>
        /// <returns>True if the the item was successfully replaced. Otherwise throws. </returns>
        /// <exception cref="IndexOutOfRangeException"/>
        public virtual bool Replace(int index, T newItem)
        {
            lock (LockObject)
            {
                var old = InternalList;
                InternalList = InternalList.Replace(index, newItem, out bool replaced);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, old[index], index));
                return replaced;
            }
        }

        /// <summary>
        /// Replaces the items in this list with the items in supplied collection. If the collection has more items than will be removed from the list, the remaining items will be added to the list. <br/>
        /// EX: List has 10 items, collection has 5 items, index of 8 is specified (which is item 9 on 0-based index) -> Item 9 + 10 are replaced, and remaining 3 items from collection are added to the list.
        /// </summary>
        /// <param name="index">Index of the item to replace</param>
        /// <param name="collection">Collection of items to insert into the list.</param>
        /// <returns>True if the the collection was successfully inserted into the list. Otherwise throws.</returns>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public virtual bool Replace(int index, IEnumerable<T> collection)
        {
            lock (LockObject)
            {
                int collectionCount = collection.Count();
                int ItemsAfterIndex = this.Count - index;     // # of items in the list after the index
                int CountToReplace = collectionCount <= ItemsAfterIndex ? collectionCount : ItemsAfterIndex;  //# if items that will be replaced
                int AdditionalItems = collectionCount - CountToReplace; //# of additional items that will be added to the list.

                List<T> oldItemsList = InternalList.GetRange(index, CountToReplace);

                //Insert the collection
                InternalList = InternalList.Replace(index, collection, out bool replaced);

                //Notify
                if (ResetNotificationsOnly)
                    NotifyCollectionChanged();
                else
                {
                    List<T> insertedList = collection.ToList();
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems: insertedList.GetRange(0, CountToReplace), oldItems: oldItemsList, index));
                    if (AdditionalItems > 0)
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, insertedList.GetRange(CountToReplace, AdditionalItems)));
                }

                return true;
            }
        }

        #endregion

        #endregion

        #region < IList Implementation >

        /// <inheritdoc/>
        object IList.this[int index]
        {
            get => ((IList)InternalList)[index];
            set
            {
                if (value.GetType() == typeof(T)) throw new NotSupportedException("Incorrect object Type");
                this[index] = (T)value;
            }
        }

        /// <inheritdoc/>
        bool IList.IsFixedSize => ((IList)InternalList).IsFixedSize;

        /// <inheritdoc/>
        int IList.Add(object value)
        {
            if (value.GetType() == typeof(T)) throw new NotSupportedException("Incorrect object Type");
            Add((T)value);
            return IndexOf((T)value);
        }

        /// <inheritdoc/>
        bool IList.Contains(object value)
        {
            if (value.GetType() == typeof(T)) return false;
            return Contains((T)value);
        }

        /// <inheritdoc/>
        int IList.IndexOf(object value)
        {
            if (value.GetType() == typeof(T)) return -1;
            return IndexOf((T)value);
        }

        /// <inheritdoc/>
        void IList.Insert(int index, object value)
        {
            if (value.GetType() == typeof(T)) throw new NotSupportedException("Incorrect object Type");
            Insert(index, (T)value);
        }

        /// <inheritdoc/>
        void IList.Remove(object value)
        {
            if (value.GetType() == typeof(T)) throw new NotSupportedException("Incorrect object Type");
            Remove((T)value);
        }

        #endregion
    }

    
}
