using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

namespace System.Collections.Generic
{
    /// <summary>
    /// Extends the Generic <see cref="List{T}"/> class with an event that will fire when the list is updated. <para/>
    /// Note: Replacing an item using  <c>List[index]={T}</c> will not trigger the <see cref="INotifyCollectionChanged"/> event. Use the { Replace } methods instead to trigger the event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This class is being provided by the RoboSharp DLL <br/>
    /// <see href="https://github.com/tjscience/RoboSharp/tree/dev/RoboSharp/GenericListWithEvents.cs"/>
    /// </remarks>
    public class ObservableList<T> : List<T>, INotifyCollectionChanged
    {
        #region < Constructors >

        ///<inheritdoc cref="List{T}()"/>
        public ObservableList() : base() { }

        ///<inheritdoc cref="List{T}(int)"/>
        public ObservableList(int capacity) : base(capacity) { }

        ///<inheritdoc cref="List{T}(IEnumerable{T})"/>
        public ObservableList(IEnumerable<T> collection) : base(collection) { }

        #endregion

        #region < Properties >

        #endregion

        #region < Events >

        /// <summary>
        /// Raise the <see cref="CollectionChanged"/> event. <para/>
        /// Override this method to provide post-processing of Added/Removed items within derived classes. <br/>
        /// <see cref="HasEventListener_CollectionChanged"/> may need to be overridden to allow post-processing.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) => CollectionChanged?.Invoke(this, e);

        /// <summary>
        /// Boolean check that optimizes the Remove*/Clear methods if the CollectionChanged Event has no listeners. (No need to collect information on removed items if not reacting to the event)
        /// <para/>Override this method if you always wish to grab the removed items for processing during <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)"/>
        /// </summary>
        /// <returns>Result of: ( <see cref="CollectionChanged"/> != null ) </returns>
        protected virtual bool HasEventListener_CollectionChanged() { return CollectionChanged != null; }

        /// <summary> This event fires whenever the List's array is updated. </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region < New Methods >

        /// <summary>
        /// Replace an item in the list and trigger a <see cref="INotifyCollectionChanged"/> event.
        /// </summary>
        /// <param name="itemToReplace">Search for this item in the list. If found, replace it.</param>
        /// <param name="newItem">This item will replace the <paramref name="itemToReplace"/>. If <paramref name="itemToReplace"/> was not found, this item does not get added to the list.</param>
        /// <returns>True if the <paramref name="itemToReplace"/> was found in the list and successfully replaced. Otherwise false.</returns>
        public virtual bool Replace(T itemToReplace, T newItem)
        {
            if (!this.Contains(itemToReplace)) return false;
            return Replace(this.IndexOf(itemToReplace), newItem);
        }


        /// <summary>
        /// Replace an item in the list and trigger a <see cref="INotifyCollectionChanged"/> event. <br/>
        /// </summary>
        /// <param name="index">Index of the item to replace</param>
        /// <param name="newItem">This item will replace the item at the specified <paramref name="index"/></param>
        /// <returns>True if the the item was successfully replaced. Otherwise will throw due to IndexOutOfRange.</returns>
        /// <exception cref="IndexOutOfRangeException"/>
        public virtual bool Replace(int index, T newItem)
        {
            T oldItem = this[index];
            this[index] = newItem;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems: new List<T> { newItem }, oldItems: new List<T> { oldItem }));
            return true;
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
            int collectionCount = collection.Count();
            int ItemsAfterIndex = this.Count - index;     // # of items in the list after the index
            int CountToReplace = collectionCount >= ItemsAfterIndex ? ItemsAfterIndex : ItemsAfterIndex - collectionCount;  //# if items that will be replaced
            int AdditionalItems = collectionCount - CountToReplace; //# of additional items that will be added to the list.

            List<T> oldItemsList = this.GetRange(index, CountToReplace);

            //Insert the collection
            base.RemoveRange(index, CountToReplace);
            if (AdditionalItems > 0)
                base.AddRange(collection);
            else
                base.InsertRange(index, collection);
            
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems: collection.ToList(), oldItems: oldItemsList, index));
            return true;
        }

        #endregion

        #region < Methods that Override List<T> Methods >

        #region < Add >

        ///<inheritdoc cref="List{T}.Insert(int, T)"/>
        new public virtual void Insert(int index, T item)
        {
            base.Insert(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index: index));
        }

        ///<inheritdoc cref="List{T}.Add(T)"/>
        new public virtual void Add(T item)
        {
            base.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        ///<inheritdoc cref="System.Collections.Generic.List{T}.AddRange(IEnumerable{T})"/>
        new public virtual void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToList()));
        }


        #endregion

        #region < Remove >

        ///<inheritdoc cref="List{T}.Clear"/>
        new public virtual void Clear()
        {
            T[] removedItems = HasEventListener_CollectionChanged() ? base.ToArray() : new T[] { };
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        ///<inheritdoc cref="List{T}.Remove(T)"/>
        new public virtual bool Remove(T item)
        {
            if (base.Remove(item))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                return true;
            }
            else
                return false;
        }

        ///<inheritdoc cref="List{T}.RemoveAt(int)"/>
        new public virtual void RemoveAt(int index)
        {
            T item = HasEventListener_CollectionChanged() ? base[index] : default;
            base.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index: index));
        }

        ///<inheritdoc cref="List{T}.RemoveRange(int,int)"/>
        new public virtual void RemoveRange(int index, int count)
        {
            List<T> removedItems = HasEventListener_CollectionChanged() ? base.GetRange(index, count) : null;
            base.RemoveRange(index, count);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems.ToList(), index));
        }

        #endregion

        #region < Move >

        #region < Hide base members >

        ///<inheritdoc cref="List{T}.Sort(int, int, IComparer{T})"/>
        new public virtual void Sort(int index, int count, IComparer<T> comparer) => Sort(index, count, comparer, false);
        ///<inheritdoc cref="List{T}.Sort(IComparer{T})"/>
        new public virtual void Sort(IComparer<T> comparer) => Sort(comparer, false);
        ///<inheritdoc cref="List{T}.Sort(Comparison{T})"/>
        new public virtual void Sort(Comparison<T> comparison) => Sort(comparison, false);
        ///<inheritdoc cref="List{T}.Sort()"/>
        new public virtual void Sort() => Sort(false);
        ///<inheritdoc cref="List{T}.Reverse(int, int)"/>
        new public virtual void Reverse(int index, int count) => Reverse(index, count, false);
        ///<inheritdoc cref="List{T}.Reverse()"/>
        new public virtual void Reverse() => Reverse(false);

        #endregion

        ///<inheritdoc cref="List{T}.Reverse()"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Reverse(bool verbose = false)
        {
            PerformMove(new Action(() => base.Reverse()), this, verbose);
        }

        ///<inheritdoc cref="List{T}.Reverse(int, int)"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Reverse(int index, int count, bool verbose = false)
        {
            List<T> OriginalOrder = HasEventListener_CollectionChanged() ? base.GetRange(index, count) : null;
            PerformMove(new Action(() => base.Reverse(index, count)), OriginalOrder, verbose);
        }

        ///<inheritdoc cref="List{T}.Sort()"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(bool verbose = false)
        {
            PerformMove(new Action(() => base.Sort()), this, verbose);
        }

        ///<inheritdoc cref="List{T}.Sort(Comparison{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(Comparison<T> comparison, bool verbose = false)
        {
            PerformMove(new Action(() => base.Sort(comparison)), this, verbose);
        }

        ///<inheritdoc cref="List{T}.Sort(IComparer{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(IComparer<T> comparer, bool verbose = false)
        {
            PerformMove(new Action(() => base.Sort(comparer)), this, verbose);
        }

        ///<inheritdoc cref="List{T}.Sort(int, int, IComparer{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(int index, int count, IComparer<T> comparer, bool verbose = false)
        {
            List<T> OriginalOrder = HasEventListener_CollectionChanged() ? base.GetRange(index, count) : new List<T>();
            Action action = new Action( () => base.Sort(index, count, comparer));
            PerformMove(action, OriginalOrder, verbose);
        }

        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="MoveAction"/>
        /// <param name="OriginalOrder"/>
        /// <param name="verbose">
        /// Create a 'Move' OnCollectionChange event for all items that were moved within the list. <para/>
        /// If false, Triggers a single <see cref="NotifyCollectionChangedAction.Move"/> event. 
        /// </param>
        private void PerformMove(Action MoveAction, List<T>OriginalOrder, bool verbose)
        {
            List<MoveTracker> Tracker = new List<MoveTracker>();

            foreach (T obj in OriginalOrder)
                Tracker.Add(new MoveTracker(this, obj));

            MoveAction.Invoke();

            if (verbose)
            {
                foreach (MoveTracker objTracker in Tracker)
                    objTracker.Notify();
            }
            else
            {
                if (Tracker.Any( c => c.OldIndex != c.NewIndex))
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move));
            }
            Tracker.Clear();
            Tracker = null;
        }

        private struct MoveTracker
        {
            public MoveTracker(ObservableList<T> listRef, T obj) 
            {
                ListRef = listRef;
                Obj = obj;
                OldIndex = ListRef.IndexOf(obj);
            }

            private ObservableList<T> ListRef;
            public T Obj;
            public int OldIndex;
            public int NewIndex => ListRef.IndexOf(Obj);
            public void Notify()
            {
                if (OldIndex != NewIndex)
                    ListRef.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItem: Obj, NewIndex, OldIndex));
            }

        }

        #endregion


        #endregion
    }
}
