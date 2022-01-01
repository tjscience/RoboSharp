using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using RoboSharp.Results;
using System.Collections.ObjectModel;

namespace RoboSharp
{
    /// <summary>
    /// Contains a private List{RoboCommand} object with controlled methods for access to it.  
    /// <para/>Implements the following: <br/>
    /// <see cref="IEnumerable"/> -- Allow enumerating through the collection that is stored in a private list -- Also see <see cref="Commands"/> <br/>
    /// <see cref="INotifyCollectionChanged"/> -- Allow subscription to collection changes  against the list <see cref="ObservableList{T}"/> <br/>
    /// <see cref="INotifyPropertyChanged"/> -- Most properties will trigger <see cref="PropertyChanged"/> events when updated.<br/>
    /// <see cref="IDisposable"/> -- Allow disposal of all <see cref="RoboCommand"/> objects in the list.
    /// </summary>
    public sealed class RoboQueue : IDisposable, INotifyPropertyChanged, IEnumerable, INotifyCollectionChanged
    {
        #region < Constructors >

        /// <summary>
        /// Initialize a new (empty) <see cref="RoboQueue"/> object.
        /// </summary>
        public RoboQueue()
        {
            Init();
            Commands = new ReadOnlyCollection<RoboCommand>(CommandList);
        }
        
        /// <summary>
        /// Initialize a new (empty) <see cref="RoboQueue"/> object with a specificed Name.
        /// </summary>
        /// <inheritdoc cref="RoboQueue(IEnumerable{RoboCommand}, string, int)"/>
        public RoboQueue(string name, int maxConcurrentJobs = 1)
        {
            Init(name, maxConcurrentJobs);
            Commands = new ReadOnlyCollection<RoboCommand>(CommandList);
        }

        /// <summary>
        /// Initialize a new <see cref="RoboQueue"/> object that contains the supplied <see cref="RoboCommand"/>.
        /// </summary>
        /// <inheritdoc cref="RoboQueue(IEnumerable{RoboCommand}, string, int)"/>
        public RoboQueue(RoboCommand roboCommand, string name = "", int maxConcurrentJobs = 1)
        {
            CommandList.Add(roboCommand);
            Init(name, maxConcurrentJobs);
            Commands = new ReadOnlyCollection<RoboCommand>(CommandList);
        }

        /// <summary>
        /// Initialize a new <see cref="RoboQueue"/> object that contains the supplied <see cref="RoboCommand"/> collection.
        /// </summary>
        /// <param name="roboCommand">RoboCommand(s) to populate the list with.</param>
        /// <param name="name"><inheritdoc cref="Name"/></param>
        /// <param name="maxConcurrentJobs"><inheritdoc cref="MaxConcurrentJobs"/></param>
        public RoboQueue(IEnumerable<RoboCommand> roboCommand, string name = "", int maxConcurrentJobs = 1)
        {
            CommandList.AddRange(roboCommand);
            Init(name, maxConcurrentJobs);
            Commands = new ReadOnlyCollection<RoboCommand>(CommandList);
        }
        
        private void Init(string name = "", int maxConcurrentJobs = 1)
        {
            NameField = name;
            MaxConcurrentJobsField = maxConcurrentJobs;
        }

        #endregion

        #region < Fields >

        private readonly ObservableList<RoboCommand> CommandList = new ObservableList<RoboCommand>();
        private bool disposedValue;
        private bool isDisposing;
        private CancellationTokenSource TaskCancelSource;
        private int MaxConcurrentJobsField;
        private string NameField;

        private bool WasCancelledField = false;
        private bool IsPausedField = false;
        private bool IsCopyOperationRunningField = false;
        private bool IsListOperationRunningField = false;
        private bool ListOnlyCompletedField = false;
        private bool CopyOpCompletedField = false;

        #endregion

        #region < Properties >
        
        /// <summary>
        /// Name of this collection of RoboCommands
        /// </summary>
        public string Name { 
            get => NameField;
            private set {
                if (value != NameField) {
                    NameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        
        /// <summary>
        /// Wraps the private <see cref="ObservableList{T}"/> into a ReadOnlyCollection for public consumption and data binding.
        /// </summary>
        public ReadOnlyCollection<RoboCommand> Commands { get; }

        /// <summary>
        /// <inheritdoc cref="RoboCommand.ProgressEstimator"/>
        /// </summary>
        public ProgressEstimator ProgressEstimator { get; private set; }

        /// <summary> 
        /// Indicates if a task is currently running or paused. <br/>
        /// When true, prevents starting new tasks and prevents modication of the list.
        /// </summary>
        public bool IsRunning => isDisposing || IsCopyOperationRunning || IsListOnlyRunning;

        /// <summary>
        /// This is set true when <see cref="PauseAll"/> is called while any of the items in the list were running, and set false when <see cref="ResumeAll"/> or <see cref="StopAll"/> is called.
        /// </summary>
        public bool IsPaused {
            get => IsPausedField;
            private set {
                if (value != IsPausedField) {
                    IsPausedField = value;
                    OnPropertyChanged("IsPaused");
                }
            }
        }

        /// <summary>
        /// Flag is set to TRUE if the 'Stop' command is issued. Reset to False when starting a new operation.
        /// </summary>
        public bool WasCancelled
        {
            get => WasCancelledField;
            private set {
                if (value != WasCancelledField) {
                    WasCancelledField = value;  
                    OnPropertyChanged("WasCancelled");
                }
            }
        }

        /// <summary> Indicates if the StartAll task is currently running. </summary>
        public bool IsCopyOperationRunning { 
            get => IsCopyOperationRunningField;
            private set {
                if (value != IsCopyOperationRunningField)
                {
                    bool running = IsRunning;
                    IsCopyOperationRunningField = value;
                    OnPropertyChanged("IsCopyOperationRunning");
                    if (IsRunning != running) OnPropertyChanged("IsRunning");
                }
            }
        }

        /// <summary> Indicates if the StartAll_ListOnly task is currently running. </summary>
        public bool IsListOnlyRunning {
            get => IsListOperationRunningField; 
            private set {
                if (value != IsListOperationRunningField) {
                    bool running = IsRunning;
                    IsListOperationRunningField = value;
                    OnPropertyChanged("IsListOnlyRunning");
                    if (IsRunning != running) OnPropertyChanged("IsRunning");
                }
            } 
        }

        /// <summary> Indicates if the StartAll_ListOnly() operation has been completed. </summary>
        public bool ListOnlyCompleted {
            get => ListOnlyCompletedField;
            private set {
                if (value != ListOnlyCompletedField) {
                    ListOnlyCompletedField = value;
                    OnPropertyChanged("ListOnlyCompleted");
                }
            }
        }

        /// <summary> Indicates if the StartAll() operation has been completed. </summary>
        public bool CopyOperationCompleted {
            get => CopyOpCompletedField;
            private set {
                if (value != CopyOpCompletedField) {
                    CopyOpCompletedField = value;
                    OnPropertyChanged("CopyOperationCompleted");
                }
            }
        }

        /// <summary> 
        /// Checks <see cref="RoboCommand.IsRunning"/> property of all items in the list. 
        /// <br/> INotifyPropertyChanged is not raised when this property changes.
        /// </summary>
        public bool AnyRunning => CommandList.Any(c => c.IsRunning);

        /// <summary> 
        /// Checks <see cref="RoboCommand.IsPaused"/> property of all items in the list. 
        /// <br/> INotifyPropertyChanged is not raised when this property changes.
        /// </summary>
        public bool AnyPaused => CommandList.Any(c => c.IsPaused);

        /// <summary> 
        /// Checks <see cref="RoboCommand.IsCancelled"/> property of all items in the list. 
        /// <br/> INotifyPropertyChanged is not raised when this property changes.
        /// </summary>
        public bool AnyCancelled => CommandList.Any(c => c.IsCancelled);

        /// <summary> 
        /// Check the list and get the count of RoboCommands that are either in the 'Run' or 'Paused' state. <br/>
        /// (Paused state is included since these can be resumed at any time) 
        /// </summary>
        public int JobsCurrentlyRunning => CommandList.Where((C) => C.IsRunning | C.IsPaused).Count();

        /// <summary>
        /// Specify the max number of RoboCommands to execute at the same time. <br/>
        /// Set Value to 0 to allow infinite number of jobs (Will issue all start commands at same time) <br/>
        /// Default Value = 1; <br/>
        /// </summary>
        public int MaxConcurrentJobs { 
            get => MaxConcurrentJobsField;
            set {
                int newVal = value > 0 ? value : IsRunning & MaxConcurrentJobsField > 0 ? 1 : 0;  //Allow > 0 at all times //If running, set value to 1
                if (newVal != MaxConcurrentJobsField) {
                    MaxConcurrentJobsField = newVal;
                    OnPropertyChanged("MaxConcurrentJobs");
                }
            } 
        }
        
        /// <summary> Number of RoboCommands in the list </summary>
        public int ListCount => CommandList.Count;
        
        /// <summary>
        /// Report how many <see cref="RoboCommand.Start"/> tasks has completed during the run. <br/>
        /// This value is reset to 0 when a new run starts, and increments as each job exits.
        /// </summary>
        public int JobsComplete { get; private set; } = 0; //INotifyPropertyChange raised in the StartJobs Method

        /// <summary>
        /// This list will be cleared and repopulated when one of the ListOnly methods are called. If this object is disposed, this list will be as well. <br/>
        /// To store these results for future use, call <see cref="GetListOnlyResults"/>.
        /// </summary>
        public RoboCopyResultsList ListOnlyResults { get; } = new RoboCopyResultsList();

        /// <summary>
        /// This list will be cleared and repopulated when one of the ListOnly methods are called. If this object is disposed, this list will be as well. <br/>
        /// To store these results for future use, call <see cref="GetRunOperationResults"/>.
        /// </summary>
        public RoboCopyResultsList RunOperationResults { get; } = new RoboCopyResultsList();

        #endregion

        #region < Events >

        /// <inheritdoc cref="RoboCommand.OnFileProcessed"/>
        public event RoboCommand.FileProcessedHandler OnFileProcessed;
        
        /// <inheritdoc cref="RoboCommand.OnCommandError"/>
        public event RoboCommand.CommandErrorHandler OnCommandError;

        /// <inheritdoc cref="RoboCommand.OnError"/>
        public event RoboCommand.ErrorHandler OnError;

        /// <inheritdoc cref="RoboCommand.OnCommandCompleted"/>
        public event RoboCommand.CommandCompletedHandler OnCommandCompleted;

        /// <inheritdoc cref="RoboCommand.OnCopyProgressChanged"/>
        public event RoboCommand.CopyProgressHandler OnCopyProgressChanged;

        /// <summary>Handles <see cref="OnProgressEstimatorCreated"/></summary>
        public delegate void ProgressUpdaterCreatedHandler(RoboQueue sender, ProgressEstimatorCreatedEventArgs e);
        /// <summary>
        /// Occurs when a <see cref="Results.ProgressEstimator"/> is created when starting a new task, allowing binding to occur within the event subscriber. <br/>
        /// This event will occur once per Start.
        /// </summary>
        public event ProgressUpdaterCreatedHandler OnProgressEstimatorCreated;

        /// <summary>Handles <see cref="OnCommandStarted"/></summary>
        public delegate void CommandStartedHandler(RoboQueue sender, CommandStartedEventArgs e);
        /// <summary>
        /// Occurs after a Command has started succesffully
        /// </summary>
        public event CommandStartedHandler OnCommandStarted;

        /// <summary>
        /// EventArgs to declare when a RoboCommand process starts
        /// </summary>
        public class CommandStartedEventArgs : EventArgs
        {
            private CommandStartedEventArgs():base() { }
            internal CommandStartedEventArgs(RoboCommand cmd) : base() { Command = cmd; StartTime = System.DateTime.Now; }
            
            /// <summary>
            /// Command that started
            /// </summary>
            public RoboCommand Command { get; }
            
            /// <summary>
            /// Local time the command started.
            /// </summary>
            public System.DateTime StartTime { get; }
        }

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
            //If a TaskCancelSource is present, request cancellation. The continuation tasks null the value out then call this method to ensure everything stopped once they complete. 
            if (TaskCancelSource != null && !TaskCancelSource.IsCancellationRequested)
                TaskCancelSource.Cancel();
            else if (TaskCancelSource == null)
            {
                CommandList.ForEach((c) => c.Stop());
                IsCopyOperationRunning = false;
                IsListOnlyRunning = false;
                IsPaused = false;
            }
            WasCancelled = true;
        }

        /// <summary>
        /// Loop through the items in the list and issue <see cref="RoboCommand.Pause"/> on any commands where <see cref="RoboCommand.IsRunning"/> is true.
        /// </summary>
        public void PauseAll()
        {
            CommandList.ForEach((c) => { if (c.IsRunning) c.Pause(); });
            IsPaused = AnyPaused;
        }

        /// <summary>
        /// Loop through the items in the list and issue <see cref="RoboCommand.Resume"/> on any commands where <see cref="RoboCommand.IsPaused"/> is true.
        /// </summary>
        public void ResumeAll()
        {
            CommandList.ForEach((c) => { if (c.IsPaused) c.Resume(); });
            IsPaused = false;
        }

        #endregion

        #region < Run List-Only Mode >

        /// <summary>
        /// Set all RoboCommand objects to ListOnly mode, run them, then set all RoboCommands back to their previous ListOnly mode setting.
        /// </summary>
        /// <inheritdoc cref="StartJobs"/>
        public Task StartAll_ListOnly(string domain = "", string username = "", string password = "")
        {
            IsListOnlyRunning = true;
            ListOnlyCompleted = false;
            ListOnlyResults.Clear();
            //Store the setting for ListOnly prior to changing it
            List<Tuple<RoboCommand, bool>> OldListValues = new List<Tuple<RoboCommand, bool>>();
            CommandList.ForEach((c) => {
                OldListValues.Add(new Tuple<RoboCommand, bool>(c, c.LoggingOptions.ListOnly));
                c.LoggingOptions.ListOnly = true;
            });
            //Run the commands
            Task<RoboCopyResultsList> Run = StartJobs(domain, username, password);
            Task ResultsTask = Run.ContinueWith((continuation) =>
            {
                //Store the results then restore the ListOnly values
                ListOnlyResults.AddRange(Run.Result);
                foreach (var obj in OldListValues)
                    obj.Item1.LoggingOptions.ListOnly = obj.Item2;
                //Set Flags
                IsListOnlyRunning = false;
                IsPaused = false;
                ListOnlyCompleted = !WasCancelled;
            }
            );
            return ResultsTask;
        }

        #endregion

        #region < Run User-Set Parameters >

        /// <inheritdoc cref="StartJobs"/>
        public Task StartAll(string domain = "", string username = "", string password = "")
        {
            IsCopyOperationRunning = true;
            CopyOperationCompleted = false;
            RunOperationResults.Clear();
            Task<RoboCopyResultsList> Run = StartJobs(domain, username, password);
            Task ResultsTask = Run.ContinueWith((continuation) =>
            {
                RunOperationResults.AddRange(Run.Result);
                IsCopyOperationRunning = false;
                IsPaused = false;
                CopyOperationCompleted = !WasCancelled;
            }
            );
            return ResultsTask;
        }

        #endregion

        #region < StartJobs Method >

        /// <summary>
        /// Create Task that Starts all RoboCommands. 
        /// </summary>
        /// <remarks> <paramref name="domain"/>, <paramref name="password"/>, and <paramref name="username"/> are applied to all RoboCommand objects during this run. </remarks>
        /// <returns> New Task that finishes after all RoboCommands have stopped executing </returns>
        private Task<RoboCopyResultsList> StartJobs(string domain = "", string username = "", string password = "")
        {
            Debugger.Instance.DebugMessage("Starting Parallel execution of RoboQueue");

            TaskCancelSource = new CancellationTokenSource();
            CancellationToken cancellationToken = TaskCancelSource.Token;

            RoboCopyResultsList returnList = new RoboCopyResultsList();
            List<Task> TaskList = new List<Task>();
            JobsComplete = 0; OnPropertyChanged("JobsComplete");
            WasCancelled = false;

            //Create a Task to Start all the RoboCommands
            Task StartAll = Task.Factory.StartNew(() =>
            {
                //Reset results of all commands in the list
                foreach (RoboCommand cmd in CommandList)
                    cmd.ResetResults();

                ProgressEstimator = new ProgressEstimator(null);
                OnProgressEstimatorCreated?.Invoke(this, new ProgressEstimatorCreatedEventArgs(ProgressEstimator));

                //Start all commands, running as many as allowed
                foreach (RoboCommand cmd in CommandList)
                {
                    if (TaskCancelSource.IsCancellationRequested) break;
                    
                    //Assign the events
                    cmd.OnCommandCompleted += this.OnCommandCompleted;
                    cmd.OnCommandError+= this.OnCommandError;
                    cmd.OnCopyProgressChanged += this.OnCopyProgressChanged;
                    cmd.OnError += this.OnError;
                    cmd.OnFileProcessed += this.OnFileProcessed;
                    cmd.OnProgressEstimatorCreated += Cmd_OnProgressEstimatorCreated;

                    //Start the job
                    //Once the job ends, unsubscribe events
                    Task C = cmd.Start(domain, username, password);
                    Task T = C.ContinueWith((t) =>
                    {
                        cmd.OnCommandCompleted -= this.OnCommandCompleted;
                        cmd.OnCommandError -= this.OnCommandError;
                        cmd.OnCopyProgressChanged -= this.OnCopyProgressChanged;
                        cmd.OnError -= this.OnError;
                        cmd.OnFileProcessed -= this.OnFileProcessed;
                        //Notify the Property Changes
                        if (!cmd.IsCancelled) {
                            JobsComplete++;
                            OnPropertyChanged("JobsComplete");
                        }
                        OnPropertyChanged("JobsCurrentlyRunning");
                    });
                    TaskList.Add(T);                    //Add the continuation task to the list.
                    C.WaitUntil(TaskStatus.Running);    //Wait until the RoboCopy operation has begun
                    if (C.Status < TaskStatus.RanToCompletion) OnCommandStarted?.Invoke(this, new CommandStartedEventArgs(cmd)); //Declare that a new command in the queue has started.
                    OnPropertyChanged("JobsCurrentlyRunning");  //Notify the Property Changes

                    //Check if more jobs are allowed to run
                    while (MaxConcurrentJobs > 0 && JobsCurrentlyRunning >= MaxConcurrentJobs)
                        Thread.Sleep(500);
                }
            } , cancellationToken, TaskCreationOptions.LongRunning, PriorityScheduler.BelowNormal);

            //After all commands have started, continue with waiting for all commands to complete.
            Task WhenAll = StartAll.ContinueWith( (continuation) => Task.WaitAll(TaskList.ToArray(), cancellationToken), cancellationToken, TaskContinuationOptions.LongRunning, PriorityScheduler.BelowNormal);

            //Continuation Task return results to caller
            Task<RoboCopyResultsList> ContinueWithTask = WhenAll.ContinueWith((continuation) =>
            {
                ProgressEstimator?.UnBind();
                if (cancellationToken.IsCancellationRequested)
                {
                    //If cancellation was requested -> Issue the STOP command to all commands in the list
                    Debugger.Instance.DebugMessage("RunParrallel Task Was Cancelled");
                    TaskCancelSource = null;
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

        private void Cmd_OnProgressEstimatorCreated(RoboCommand sender, ProgressEstimatorCreatedEventArgs e)
        {
            ProgressEstimator?.BindToProgressEstimator(e.ResultsEstimate);
            sender.OnProgressEstimatorCreated -= Cmd_OnProgressEstimatorCreated;
        }

        #endregion

        #region < IDisposable Implementation >

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
                    ProgressEstimator?.UnBind();
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
        ~RoboQueue()
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

        #region < INotifyPropertyChanged, INotifyCollectionChanged, IEnumerable >

        /// <inheritdoc cref="INotifyPropertyChanged"/>
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <inheritdoc cref="ObservableCollection{T}.CollectionChanged"/>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { CommandList.CollectionChanged += value; }
            remove { CommandList.CollectionChanged -= value; }
        }

        /// <summary>
        /// Gets the enumerator for the enumeating through this object's <see cref="RoboCommand"/> objects
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return CommandList.GetEnumerator();
        }

        #endregion

        #region < List Access Methods >

        /// <summary>
        /// Exception thrown when attempting to run a method accesses the list backing a RoboQueue object while the tasks are in progress.
        /// </summary>
        public class ListAccessDeniedException : Exception
        {
            /// <remarks>This functionality is disabled if <see cref="IsRunning"/> == true.</remarks>
            /// <exception cref="ListAccessDeniedException"/>
            private const string StandardMsg = "Running methods that modify the list of RoboCommands methods while RoboQueue.IsRunning = TRUE is prohibited.";
            internal ListAccessDeniedException() : base(StandardMsg) { }
            internal ListAccessDeniedException(string message) : base($"{StandardMsg}\n{message}") { }
            internal ListAccessDeniedException(string message, Exception innerException) : base(message, innerException) { }
        }

        #region < Add >

        /// <inheritdoc cref="List{T}.Add(T)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void AddCommand(RoboCommand item)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.Add(item);
            OnPropertyChanged("ListCount");
            OnPropertyChanged("Commands");
            
        }

        /// <inheritdoc cref="List{T}.Insert(int, T)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void AddCommand(int index, RoboCommand item)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.Insert(index, item);
            OnPropertyChanged("ListCount");
            OnPropertyChanged("Commands");
        }

        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void AddCommand(IEnumerable<RoboCommand> collection)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.AddRange(collection);
            OnPropertyChanged("ListCount");
            OnPropertyChanged("Commands");
        }

        #endregion

        #region < Remove >

        /// <inheritdoc cref="List{T}.Remove(T)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void RemoveCommand(RoboCommand item)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.Remove(item);
            OnPropertyChanged("ListCount");
            OnPropertyChanged("Commands");
        }

        /// <inheritdoc cref="List{T}.RemoveAt(int)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void RemoveCommand(int index)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.RemoveAt(index);
            OnPropertyChanged("ListCount");
            OnPropertyChanged("Commands");
        }

        /// <inheritdoc cref="List{T}.RemoveRange(int, int)"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void RemoveCommand(int index, int count)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.RemoveRange(index, count);
            OnPropertyChanged("ListCount");
            OnPropertyChanged("Commands");
        }

        /// <inheritdoc cref="List{T}.RemoveAll(Predicate{T})"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void RemovCommand(Predicate<RoboCommand> match)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.RemoveAll(match);
            OnPropertyChanged("ListCount");
            OnPropertyChanged("Commands");
        }

        /// <inheritdoc cref="List{T}.Clear"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void ClearCommandList()
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.Clear();
            OnPropertyChanged("ListCount");
            OnPropertyChanged("Commands");
        }

        #endregion

        /// <inheritdoc cref="List{T}.ForEach(Action{T})"/>
        /// <inheritdoc cref="ListAccessDeniedException.StandardMsg"/>
        public void ForEach(Action<RoboCommand> action)
        {
            if (IsRunning) throw new ListAccessDeniedException();
            CommandList.ForEach(action);
        }

        /// <inheritdoc cref="List{T}.FindAll(Predicate{T})"/>
        public List<RoboCommand> FindAll(Predicate<RoboCommand> predicate) => CommandList.FindAll(predicate);

        /// <inheritdoc cref="List{T}.Find(Predicate{T})"/>
        public RoboCommand Find(Predicate<RoboCommand> predicate) => CommandList.Find(predicate);

        /// <inheritdoc cref="List{T}.IndexOf(T)"/>
        public int IndexOf(RoboCommand item) => CommandList.IndexOf(item);

        #endregion

    }
}
