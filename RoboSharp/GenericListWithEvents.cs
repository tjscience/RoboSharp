using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    /// <summary>
    /// Extends the Generic <see cref="List{T}"/> class with an event that will fire when the list is updated.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListWithEvents<T> : List<T>
    {
        #region < Constructors >

        ///<inheritdoc cref="List{T}()"/>
        public ListWithEvents() : base() { }

        ///<inheritdoc cref="List{T}(int)"/>
        public ListWithEvents(int capacity) : base(capacity) { }

        ///<inheritdoc cref="List{T}(IEnumerable{T})"/>
        public ListWithEvents(IEnumerable<T> collection) : base(collection) { }

        #endregion

        #region < Properties >

        #endregion

        #region < Events >

        /// <summary>
        /// Event Args for the <see cref="ListWithEvents{T}.ListModification"/> event. <br/>
        /// Reports what items have been added / removed from the list.
        /// </summary>
        public class ListModificationEventArgs : EventArgs
        {
            /// <summary>Create new instance of <see cref="ListModificationEventArgs"/></summary>
            /// <param name="Item">Single Item Added/Removed from the list</param>
            /// <param name="Added">
            /// TRUE: Item(s) added to the <see cref="ItemsAdded"/> property. <br/>
            /// FALSE: Item(s) added to the <see cref="ItemsRemoved"/> property. <br/>
            /// </param>
            public ListModificationEventArgs(T Item, bool Added) 
            { 
                this.ItemsAdded = Added ? new T[] { Item } : new T[] { };
                this.ItemsRemoved = !Added ? new T[] { Item } : new T[] { };
            }

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do) -> Inherits the parameter description, but VS isn't smart enough to see it
            /// <param name="collection">Collection of items added / removed from the list.</param>
            /// <inheritdoc cref="ListModificationEventArgs.ListModificationEventArgs(T, bool)"/>            
            public ListModificationEventArgs(IEnumerable<T> collection, bool Added)
            { 
                this.ItemsAdded = Added ? collection.ToArray() : new T[] { };
                this.ItemsRemoved = !Added ? collection.ToArray() : new T[] { };
            }
#pragma warning restore CS1573

            /// <summary> Array of items that were added to the list. </summary>
            public T[] ItemsAdded { get; private set; }

            /// <summary> Array of items that were removed from the list. </summary>
            public T[] ItemsRemoved { get; private set; }
        }

        /// <summary>
        /// Raise the <see cref="ListModification"/> event. <para/>
        /// Override this method to provide post-processing of Added/Removed items within derived classes. <br/>
        /// <see cref="HasEventListener_ListModification"/> may need to be overridden to allow post-processing.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnListModification(ListModificationEventArgs e) => ListModification?.Invoke(this, e);

        /// <summary>
        /// Boolean check that optimizes the Remove*/Clear methods if the ListModification Event has no listeners. (No need to collect information on removed items if not reacting to the event)
        /// <para/>Override this method if you always wish to grab the removed items for processing during <see cref="OnListModification(ListModificationEventArgs)"/>
        /// </summary>
        /// <returns>Result of: ( <see cref="ListModification"/> != null ) </returns>
        protected virtual bool HasEventListener_ListModification() { return ListModification != null; }

        /// <summary> 
        /// This event fires whenever the List's array is updated. 
        /// <br/> The event will not fire when a new list is constructed/initialized, only when items are added or removed.
        /// </summary>
        public event EventHandler<ListModificationEventArgs> ListModification;

        #endregion

        #region < Methods that Override List<T> Methods >

        ///<inheritdoc cref="List{T}.Insert(int, T)"/>
        new public void Insert(int index, T item)
        {
            base.Insert(index, item);
            OnListModification(new ListModificationEventArgs(item, true));
        }

        ///<inheritdoc cref="List{T}.Add(T)"/>
        new public void Add(T item)
        {
            base.Add(item);
            OnListModification(new ListModificationEventArgs(item, true));
        }

        ///<inheritdoc cref="System.Collections.Generic.List{T}.AddRange(IEnumerable{T})"/>
        new public void AddRange(IEnumerable<T> collection)
        {
            base.AddRange(collection);
            OnListModification(new ListModificationEventArgs(collection, true));
        }

        ///<inheritdoc cref="List{T}.Clear"/>
        new public void Clear()
        {
            T[] arglist = HasEventListener_ListModification() ? base.ToArray() : null;
            base.Clear();
            OnListModification(new ListModificationEventArgs(arglist, false));
        }

        ///<inheritdoc cref="List{T}.Remove(T)"/>
        new public bool Remove(T item)
        {
            if (base.Remove(item))
            {
                OnListModification(new ListModificationEventArgs(item, false));
                return true;
            }
            else
                return false;
        }

        ///<inheritdoc cref="List{T}.RemoveAt(int)"/>
        new public void RemoveAt(int index)
        {
            T item = HasEventListener_ListModification() ? base[index] : default;
            base.RemoveAt(index);
            OnListModification(new ListModificationEventArgs(item, false));
        }

        ///<inheritdoc cref="List{T}.RemoveRange(int,int)"/>
        new public void RemoveRange(int index, int count)
        {
            List<T> arglist = HasEventListener_ListModification() ? base.GetRange(index, count) : null;
            base.RemoveRange(index, count);
            OnListModification(new ListModificationEventArgs(arglist, false));
        }

        #endregion
    }
}
