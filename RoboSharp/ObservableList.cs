using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp
{
    /// <summary>
    /// ObservableList is no longer available. Its functionality has been moved to <see cref="ConcurrentList{T}"/>.<br/>
    /// - The <see cref="ConcurrentList{T}"/> is still safe to use for non-UI related observable lists.
    /// - The object appeared to be safe for WPF for infrequent list updates, but was would throw an exception if a modification was being performed
    /// at the same time the UI Dispatcher was processing a CollectionChanged notification. <br/>
    /// - As such, this has been deprecated to notify consumers that a safer option should be used for ViewModels. Refer to the link below regarding this change:
    /// <br/><see href="https://github.com/tjscience/RoboSharp/issues/165"/>
    /// </summary>
    [Obsolete("" +
        "The ObservableList previously provided by RoboSharp was found not to be safe for User-Interface scenarios, specifically WPF bindings." +
        "\nThis object is now available as RoboSharp.ConcurrentList<T>. ConcurrentList<T> is thread-safe and will still raise NotifyCollectionChanged, but is not recommended for use as a WPF Binding"
        , error: true
        )]
    public class ObservableList<T>
    { }
}
