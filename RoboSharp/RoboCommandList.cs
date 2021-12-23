using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RoboSharp.Results;

namespace RoboSharp
{
    /// <summary>
    /// Contains a private List{RoboCommand} object with controlled methods for access to it.  Implements the following: <br/>
    /// <see cref="IEnumerable"/> <br/>
    /// <see cref="IDisposable"/>
    /// </summary>
    public sealed class RoboCommandList : IEnumerable, IDisposable
    {
        #region < Constructors >

        /// <summary>
        /// Initialize a new (empty) <see cref="RoboCommandList"/> object.
        /// </summary>
        public RoboCommandList()
        {

        }

        /// <summary>
        /// Initialize a new <see cref="RoboCommandList"/> object that contains the supplied <see cref="RoboCommand"/>
        /// </summary>
        public RoboCommandList(RoboCommand roboCommand)
        {
            CommandList.Add(roboCommand);
        }

        /// <summary>
        /// Initialize a new <see cref="RoboCommandList"/> object that contains the supplied <see cref="RoboCommand"/> collection
        /// </summary>
        public RoboCommandList(IEnumerable<RoboCommand> roboCommands)
        {
            CommandList.AddRange(roboCommands);
        }

        #endregion

        #region < Fields >

        private readonly List<RoboCommand> CommandList = new List<RoboCommand>();
        private Task ListTask;
        private Task RunTask;
        private bool disposedValue;
        private bool isDisposing;

        #endregion

        #region < Properties >

        /// <summary> 
        /// Indicates if a task is currently running. <br/>
        /// When true, prevents starting new tasks and prevents modication of the list.
        /// </summary>
        public bool IsRunning => isDisposing || IsCopyOperationRunning || IsListOnlyRunning;

        /// <summary> Indicates if the StartAll task is currently running. </summary>
        public bool IsCopyOperationRunning { get; private set; }

        /// <summary> Indicates if the ListOnly task is currently running. </summary>
        public bool IsListOnlyRunning { get; private set; }

        /// <summary> Indicates if the ListOnly() operation has been completed. </summary>
        public bool ListOnlyCompleted { get; private set; }

        /// <summary> Indicates if the StartAll() operation has been completed. </summary>
        public bool CopyOperationCompleted { get; private set; }

        /// <summary> Checks <see cref="RoboCommand.IsRunning"/> property of all items in the list. </summary>
        public bool AnyRunning => CommandList.Any(c => c.IsRunning);

        /// <summary> Checks <see cref="RoboCommand.IsPaused"/> property of all items in the list. </summary>
        public bool AnyPaused => CommandList.Any(c => c.IsPaused);

        /// <summary> Checks <see cref="RoboCommand.IsCancelled"/> property of all items in the list. </summary>
        public bool AnyCancelled => CommandList.Any(c => c.IsCancelled);

        /// <summary>
        /// This list will be cleared and repopulated every time the <see cref="StartAll"/> method is called. If this object is disposed, this list will be as well. <br/>
        /// To store these results for future use, call <see cref="GetListOnlyResults"/>.
        /// </summary>
        public RoboCopyResultsList ListOnlyResults { get; } = new RoboCopyResultsList();

        /// <summary>
        /// This list will be cleared and repopulated every time the <see cref="StartAll"/> method is called. If this object is disposed, this list will be as well. <br/>
        /// To store these results for future use, call <see cref="GetRunOperationResults"/>.
        /// </summary>
        public RoboCopyResultsList RunOperationResults { get; } = new RoboCopyResultsList();

        #endregion

        #region < IDisposable >

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                isDisposing = true; //Set flag to prevent code modifying the list during disposal 

                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    ListOnlyResults.Dispose();
                    RunOperationResults.Dispose();
                }

                //RoboCommand objects attach to a process, so must be in the 'unmanaged' section.
                foreach (RoboCommand cmd in CommandList)
                    cmd.Dispose();
                CommandList.Clear();

                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizer -> Ensures that all RoboCommand objects get disposed of properly when program exits
        /// </summary>
        ~RoboCommandList()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>
        /// Dispose all RoboCommand objects contained in the list. <br/>
        /// Also disposes <see cref="ListOnlyResults"/> and <see cref="RunOperationResults"/>
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region < List Access Methods >

        /// <summary>
        /// Gets the enumerator for the private list of <see cref="RoboCommand"/> objects
        /// </summary>
        /// <remarks>
        /// Can be used to iterate through the list of commands while <see cref="IsRunning"/> == true, but is not recomended.
        /// </remarks>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)CommandList).GetEnumerator();
        }

        /// <summary>
        /// Exception thrown when attempting to run a method accesses the list backing a RoboCommandList object while the tasks are in progress.
        /// </summary>
        public class ListAccessDeniedException : Exception
        {
            /// <remarks>This functionality is disabled if <see cref="IsRunning"/> == true.</remarks>
            /// <exception cref="ListAccessDeniedException"/>
            private const string StandardMsg = "Running methods that modify the list of RoboCommands methods while RoboCommandList.IsRunning = TRUE is prohibited.";
            internal ListAccessDeniedException() : base(StandardMsg) { }
            internal ListAccessDeniedException(string message) : base($"{StandardMsg}\n{message}") { }
            internal ListAccessDeniedException(string message, Exception innerException) : base(message, innerException) { }
        }


        /// <inheritdoc cref="List{T}.ForEach(Action{T})"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void ForEach(Action<RoboCommand> action)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.Add(T)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void AddCommand(RoboCommand item)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.Add(item);
        }

        /// <inheritdoc cref="List{T}.Insert(int, T)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void AddCommand(int index, RoboCommand item)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.Insert(index, item);
        }

        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void AddCommand(IEnumerable<RoboCommand> collection)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.AddRange(collection);
        }

        /// <inheritdoc cref="List{T}.Remove(T)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void RemoveCommand(RoboCommand item)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.Remove(item);
        }

        /// <inheritdoc cref="List{T}.RemoveAt(int)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void RemoveCommand(int index)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.RemoveAt(index);
        }

        /// <inheritdoc cref="List{T}.RemoveRange(int, int)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void RemovCommand(int index, int count)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.RemoveRange(index, count);
        }

        /// <inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void RemovCommand(Predicate<RoboCommand> match)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.RemoveAll(match);
        }

        /// <inheritdoc cref="List{T}.Clear"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void ClearCommandList()
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.Clear();
        }

        #endregion

    }
}
