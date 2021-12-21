using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

namespace System.Collections.Generic
{
    /// <summary>
    /// Extends the Generic <see cref="List{T}"/> class with an event that will fire when the list is updated.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        #region < Methods that Override List<T> Methods >

        ///<inheritdoc cref="List{T}.Insert(int, T)"/>
        new public void Insert(int index, T item)
        {
            base.Insert(index, item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index: index));
        }

        ///<inheritdoc cref="List{T}.Add(T)"/>
        new public void Add(T item)
        {
            base.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        ///<inheritdoc cref="System.Collections.Generic.List{T}.AddRange(IEnumerable{T})"/>
        new public void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToList()));
        }

        ///<inheritdoc cref="List{T}.Clear"/>
        new public void Clear()
        {
            T[] removedItems = HasEventListener_CollectionChanged() ? base.ToArray() : null;
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, newItems: new List<T>(), oldItems: removedItems.ToList()));
        }

        ///<inheritdoc cref="List{T}.Remove(T)"/>
        new public bool Remove(T item)
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
        new public void RemoveAt(int index)
        {
            T item = HasEventListener_CollectionChanged() ? base[index] : default;
            base.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index: index));
        }

        ///<inheritdoc cref="List{T}.RemoveRange(int,int)"/>
        new public void RemoveRange(int index, int count)
        {
            List<T> removedItems = HasEventListener_CollectionChanged() ? base.GetRange(index, count) : null;
            base.RemoveRange(index, count);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, newItems: new List<T>(), oldItems: removedItems.ToList()));
        }

        #endregion
    }
}
