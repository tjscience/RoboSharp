using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RoboSharp.Interfaces;
using RoboSharp.EventArgObjects;

namespace RoboSharp
{
    /// <summary>
    /// Wrapper for the RoboCopy process
    /// </summary>
    public class RoboCommand : IDisposable, IRoboCommand, ICloneable
    {
        #region < Constructors >

        /// <summary>Create a new RoboCommand object</summary>
        public RoboCommand()
        {
            InitClassProperties();
            Init();
        }

        /// <inheritdoc cref="Init"/>
        public RoboCommand(string name, bool stopIfDisposing = true)
        {
            InitClassProperties();
            Init(name, stopIfDisposing);
        }

        /// <inheritdoc cref="Init"/>
        public RoboCommand(string source, string destination, bool stopIfDisposing = true)
        {
            InitClassProperties();
            Init("", stopIfDisposing, source, destination);

        }
        /// <inheritdoc cref="Init"/>
        public RoboCommand(string source, string destination, string name, bool stopIfDisposing = true)
        {
            InitClassProperties();
            Init(name, stopIfDisposing, source, destination);
        }

        /// <remarks> Each of the Options objects can be specified within this constructor. If left = null, a new object will be generated using the default options for that object. </remarks>
        /// <inheritdoc cref="Init"/>
        public RoboCommand(string name, string source = null, string destination = null, bool StopIfDisposing = true, RoboSharpConfiguration configuration = null, CopyOptions copyOptions = null, SelectionOptions selectionOptions = null, RetryOptions retryOptions = null, LoggingOptions loggingOptions = null, JobOptions jobOptions = null)
        {
            this.configuration = configuration ?? new RoboSharpConfiguration();
            this.copyOptions = copyOptions ?? new CopyOptions();
            this.selectionOptions = selectionOptions ?? new SelectionOptions();
            this.retryOptions = retryOptions ?? new RetryOptions();
            this.loggingOptions = loggingOptions ?? new LoggingOptions();
            this.jobOptions = jobOptions ?? new JobOptions();
            Init(name, StopIfDisposing, source ?? CopyOptions.Source, destination ?? CopyOptions.Destination);
        }

        /// <summary>
        /// Create a new RoboCommand with identical options at this RoboCommand
        /// </summary>
        /// <remarks>
        /// If Desired, the new RoboCommand object will share some of the same Property objects as the input <paramref name="command"/>. 
        /// For Example, that means that if a SelectionOption property changes, it will affect both RoboCommand objects since the  <see cref="SelectionOptions"/> property is shared between them. <br/>
        /// If the Link* options are set to FALSE (default), then it will create new property objects whose settings match the current settings of <paramref name="command"/>.
        /// <para/> Properties that can be linked: <br/>
        /// <see cref="Configuration"/> ( Linked by default ) <br/>
        /// <see cref="RetryOptions"/> ( Linked by default )<br/>
        /// <see cref="SelectionOptions"/><br/>
        /// <see cref="LoggingOptions"/><br/>
        /// </remarks>
        /// <param name="command">RoboCommand to Clone</param>
        /// <param name="NewSource">Specify a new source if desired. If left as null, will use Source from <paramref name="command"/></param>
        /// <param name="NewDestination">Specify a new source if desired. If left as null, will use Destination from <paramref name="command"/></param>
        /// <param name="LinkConfiguration">Link the <see cref="Configuration"/> of the two commands ( True Default )</param>
        /// <param name="LinkLoggingOptions">Link the <see cref="LoggingOptions"/> of the two commands</param>
        /// <param name="LinkRetryOptions">Link the <see cref="RetryOptions"/> of the two commands ( True Default )</param>
        /// <param name="LinkSelectionOptions">Link the <see cref="SelectionOptions"/> of the two commands</param>
        public RoboCommand(RoboCommand command, string NewSource = null, string NewDestination = null, bool LinkConfiguration = true, bool LinkRetryOptions = true, bool LinkSelectionOptions = false, bool LinkLoggingOptions = false)
        {
            Name = command.Name;
            StopIfDisposing = command.StopIfDisposing;

            copyOptions = new CopyOptions(command.CopyOptions, NewSource, NewDestination);
            selectionOptions = LinkSelectionOptions ? command.selectionOptions : command.SelectionOptions.Clone();
            retryOptions = LinkRetryOptions ? command.retryOptions : command.retryOptions.Clone();
            loggingOptions = LinkLoggingOptions ? command.loggingOptions : command.loggingOptions.Clone();
            configuration = LinkConfiguration ? command.configuration : command.configuration.Clone();
        }

        /// <summary>Create a new RoboCommand object</summary>
        /// <param name="name"><inheritdoc cref="Name" path="*"/></param>
        /// <param name="stopIfDisposing"><inheritdoc cref="StopIfDisposing" path="*"/></param>
        /// <param name="source"><inheritdoc cref="RoboSharp.CopyOptions.Source"/></param>
        /// <param name="destination"><inheritdoc cref="RoboSharp.CopyOptions.Destination"/></param>
        private void Init(string name = "", bool stopIfDisposing = true, string source = "", string destination = "")
        {
            Name = name;
            StopIfDisposing = stopIfDisposing;
            CopyOptions.Source = source;
            CopyOptions.Destination = destination;
        }

        private void InitClassProperties()
        {
            copyOptions = new CopyOptions();
            selectionOptions = new SelectionOptions();
            retryOptions = new RetryOptions();
            loggingOptions = new LoggingOptions();
            configuration = new RoboSharpConfiguration();
            jobOptions = new JobOptions();
        }

        /// <inheritdoc cref="RoboCommand.RoboCommand(RoboCommand, string, string, bool, bool, bool, bool)"/>
        public RoboCommand Clone(string NewSource = null, string NewDestination = null, bool LinkConfiguration = true, bool LinkRetryOptions = true, bool LinkSelectionOptions = false, bool LinkLoggingOptions = false)
            => new RoboCommand(this, NewSource, NewDestination, LinkConfiguration, LinkRetryOptions, LinkSelectionOptions, LinkLoggingOptions);

        object ICloneable.Clone() => new RoboCommand(this, null, null, false, false, false, false);

        #endregion

        #region < Private Vars >

        // set up in Constructor
        private CopyOptions copyOptions;
        private SelectionOptions selectionOptions;
        private RetryOptions retryOptions;
        private LoggingOptions loggingOptions;
        private RoboSharpConfiguration configuration;
        private JobOptions jobOptions;

        // Modified while running
        private Process process;
        private Task backupTask;
        private bool hasError;
        //private bool hasExited; //No longer evaluated
        private bool isPaused;
        private bool isRunning;
        private bool isCancelled;
        private Results.ResultsBuilder resultsBuilder;
        private Results.RoboCopyResults results;

        #endregion Private Vars

        #region < Public Vars >

        /// <summary> ID Tag for the job - Allows consumers to find/sort/remove/etc commands within a list via string comparison</summary>
        public string Name { get; set; }
        /// <summary> Value indicating if process is currently paused </summary>
        public bool IsPaused { get { return isPaused; } }
        /// <summary> Value indicating if process is currently running </summary>
        public bool IsRunning { get { return isRunning; } }
        /// <summary> Value indicating if process was Cancelled </summary>
        public bool IsCancelled { get { return isCancelled; } }
        /// <summary> TRUE if <see cref="CopyOptions.RunHours"/> is set up (Copy Operation is scheduled to only operate within specified timeframe). Otherwise False. </summary>
        public bool IsScheduled { get => !String.IsNullOrWhiteSpace(CopyOptions.RunHours); }
        /// <summary> Get the parameters string passed to RoboCopy based on the current setup </summary>
        public string CommandOptions { get { return GenerateParameters(); } }
        /// <inheritdoc cref="RoboSharp.CopyOptions"/>
        public CopyOptions CopyOptions
        {
            get { return copyOptions; }
            set { copyOptions = value ?? copyOptions; }
        }
        /// <inheritdoc cref="RoboSharp.SelectionOptions"/>
        public SelectionOptions SelectionOptions
        {
            get { return selectionOptions; }
            set { selectionOptions = value ?? selectionOptions; }
        }
        /// <inheritdoc cref="RoboSharp.RetryOptions"/>
        public RetryOptions RetryOptions
        {
            get { return retryOptions; }
            set { retryOptions = value ?? retryOptions; }
        }
        /// <inheritdoc cref="RoboSharp.LoggingOptions"/>
        public LoggingOptions LoggingOptions
        {
            get { return loggingOptions; }
            set { loggingOptions = value ?? loggingOptions; }
        }
        /// <inheritdoc cref="RoboSharp.JobOptions"/>
        public JobOptions JobOptions
        {
            get { return jobOptions; }
            set { jobOptions = value ?? jobOptions; }
        }
        /// <inheritdoc cref="RoboSharp.RoboSharpConfiguration"/>
        public RoboSharpConfiguration Configuration
        {
            get { return configuration; }
        }
        /// <inheritdoc cref="Results.ProgressEstimator"/>
        /// <remarks>
        /// A new <see cref="Results.ProgressEstimator"/> object is created every time the <see cref="Start"/> method is called, but will not be created until called for the first time. 
        /// </remarks>
        internal Results.ProgressEstimator ProgressEstimator { get; private set; }

        /// <inheritdoc cref="RoboCommand.ProgressEstimator"/>
        public IProgressEstimator IProgressEstimator => this.ProgressEstimator;

        /// <summary>
        /// Value indicating if the process should be killed when the <see cref="Dispose()"/> method is called.<br/>
        /// For example, if the RoboCopy process should exit when the program exits, this should be set to TRUE (default).
        /// </summary>
        public bool StopIfDisposing { get; set; } = true;

        #endregion Public Vars

        #region < Events >

        /// <summary>Handles <see cref="OnFileProcessed"/></summary>
        public delegate void FileProcessedHandler(RoboCommand sender, FileProcessedEventArgs e);
        /// <summary>Occurs each time a new item has started processing</summary>
        public event FileProcessedHandler OnFileProcessed;

        /// <summary>Handles <see cref="OnCommandError"/></summary>
        public delegate void CommandErrorHandler(RoboCommand sender, CommandErrorEventArgs e);
        /// <summary>Occurs when an error occurs while generating the command that prevents the RoboCopy process from starting.</summary>
        public event CommandErrorHandler OnCommandError;

        /// <summary>Handles <see cref="OnError"/></summary>
        public delegate void ErrorHandler(RoboCommand sender, ErrorEventArgs e);
        /// <summary>Occurs an error is detected by RoboCopy </summary>
        public event ErrorHandler OnError;

        /// <summary>Handles <see cref="OnCommandCompleted"/></summary>
        public delegate void CommandCompletedHandler(RoboCommand sender, RoboCommandCompletedEventArgs e);
        /// <summary>Occurs when the RoboCopy process has finished executing and results are available.</summary>
        public event CommandCompletedHandler OnCommandCompleted;

        /// <summary>Handles <see cref="OnCopyProgressChanged"/></summary>
        public delegate void CopyProgressHandler(RoboCommand sender, CopyProgressEventArgs e);
        /// <summary>Occurs each time the current item's progress is updated</summary>
        public event CopyProgressHandler OnCopyProgressChanged;

        /// <summary>Handles <see cref="OnProgressEstimatorCreated"/></summary>
        public delegate void ProgressUpdaterCreatedHandler(RoboCommand sender, ProgressEstimatorCreatedEventArgs e);
        /// <summary>
        /// Occurs when a <see cref="Results.ProgressEstimator"/> is created during <see cref="Start"/>, allowing binding to occur within the event subscriber. <br/>
        /// This event will occur once per Start.
        /// </summary>
        public event ProgressUpdaterCreatedHandler OnProgressEstimatorCreated;

        #endregion

        #region < Pause / Stop / Resume >

        /// <summary>Pause execution of the RoboCopy process when <see cref="IsPaused"/> == false</summary>
        public void Pause()
        {
            if (process != null && isPaused == false)
            {
                Debugger.Instance.DebugMessage("RoboCommand execution paused.");
                process.Suspend();
                isPaused = true;
            }
        }

        /// <summary>Resume execution of the RoboCopy process when <see cref="IsPaused"/> == true</summary>
        public void Resume()
        {
            if (process != null && isPaused == true)
            {
                Debugger.Instance.DebugMessage("RoboCommand execution resumed.");
                process.Resume();
                isPaused = false;
            }
        }

        /// <summary> Immediately Kill the RoboCopy process</summary>
        public void Stop() => Stop(false);

        private void Stop(bool DisposeProcess)
        {
            //Note: This previously checked for CopyOptions.RunHours.IsNullOrWhiteSpace() == TRUE prior to issuing the stop command
            //If the removal of that check broke your application, please create a new issue thread on the repo.
            if (process != null)
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    isCancelled = true;
                }
                //hasExited = true;
                if (DisposeProcess)
                {
                    process.Dispose();
                    process = null;
                }
            }
            isPaused = false;
        }

        #endregion

        #region < Start Methods >

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Start the RoboCopy Process, then return the results.
        /// </summary>
        /// <returns>Returns the RoboCopy results once RoboCopy has finished executing.</returns>        
        /// <inheritdoc cref="Start(string, string, string)"/>
        public async Task<Results.RoboCopyResults> StartAsync(string domain = "", string username = "", string password = "")
        {
            await Start(domain, username, password);
            return GetResults();
        }
#endif

        /// <summary>
        /// Start the RoboCopy Process.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Returns a task that reports when the RoboCopy process has finished executing.</returns>
        public Task Start(string domain = "", string username = "", string password = "")
        {
            Debugger.Instance.DebugMessage("RoboCommand started execution.");
            hasError = false;
            isCancelled = false;
            isPaused = false;
            isRunning = true;

            resultsBuilder = new Results.ResultsBuilder(this);
            results = null;

            #region Check Source and Destination

#if NET40_OR_GREATER
            // Authenticate on Target Server -- Create user if username is provided, else null
            ImpersonatedUser impersonation = username.IsNullOrWhiteSpace() ? null : impersonation = new ImpersonatedUser(username, domain, password);
#endif
            // make sure source path is valid
            if (!Directory.Exists(CopyOptions.Source))
            {
                Debugger.Instance.DebugMessage("The Source directory does not exist.");
                hasError = true;
                OnCommandError?.Invoke(this, new CommandErrorEventArgs(new DirectoryNotFoundException("The Source directory does not exist.")));
                Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
            }

            #region Create Destination Directory

            //Check that the Destination Drive is accessible insteead [fixes #106]
            try
            {
                //Check if the destination drive is accessible -> should not cause exception [Fix for #106]
                DirectoryInfo dInfo = new DirectoryInfo(CopyOptions.Destination).Root;
                if (!dInfo.Exists)
                {
                    Debugger.Instance.DebugMessage("The destination drive does not exist.");
                    hasError = true;
                    OnCommandError?.Invoke(this, new CommandErrorEventArgs(new DirectoryNotFoundException("The Destination Drive is invalid.")));
                    Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
                }
                //If not list only, verify that drive has write access -> should cause exception if no write access [Fix #101]
                if (!LoggingOptions.ListOnly & !hasError)
                {
                    dInfo = Directory.CreateDirectory(CopyOptions.Destination);
                    if (!dInfo.Exists)
                    {
                        Debugger.Instance.DebugMessage("The destination directory does not exist.");
                        hasError = true;
                        OnCommandError?.Invoke(this, new CommandErrorEventArgs(new DirectoryNotFoundException("Unable to create Destination Folder. Check Write Access.")));
                        Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.Instance.DebugMessage(ex.Message);
                hasError = true;
                OnCommandError?.Invoke(this, new CommandErrorEventArgs("The Destination directory is invalid.", ex));
                Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
            }

            #endregion

#if NET40_OR_GREATER
            //Dispose Authentification
            impersonation?.Dispose();
#endif

            #endregion

            if (hasError)
            {
                isRunning = false;
                return Task.Delay(5);
            }
            else
            {
                //Raise EstimatorCreatedEvent to alert consumers that the Estimator can now be bound to
                ProgressEstimator = resultsBuilder.Estimator;
                OnProgressEstimatorCreated?.Invoke(this, new ProgressEstimatorCreatedEventArgs(resultsBuilder.Estimator));
                return GetBackupTask(resultsBuilder, domain, username, password);
            }
        }

        /// <summary>
        /// Start the RoboCopy process and the watcher task
        /// </summary>
        /// <returns>The continuation task that cleans up after the task that watches RoboCopy has finished executing.</returns>
        private Task GetBackupTask(Results.ResultsBuilder resultsBuilder, string domain = "", string username = "", string password = "")
        {
            var tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            isRunning = !cancellationToken.IsCancellationRequested;
            DateTime StartTime = DateTime.Now;

            backupTask = Task.Factory.StartNew(async () =>
           {
               cancellationToken.ThrowIfCancellationRequested();

               process = new Process();

               //Declare Encoding
               process.StartInfo.StandardOutputEncoding = Configuration.StandardOutputEncoding;
               process.StartInfo.StandardErrorEncoding = Configuration.StandardErrorEncoding;

               if (!string.IsNullOrEmpty(domain))
               {
                   Debugger.Instance.DebugMessage(string.Format("RoboCommand running under domain - {0}", domain));
                   process.StartInfo.Domain = domain;
               }

               if (!string.IsNullOrEmpty(username))
               {
                   Debugger.Instance.DebugMessage(string.Format("RoboCommand running under username - {0}", username));
                   process.StartInfo.UserName = username;
               }

               if (!string.IsNullOrEmpty(password))
               {
                   Debugger.Instance.DebugMessage("RoboCommand password entered.");
                   var ssPwd = new System.Security.SecureString();

                   for (int x = 0; x < password.Length; x++)
                   {
                       ssPwd.AppendChar(password[x]);
                   }

                   process.StartInfo.Password = ssPwd;
               }

               Debugger.Instance.DebugMessage("Setting RoboCopy process up...");
               process.StartInfo.UseShellExecute = false;
               process.StartInfo.RedirectStandardOutput = true;
               process.StartInfo.RedirectStandardError = true;
               process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
               process.StartInfo.CreateNoWindow = true;
               process.StartInfo.FileName = Configuration.RoboCopyExe;
               if (resultsBuilder != null)
               {
                   resultsBuilder.Source = CopyOptions.Source;
                   resultsBuilder.Destination = CopyOptions.Destination;
                   resultsBuilder.CommandOptions = GenerateParameters();
               }
               process.StartInfo.Arguments = resultsBuilder?.CommandOptions ?? GenerateParameters();
               process.OutputDataReceived += process_OutputDataReceived;
               process.ErrorDataReceived += process_ErrorDataReceived;
               process.EnableRaisingEvents = true;
               //hasExited = false;
               //Setup the Wait Cancellation Token
               var WaitExitSource = new CancellationTokenSource();
               process.Exited += (o,e) => Process_Exited(WaitExitSource);
               Debugger.Instance.DebugMessage("RoboCopy process started.");
               process.Start();
               process.BeginOutputReadLine();
               process.BeginErrorReadLine();
               await process.WaitForExitAsync(WaitExitSource.Token);    //Wait asynchronously for process to exit
               if (resultsBuilder != null)          // Only replace results if a ResultsBuilder was supplied
               {
                   results = resultsBuilder.BuildResults(process?.ExitCode ?? -1);
               }
               Debugger.Instance.DebugMessage("RoboCopy process exited.");
           }, cancellationToken, TaskCreationOptions.LongRunning, PriorityScheduler.BelowNormal).Unwrap();

            Task continueWithTask = backupTask.ContinueWith( (continuation) => // this task always runs
            {
                tokenSource.Dispose(); tokenSource = null; // Dispose of the Cancellation Token
                Stop(true); //Ensure process is disposed of - Sets IsRunning flags to false

                //Run Post-Processing of the Generated JobFile if one was created.
                JobOptions.RunPostProcessing(this);

                isRunning = false; //Now that all processing is complete, IsRunning should be reported as false.
                //Raise event announcing results are available
                if (!hasError && resultsBuilder != null)
                    OnCommandCompleted?.Invoke(this, new RoboCommandCompletedEventArgs(results, StartTime, DateTime.Now));
            });

            return continueWithTask;
        }

        /// <summary>
        /// Save this RoboCommand's options to a new RoboCopyJob ( *.RCJ ) file. <br/>
        /// Note: This will not save the path submitted into <see cref="JobOptions.FilePath"/>.
        /// </summary>
        /// <remarks>
        /// Job Files don't care if the Source/Destination are invalid, since they just save the command values to a file.
        /// </remarks>
        /// <param name="path"><inheritdoc cref="JobOptions.FilePath"/></param>
        /// <param name="IncludeSource">Save <see cref="CopyOptions.Source"/> into the RCJ file.</param>
        /// <param name="IncludeDestination">Save <see cref="CopyOptions.Destination"/> into the RCJ file.</param>
        /// <inheritdoc cref = "Start(string, string, string)" />
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
        public async Task SaveAsJobFile(string path, bool IncludeSource = false, bool IncludeDestination = false, string domain = "", string username = "", string password = "")
#pragma warning restore CS1573
        {
            bool _QUIT = JobOptions.PreventCopyOperation;
            string _PATH = JobOptions.FilePath;
            bool _NODD = JobOptions.NoDestinationDirectory;
            bool _NOSD = JobOptions.NoSourceDirectory;

            JobOptions.FilePath = path;
            JobOptions.NoSourceDirectory = !IncludeSource;
            JobOptions.NoDestinationDirectory = !IncludeDestination;
            JobOptions.PreventCopyOperation = true;

            await GetBackupTask(null, domain, username, password); //This should take approximately 1-2 seconds at most

            //Restore Original Settings
            JobOptions.FilePath = _PATH;
            JobOptions.NoSourceDirectory = _NOSD;
            JobOptions.NoDestinationDirectory = _NODD;
            JobOptions.PreventCopyOperation = _QUIT;
        }


        #endregion

        #region < Process Event Handlers >

        /// <summary> Occurs when the Process reports an error prior to starting the robocopy process, not an 'error' from Robocopy </summary>
        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (OnCommandError != null && !e.Data.IsNullOrWhiteSpace())
            {
                hasError = true;
                OnCommandError(this, new CommandErrorEventArgs(e.Data, null));
            }
        }

        /// <summary> Process has either run to completion or has been killed prematurely </summary>
        void Process_Exited(CancellationTokenSource WaitExitSource)
        {
            //hasExited = true;
            WaitExitSource.Cancel();
        }

        /// <summary> React to Process.StandardOutput </summary>
        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var currentFile = resultsBuilder?.Estimator?.CurrentFile;
            var currentDir = resultsBuilder?.Estimator?.CurrentDir;
            resultsBuilder?.AddOutput(e.Data);

            if (e.Data == null)
                return;
            var data = e.Data.Trim().Replace("\0", "");
            if (data.IsNullOrWhiteSpace())
                return;

            if (data.EndsWith("%", StringComparison.Ordinal))
            {
                //Increment ProgressEstimator
                if (data == "100%")
                    resultsBuilder?.AddFileCopied(currentFile);
                else
                    resultsBuilder?.SetCopyOpStarted();
                resultsBuilder?.SetCopyOpStarted();

                // copy progress data -> Use the CurrentFile and CurrentDir from the ResultsBuilder
                OnCopyProgressChanged?.Invoke(this,
                    new CopyProgressEventArgs(
                        Convert.ToDouble(data.Replace("%", ""), CultureInfo.InvariantCulture),
                        currentFile, currentDir
                    ));

            }
            else
            {
                //Parse the string to determine which event to raise
                var splitData = data.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (splitData.Length == 2) // Directory
                {
                    // Regex to parse the string for FileCount, Path, and Type (Description)
                    Regex DirRegex = new Regex("^(?<Type>\\*?[a-zA-Z]{0,10}\\s?[a-zA-Z]{0,3})\\s*(?<FileCount>[-]{0,1}[0-9]{1,100})\\t(?<Path>.+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

                    var file = new ProcessedFileInfo();
                    file.FileClassType = FileClassType.NewDir;

                    if (DirRegex.IsMatch(data))
                    {
                        //New Method - Parsed using Regex
                        GroupCollection MatchData = DirRegex.Match(data).Groups;
                        file.FileClass = MatchData["Type"].Value.Trim();
                        if (file.FileClass == "") file.FileClass = configuration.LogParsing_ExistingDir;
                        long.TryParse(MatchData["FileCount"].Value, out long size);
                        file.Size = size;
                        file.Name = MatchData["Path"].Value.Trim();
                    }
                    else
                    {
                        //Old Method -> Left Intact for other language compatibilty / unforseen cases
                        file.FileClass = "New Dir";
                        long.TryParse(splitData[0].Replace("New Dir", "").Trim(), out long size);
                        file.Size = size;
                        file.Name = splitData[1];
                    }

                    resultsBuilder?.AddDir(file, !this.LoggingOptions.ListOnly);
                    OnFileProcessed?.Invoke(this, new FileProcessedEventArgs(file));
                }
                else if (splitData.Length == 3) // File
                {
                    var file = new ProcessedFileInfo();
                    file.FileClass = splitData[0].Trim();
                    file.FileClassType = FileClassType.File;
                    long size = 0;
                    long.TryParse(splitData[1].Trim(), out size);
                    file.Size = size;
                    file.Name = splitData[2];
                    resultsBuilder?.AddFile(file, !LoggingOptions.ListOnly);
                    OnFileProcessed?.Invoke(this, new FileProcessedEventArgs(file));
                }
                else if (OnError != null && Configuration.ErrorTokenRegex.IsMatch(data)) // Error Message
                {

                    // parse error code
                    var match = Configuration.ErrorTokenRegex.Match(data);
                    string value = match.Groups[1].Value;
                    int parsedValue = Int32.Parse(value);

                    var errorCode = ApplicationConstants.ErrorCodes.FirstOrDefault(x => data.Contains(x.Key));
                    if (errorCode.Key != null)
                    {
                        OnError(this, new ErrorEventArgs(string.Format("{0}{1}{2}", data, Environment.NewLine, errorCode.Value), parsedValue));
                    }
                    else
                    {
                        OnError(this, new ErrorEventArgs(data, parsedValue));
                    }
                }
                else if (!data.StartsWith("----------")) // System Message
                {
                    // Do not log errors that have already been logged
                    var errorCode = ApplicationConstants.ErrorCodes.FirstOrDefault(x => data == x.Value);

                    if (errorCode.Key == null)
                    {
                        var file = new ProcessedFileInfo();
                        file.FileClass = "System Message";
                        file.FileClassType = FileClassType.SystemMessage;
                        file.Size = 0;
                        file.Name = data;
                        OnFileProcessed?.Invoke(this, new FileProcessedEventArgs(file));
                    }
                }
            }
        }

        #endregion

        #region < Other Public Methods >

        /// <inheritdoc cref="Results.RoboCopyResults"/>
        /// <returns>The RoboCopyResults object from the last run</returns>
        public Results.RoboCopyResults GetResults()
        {
            return results;
        }

        /// <summary>
        /// Set the results to null - This is to prevent adding results from a previous run being added to the results list by RoboQueue
        /// </summary>
        internal void ResetResults()
        {
            results = null;
        }

        /// <summary>
        /// Generate the Parameters and Switches to execute RoboCopy with based on the configured settings
        /// </summary>
        /// <returns></returns>
        private string GenerateParameters()
        {
            Debugger.Instance.DebugMessage("Generating parameters...");
            Debugger.Instance.DebugMessage(CopyOptions);
            var parsedCopyOptions = CopyOptions.Parse();
            var parsedSelectionOptions = SelectionOptions.Parse();
            Debugger.Instance.DebugMessage("SelectionOptions parsed.");
            var parsedRetryOptions = RetryOptions.Parse();
            Debugger.Instance.DebugMessage("RetryOptions parsed.");
            var parsedLoggingOptions = LoggingOptions.Parse();
            Debugger.Instance.DebugMessage("LoggingOptions parsed.");
            var parsedJobOptions = JobOptions.Parse();
            Debugger.Instance.DebugMessage("LoggingOptions parsed.");
            //var systemOptions = " /V /R:0 /FP /BYTES /W:0 /NJH /NJS";

            return string.Format("{0}{1}{2}{3} /BYTES {4}", parsedCopyOptions, parsedSelectionOptions,
                parsedRetryOptions, parsedLoggingOptions, parsedJobOptions);
        }

        /// <inheritdoc cref="GenerateParameters"/>
        public override string ToString()
        {
            return GenerateParameters();
        }

        /// <summary>
        /// Combine this object's options with that of some JobFile
        /// </summary>
        /// <param name="jobFile"></param>
        public void MergeJobFile(JobFile jobFile)
        {
            Name = Name.ReplaceIfEmpty(jobFile.Job_Name);
            copyOptions.Merge(jobFile.CopyOptions);
            LoggingOptions.Merge(jobFile.LoggingOptions);
            RetryOptions.Merge(jobFile.RetryOptions);
            SelectionOptions.Merge(jobFile.SelectionOptions);
            JobOptions.Merge(((IRoboCommand)jobFile).JobOptions);
            //this.StopIfDisposing |= ((IRoboCommand)jobFile).StopIfDisposing;
        }

        #endregion

        #region < IDisposable Implementation >

        bool disposed = false;

        /// <summary>Dispose of this object. Kills RoboCopy process if <see cref="StopIfDisposing"/> == true &amp;&amp; <see cref="IsScheduled"/> == false. </summary>
        /// <remarks><inheritdoc cref="IDisposable.Dispose" path="/summary"/></remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer -> Cleans up resources when garbage collected
        /// </summary>
        ~RoboCommand()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>IDisposable Implementation</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {

            }

            if (StopIfDisposing && !IsScheduled)
            {
                Stop(true);
            }

            //Release any hooks to the process, but allow it to continue running
            process?.Dispose();
            process = null;

            disposed = true;
        }

        #endregion IDisposable Implementation
    }
}
