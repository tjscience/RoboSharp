using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Threading;

namespace System.Collections.Generic
{
    /// <summary>
    /// Extends the Generic <see cref="List{T}"/> class with an event that will fire when the list is updated via standard list methods <para/>
    /// </summary>
    /// <typeparam name="T">Type of object the list will contain</typeparam>
    /// <remarks>
    /// This class is being provided by the RoboSharp DLL <br/>
    /// <see href="https://github.com/tjscience/RoboSharp/wiki/SelectionOptions"/>
    /// <see href="https://github.com/tjscience/RoboSharp/tree/dev/RoboSharp/ObservableList.cs"/> <br/>
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

        #region < Events >

        /// <summary> This event fires whenever the List's array is updated. </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Raise the <see cref="CollectionChanged"/> event. <br/>
        /// <para/>
        /// Override this method to provide post-processing of Added/Removed items within derived classes.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // This Syncronization code was taken from here: https://stackoverflow.com/a/54733415/12135042
            // This ensures that the CollectionChanged event is invoked from the proper thread, no matter where the change came from.

            if (SynchronizationContext.Current == _synchronizationContext)
            {
                // Execute the CollectionChanged event on the current thread
                CollectionChanged?.Invoke(this, e);
            }
            else
            {
                // Raises the CollectionChanged event on the creator thread
                _synchronizationContext.Send((callback) => CollectionChanged?.Invoke(this, e), null);
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

        #region < New Methods >

        /// <summary>
        /// Replace an item in the list.
        /// </summary>
        /// <param name="itemToReplace">Search for this item in the list. If found, replace it. If not found, return false. <paramref name="newItem"/> will not be added to the list. </param>
        /// <param name="newItem">This item will replace the <paramref name="itemToReplace"/>. If <paramref name="itemToReplace"/> was not found, this item does not get added to the list.</param>
        /// <returns>True if the <paramref name="itemToReplace"/> was found in the list and successfully replaced. Otherwise false.</returns>
        public virtual bool Replace(T itemToReplace, T newItem)
        {
            if (!this.Contains(itemToReplace)) return false;
            return Replace(this.IndexOf(itemToReplace), newItem);
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
            this[index] = newItem;
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
            int CountToReplace = collectionCount <= ItemsAfterIndex ? collectionCount : ItemsAfterIndex;  //# if items that will be replaced
            int AdditionalItems = collectionCount - CountToReplace; //# of additional items that will be added to the list.

            List<T> oldItemsList = this.GetRange(index, CountToReplace);

            //Insert the collection
            base.RemoveRange(index, CountToReplace);
            if (AdditionalItems > 0)
                base.AddRange(collection);
            else
                base.InsertRange(index, collection);

            List<T> insertedList = collection.ToList();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems: insertedList.GetRange(0, CountToReplace), oldItems: oldItemsList, index));
            if (AdditionalItems > 0)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, insertedList.GetRange(CountToReplace, AdditionalItems)));
            return true;
        }

        #endregion

        #region < Methods that Override List<T> Methods >

        /// <summary>
        /// Get or Set the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to Get or Set.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        new public T this[int index] {
            get => base[index];
            set {
                if (index >= 0 && index < Count)
                {
                    //Perform Replace
                    T old = base[index];
                    base[index] = value;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem: value, oldItem: old, index));
                }
                else if (index == Count && index <= Capacity - 1)
                {
                    //Append value to end only if the capacity doesn't need to be changed
                    base.Add(value);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
                }
                else
                {
                    base[index] = value; // Generate ArgumentOutOfRangeException exception
                }
            }
        }

        #region < Add >

        ///<inheritdoc cref="List{T}.Add(T)"/>
        new public virtual void Add(T item)
        {
            base.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        ///<inheritdoc cref="System.Collections.Generic.List{T}.AddRange(IEnumerable{T})"/>
        new public virtual void AddRange(IEnumerable<T> collection)
        {
            if (collection == null || collection.Count() == 0) return;
            base.AddRange(collection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToList()));
        }

        #endregion

        #region < Insert >

        ///<inheritdoc cref="List{T}.Insert(int, T)"/>
        ///<remarks> Generates <see cref="CollectionChanged"/> event for item that was added and item that was shifted ( Event is raised twice ) </remarks>
        new public virtual void Insert(int index, T item)
        {
            base.Insert(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index: index));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this[index + 1], index + 1, index));
        }

        ///<inheritdoc cref="List{T}.InsertRange(int, IEnumerable{T})"/>
        ///<remarks> Generates <see cref="CollectionChanged"/> event for items that were added and items that were shifted ( Event is raised twice )</remarks>
        new public virtual void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null || collection.Count() == 0) return;
            int i = index + collection.Count() < this.Count ? collection.Count() : this.Count - index;
            List<T> movedItems = base.GetRange(index, i);
            base.InsertRange(index, collection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems: collection.ToList(), index));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItems: movedItems, IndexOf(movedItems[0]), index));
        }

        #endregion

        #region < Remove >

        ///<inheritdoc cref="List{T}.Clear"/>
        new public virtual void Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        ///<inheritdoc cref="List{T}.Remove(T)"/>
        new public virtual bool Remove(T item)
        {
            if (!base.Contains(item)) return false;

            int i = base.IndexOf(item);
            if (base.Remove(item))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, i));
                return true;
            }
            else
                return false;
        }

        ///<inheritdoc cref="List{T}.RemoveAt(int)"/>
        new public virtual void RemoveAt(int index)
        {
            T item = base[index];
            base.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index: index));
        }

        ///<inheritdoc cref="List{T}.RemoveRange(int,int)"/>
        new public virtual void RemoveRange(int index, int count)
        {
            List<T> removedItems = base.GetRange(index, count);
            if (removedItems.Count > 0)
            {
                base.RemoveRange(index, count);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems.ToList(), index));
            }
        }

        ///<inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        new public virtual int RemoveAll(Predicate<T> match)
        {
            List<T> removedItems = base.FindAll(match);
            int ret = removedItems.Count;
            if (ret > 0)
            {
                ret = base.RemoveAll(match);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
            }
            return ret;
        }

        #endregion

        #region < Move / Sort >

        #region < Hide base members >

        ///<inheritdoc cref="Sort(int, int, IComparer{T},bool)"/>
        new virtual public void Sort(int index, int count, IComparer<T> comparer) => Sort(index, count, comparer, true);
        ///<inheritdoc cref="Sort(IComparer{T}, bool)"/>
        new virtual public void Sort(IComparer<T> comparer) => Sort(comparer, true);
        ///<inheritdoc cref="Sort(Comparison{T},bool)"/>
        new virtual public void Sort(Comparison<T> comparison) => Sort(comparison, true);
        ///<inheritdoc cref="Sort(bool)"/>
        new virtual public void Sort() => Sort(true);
        ///<inheritdoc cref="Reverse(int, int, bool)"/>
        new virtual public void Reverse(int index, int count) => Reverse(index, count, true);
        ///<inheritdoc cref="Reverse(bool)"/>
        new virtual public void Reverse() => Reverse(true);

        #endregion

        ///<inheritdoc cref="List{T}.Reverse()"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Reverse(bool verbose)
        {
            PerformMove(new Action(() => base.Reverse()), this, verbose);
        }

        ///<inheritdoc cref="List{T}.Reverse(int, int)"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Reverse(int index, int count, bool verbose)
        {
            List<T> OriginalOrder = base.GetRange(index, count);
            PerformMove(new Action(() => base.Reverse(index, count)), OriginalOrder, verbose);
        }

        ///<inheritdoc cref="List{T}.Sort()"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(bool verbose)
        {
            PerformMove(new Action(() => base.Sort()), this, verbose);
        }

        ///<inheritdoc cref="List{T}.Sort(Comparison{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(Comparison<T> comparison, bool verbose)
        {
            PerformMove(new Action(() => base.Sort(comparison)), this, verbose);
        }

        ///<inheritdoc cref="List{T}.Sort(IComparer{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(IComparer<T> comparer, bool verbose)
        {
            PerformMove(new Action(() => base.Sort(comparer)), this, verbose);
        }

        ///<inheritdoc cref="List{T}.Sort(int, int, IComparer{T})"/>
        ///<inheritdoc cref="PerformMove"/>
        public virtual void Sort(int index, int count, IComparer<T> comparer, bool verbose)
        {
            List<T> OriginalOrder = base.GetRange(index, count);
            Action action = new Action(() => base.Sort(index, count, comparer));
            PerformMove(action, OriginalOrder, verbose);
        }

        /// <remarks>
        /// Per <see cref="INotifyCollectionChanged"/> rules, generates <see cref="CollectionChanged"/> event for every item that has moved within the list. <br/>
        /// Set <paramref name="verbose"/> parameter in overload to generate a single <see cref="NotifyCollectionChangedAction.Reset"/> event instead.
        /// </remarks>
        /// <param name="MoveAction">Action to perform that will rearrange items in the list - should not add, remove or replace!</param>
        /// <param name="OriginalOrder">List of items that are intended to rearrage - can be whole or subset of list</param>
        /// <param name="verbose">
        /// If TRUE: Create a 'Move' OnCollectionChange event for all items that were moved within the list. <para/>
        /// If FALSE: Generate a single event with <see cref="NotifyCollectionChangedAction.Reset"/>
        /// </param>
        protected void PerformMove(Action MoveAction, List<T> OriginalOrder, bool verbose)
        {
            //Store Old List Order
            List<T> OldIndexList = this.ToList();

            //Perform the move
            MoveAction.Invoke();

            //Generate the event
            foreach (T obj in OriginalOrder)
            {
                int oldIndex = OldIndexList.IndexOf(obj);
                int newIndex = this.IndexOf(obj);
                if (oldIndex != newIndex)
                {
                    if (verbose)
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItem: obj, newIndex, oldIndex));
                    else
                    {
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        break;
                    }
                }
            }

            //OldIndexList no longer needed
            OldIndexList.Clear();
        }

        #endregion

        #endregion
    }
}
