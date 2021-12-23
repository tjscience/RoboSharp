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
        /// Indicates if a task is currently running or paused. <br/>
        /// When true, prevents starting new tasks and prevents modication of the list.
        /// </summary>
        public bool IsRunning => isDisposing || IsCopyOperationRunning || IsListOnlyRunning;

        /// <summary>
        /// This is set true when <see cref="PauseAll"/> is called while any of the items in the list were running, and set false when <see cref="ResumeAll"/> or <see cref="StopAll"/> is called.
        /// </summary>
        public bool IsPaused { get; private set; }

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

        #region < Methods >

        /// <summary>
        /// Create a new instance of the <see cref="ListOnlyResults"/> object
        /// </summary>
        /// <returns>New instance of the <see cref="ListOnlyResults"/> list.</returns>
        public RoboCopyResultsList GetListOnlyResults() => new RoboCopyResultsList(ListOnlyResults);

        /// <summary>
        /// Create a new instance of the <see cref="RunOperationResults"/> object
        /// </summary>
        /// <returns>New instance of the <see cref="RunOperationResults"/> list.</returns>
        public RoboCopyResultsList GetRunOperationResults() => new RoboCopyResultsList(RunOperationResults);

        /// <summary>
        /// Run <see cref="RoboCommand.Stop"/> against all items in the list.
        /// </summary>
        public void StopAll()
        {
            CommandList.ForEach((c) => c.Stop());
            IsCopyOperationRunning = false;
            IsListOnlyRunning = false;
            IsPaused = false;
        }

        /// <summary>
        /// Loop through the items in the list and issue <see cref="RoboCommand.Pause"/> on any commands where <see cref="RoboCommand.IsRunning"/> is true.
        /// </summary>
        public void PauseAll()
        {
            IsPaused = AnyRunning;
            CommandList.ForEach((c) => { if (c.IsRunning) c.Pause(); });
        }

        /// <summary>
        /// Loop through the items in the list and issue <see cref="RoboCommand.Resume"/> on any commands where <see cref="RoboCommand.IsPaused"/> is true.
        /// </summary>
        public void ResumeAll()
        {
            IsPaused = false;
            CommandList.ForEach((c) => { if (c.IsPaused) c.Resume(); });
        }

        #endregion

        #region < Run List-Only Mode >

        /// METHOD IS WORK IN PROGRESS
        /// 
        public Task StartAll_ListOnly_Parrallel(string domain = "", string username = "", string password = "") => ListOnlyTask(domain, username, password, true);

        /// METHOD IS WORK IN PROGRESS
        /// <inheritdoc cref="StartMethodParameterTooltips"/>
        public Task StartAll_ListOnly_Synchronous(string domain = "", string username = "", string password = "") => ListOnlyTask(domain, username, password, false);

        /// <summary>
        /// Generate the return task when running in ListOnly mode
        /// </summary>
        /// <returns></returns>
        private Task ListOnlyTask(string domain, string username, string password, bool RunInParrallel )
        {
            IsListOnlyRunning = true;
            ListOnlyResults.Clear();
            //Store the setting for ListOnly prior to changing it
            List<Tuple<RoboCommand, bool>> OldListValues = new List<Tuple<RoboCommand, bool>>();
            CommandList.ForEach((c) => {
                OldListValues.Add(new Tuple<RoboCommand, bool>(c, c.LoggingOptions.ListOnly));
                c.LoggingOptions.ListOnly = true;
            });
            //Run the commands
            Task<RoboCopyResultsList> Run = RunInParrallel ? RunParallel(domain, username, password) : RunSynchronous(domain, username, password);
            Task ResultsTask = Run.ContinueWith((continuation) =>
            {
                //Store the results then restore the ListOnly values
                ListOnlyResults.AddRange(Run.Result);
                foreach (var obj in OldListValues)
                    obj.Item1.LoggingOptions.ListOnly = obj.Item2;
                //Set Flags
                IsListOnlyRunning = false;
                IsPaused = false;
            }
            );
            return ResultsTask;
        }

        #endregion

        #region < Run User-Set Parameters >

        /// METHOD IS WORK IN PROGRESS
        /// <inheritdoc cref="StartMethodParameterTooltips"/>
        public Task StartAll_Parrallel(string domain = "", string username = "", string password = "") => RunOperationTask(domain, username, password, true);

        /// <summary>
        /// METHOD IS WORK IN PROGRESS
        /// </summary>
        /// <returns></returns>
        /// <inheritdoc cref="StartMethodParameterTooltips"/>
        public Task StartAll_Synchronous(string domain = "", string username = "", string password = "") => RunOperationTask(domain, username, password, false);


        private Task RunOperationTask(string domain, string username, string password, bool RunInParrallel)
        {
            IsCopyOperationRunning = true;
            RunOperationResults.Clear();
            Task<RoboCopyResultsList> Run = RunInParrallel ? RunParallel(domain, username, password) : RunSynchronous(domain, username, password);
            Task ResultsTask = Run.ContinueWith((continuation) =>
            {
                RunOperationResults.AddRange(Run.Result);
                IsCopyOperationRunning = false;
                IsPaused = false;
            }
            );
            return ResultsTask;
        }
    
        

        #endregion

        #region < Private Start Methods >

        /// <summary>
        /// Loop through all <see cref="RoboCommand"/> objects in the list. Operations will run one at a time. ( The first operation must finish prior to the second operation starting )
        /// </summary>
        /// <returns> Returns new task that runs all the commands in the list consecutively </returns>
        /// <inheritdoc cref="StartMethodParameterTooltips"/>
        private Task<RoboCopyResultsList> RunSynchronous(string domain = "", string username = "", string password = "")
        {
            Debugger.Instance.DebugMessage("Starting Synchronous execution of RoboCommandList");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            RoboCopyResultsList returnList = new RoboCopyResultsList();

            Task LoopTask = Task.Factory.StartNew(() =>
            {
                //Start the task, wait for it to finish, move on to the next task
                foreach (RoboCommand cmd in CommandList)
                {
                    Task RunTask = cmd.Start(domain, username, password);
                    RunTask.Wait();
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, PriorityScheduler.BelowNormal);

            Task<RoboCopyResultsList> ContinueWithTask = LoopTask.ContinueWith((continuation) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    //If cancellation was requested -> Issue the STOP command to all commands in the list
                    Debugger.Instance.DebugMessage("RunSynchronous Task Was Cancelled");
                    StopAll();
                }
                else
                {
                    Debugger.Instance.DebugMessage("RunSynchronous Task Completed");
                }
                CommandList.ForEach((c) => returnList.Add(c.GetResults())); //Loop through the list, adding the results of each command to the list
                return returnList;

            });

            return ContinueWithTask;
        }


        /// <summary>
        /// Generates a task that runs all the commands in parallel. Once all operations are complete, the returned task will resolve.
        /// </summary>
        /// <returns>Returns a new task that will finish after all commands have completed their tasks. -> Task.WhenAll() </returns>
        /// <inheritdoc cref="StartMethodParameterTooltips"/>
        private Task<RoboCopyResultsList> RunParallel(string domain = "", string username = "", string password = "")
        {
            Debugger.Instance.DebugMessage("Starting Parallel execution of RoboCommandList");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            RoboCopyResultsList returnList = new RoboCopyResultsList();
            List<Task> TaskList = new List<Task>();

            //Start all commands, adding each one to the TaskList
            foreach (RoboCommand cmd in CommandList)
            {
                TaskList.Add(cmd.Start(domain, username, password));
            }

            //Create Task.WhenAll() to wait for all robocommands to run to completion
            Task WhenAll = Task.Factory.StartNew(() => Task.WaitAll(TaskList.ToArray()), cancellationToken, TaskCreationOptions.LongRunning, PriorityScheduler.BelowNormal);

            Task<RoboCopyResultsList> ContinueWithTask = WhenAll.ContinueWith((continuation) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    //If cancellation was requested -> Issue the STOP command to all commands in the list
                    Debugger.Instance.DebugMessage("RunParrallel Task Was Cancelled");
                    StopAll();
                }
                else
                {
                    Debugger.Instance.DebugMessage("RunParrallel Task Completed");
                }

                CommandList.ForEach((c) => returnList.Add(c.GetResults())); //Loop through the list, adding the results of each command to the list

                return returnList;
            });

            return ContinueWithTask;
        }


#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        /// <remarks> <paramref name="domain"/>, <paramref name="password"/>, and <paramref name="username"/> are applied to all RoboCommand objects during this run. </remarks>
        /// <inheritdoc cref="RoboCommand.Start(string, string, string)"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used for XML ToolTips on other methods")]
        private void StartMethodParameterTooltips(string domain, string username, string password, CancellationTokenSource cancellationTokenSource) { } //This method exists primarily to avoid introducing repetetive xml tags for all the exposed Start() methods
#pragma warning restore CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

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
