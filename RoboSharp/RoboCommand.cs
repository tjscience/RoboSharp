using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RoboSharp.Interfaces;

namespace RoboSharp
{
    /// <summary>
    /// Wrapper for the RoboCopy process
    /// </summary>
    public class RoboCommand : IDisposable, IRoboCommand
    {
        #region Private Vars

        private Process process;
        private Task backupTask;
        private bool hasError;
        private bool hasExited;
        private bool isPaused;
        private bool isRunning;
        private bool isCancelled;
        private CopyOptions copyOptions = new CopyOptions();
        private SelectionOptions selectionOptions = new SelectionOptions();
        private RetryOptions retryOptions = new RetryOptions();
        private LoggingOptions loggingOptions = new LoggingOptions();
        private RoboSharpConfiguration configuration = new RoboSharpConfiguration();

        private Results.ResultsBuilder resultsBuilder;
        private Results.RoboCopyResults results;

        #endregion Private Vars

        #region Public Vars
        /// <summary> Value indicating if process is currently paused </summary>
        public bool IsPaused { get { return isPaused; } }
        /// <summary> Value indicating if process is currently running </summary>
        public bool IsRunning { get { return isRunning; } }
        /// <summary> Value indicating if process was Cancelled </summary>
        public bool IsCancelled { get { return isCancelled; } }
        /// <summary>  </summary>
        public string CommandOptions { get { return GenerateParameters(); } }
        /// <inheritdoc cref="RoboSharp.CopyOptions"/>
        public CopyOptions CopyOptions
        {
            get { return copyOptions; }
            set { copyOptions = value; }
        }
        /// <inheritdoc cref="RoboSharp.SelectionOptions"/>
        public SelectionOptions SelectionOptions
        {
            get { return selectionOptions; }
            set { selectionOptions = value; }
        }
        /// <inheritdoc cref="RoboSharp.RetryOptions"/>
        public RetryOptions RetryOptions
        {
            get { return retryOptions; }
            set { retryOptions = value; }
        }
        /// <inheritdoc cref="RoboSharp.LoggingOptions"/>
        public LoggingOptions LoggingOptions
        {
            get { return loggingOptions; }
            set { loggingOptions = value; }
        }
        /// <inheritdoc cref="RoboSharp.RoboSharpConfiguration"/>
        public RoboSharpConfiguration Configuration
        {
            get { return configuration; }
        }

        /// <summary>
        /// Value indicating if the process should be killed when the <see cref="Dispose()"/> method is called. <br/>
        /// For example, if the RoboCopy process should exit when the program exits, this should be set to TRUE.
        /// </summary>
        public bool StopIfDisposing { get; set; }

        #endregion Public Vars

        #region Events

        /// <summary>Handles <see cref="OnFileProcessed"/></summary>
        public delegate void FileProcessedHandler(object sender, FileProcessedEventArgs e);
        /// <summary>Occurs each time a new item has started processing</summary>
        public event FileProcessedHandler OnFileProcessed;

        /// <summary>Handles <see cref="OnCommandError"/></summary>
        public delegate void CommandErrorHandler(object sender, CommandErrorEventArgs e);
        /// <summary>Occurs when an error occurs while generating the command</summary>
        public event CommandErrorHandler OnCommandError;

        /// <summary>Handles <see cref="OnError"/></summary>
        public delegate void ErrorHandler(object sender, ErrorEventArgs e);
        /// <summary>Occurs when the command exits due to an error</summary>
        public event ErrorHandler OnError;

        /// <summary>Handles <see cref="OnCommandCompleted"/></summary>
        public delegate void CommandCompletedHandler(object sender, RoboCommandCompletedEventArgs e);
        /// <summary>Occurs when the command exits</summary>
        public event CommandCompletedHandler OnCommandCompleted;

        /// <summary>Handles <see cref="OnCopyProgressChanged"/></summary>
        public delegate void CopyProgressHandler(object sender, CopyProgressEventArgs e);
        /// <summary>Occurs each time the current item's progress is updated</summary>
        public event CopyProgressHandler OnCopyProgressChanged;

        #endregion

        /// <summary>Create a new RoboCommand object</summary>
        public RoboCommand()
        {

        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            resultsBuilder?.AddOutput(e.Data);

            if (e.Data == null)
                return;
            var data = e.Data.Trim().Replace("\0", "");
            if (data.IsNullOrWhiteSpace())
                return;

            if (data.EndsWith("%", StringComparison.Ordinal))
            {
                // copy progress data
                OnCopyProgressChanged?.Invoke(this, new CopyProgressEventArgs(Convert.ToDouble(data.Replace("%", ""), CultureInfo.InvariantCulture)));
            }
            else
            {
                if (OnFileProcessed != null)
                {
                    var splitData = data.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (splitData.Length == 2)
                    {
                        var file = new ProcessedFileInfo();
                        file.FileClass = "New Dir";
                        file.FileClassType = FileClassType.NewDir;
                        long size;
                        long.TryParse(splitData[0].Replace("New Dir", "").Trim(), out size);
                        file.Size = size;
                        file.Name = splitData[1];
                        OnFileProcessed(this, new FileProcessedEventArgs(file));
                    }
                    else if (splitData.Length == 3)
                    {
                        var file = new ProcessedFileInfo();
                        file.FileClass = splitData[0].Trim();
                        file.FileClassType = FileClassType.File;
                        long size = 0;
                        long.TryParse(splitData[1].Trim(), out size);
                        file.Size = size;
                        file.Name = splitData[2];
                        OnFileProcessed(this, new FileProcessedEventArgs(file));
                    }
                    else
                    {
                        var regex = new Regex($" {Configuration.ErrorToken} " + @"(\d{1,3}) \(0x\d{8}\) ");

                        if (OnError != null && regex.IsMatch(data))
                        {
                            // parse error code
                            var match = regex.Match(data);
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
                        else
                        {
                            if (!data.StartsWith("----------"))
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
                                    OnFileProcessed(this, new FileProcessedEventArgs(file));
                                }
                            }
                        }
                    }
                }
            }
        }

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

#if NET45
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
            
            isRunning = true;

            var tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            resultsBuilder = new Results.ResultsBuilder();
            results = null;

            // make sure source path is valid
            if (!Directory.Exists(CopyOptions.Source))
            {
                Debugger.Instance.DebugMessage("The Source directory does not exist.");
                hasError = true;
                OnCommandError?.Invoke(this, new CommandErrorEventArgs(new DirectoryNotFoundException("The Source directory does not exist.")));
                Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
                tokenSource.Cancel(true);
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
                    tokenSource.Cancel(true);
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
                        tokenSource.Cancel(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Debugger.Instance.DebugMessage(ex.Message);
                hasError = true;
                OnCommandError?.Invoke(this, new CommandErrorEventArgs("The Destination directory is invalid.", ex));
                Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
                tokenSource.Cancel(true);
            }

            #endregion

            backupTask = Task.Factory.StartNew(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                process = new Process();

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
                process.StartInfo.Arguments = GenerateParameters();
                process.OutputDataReceived += process_OutputDataReceived;
                process.ErrorDataReceived += process_ErrorDataReceived;
                process.EnableRaisingEvents = true;
                hasExited = false;
                process.Exited += Process_Exited;
                Debugger.Instance.DebugMessage("RoboCopy process started.");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                results = resultsBuilder.BuildResults(process?.ExitCode ?? -1);
                Debugger.Instance.DebugMessage("RoboCopy process exited.");
            }, cancellationToken, TaskCreationOptions.LongRunning, PriorityScheduler.BelowNormal);

            Task continueWithTask = backupTask.ContinueWith((continuation) =>
            {
                if (!hasError)
                {
                    // backup is complete
                    if (OnCommandCompleted != null)
                    {
                        OnCommandCompleted(this, new RoboCommandCompletedEventArgs(results));
                        isRunning = false;
                    }
                }

                Stop();
            }, cancellationToken);

            return continueWithTask;
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (OnCommandError != null && !e.Data.IsNullOrWhiteSpace())
            {
                hasError = true;
                OnCommandError(this, new CommandErrorEventArgs(e.Data, null));
            }
        }

        void Process_Exited(object sender, System.EventArgs e)
        {
            hasExited = true;
        }

        /// <summary>Kill the process</summary>
        public void Stop()
        {
            if (process != null && CopyOptions.RunHours.IsNullOrWhiteSpace() && !hasExited)
            {
                process.Kill();
                hasExited = true;
                process.Dispose();
                process = null;
                isCancelled = true;
            }
            isRunning = !hasExited;
        }

        /// <inheritdoc cref="Results.RoboCopyResults"/>
        /// <returns>The RoboCopyResults object from the last run</returns>
        public Results.RoboCopyResults GetResults()
        {
            return results;
        }

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
            //var systemOptions = " /V /R:0 /FP /BYTES /W:0 /NJH /NJS";

            return string.Format("{0}{1}{2}{3} /BYTES", parsedCopyOptions, parsedSelectionOptions,
                parsedRetryOptions, parsedLoggingOptions);
        }

        #region IDisposable Implementation

        bool disposed = false;

        /// <inheritdoc cref="IDisposable.Dispose"/>>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>IDisposable Implementation</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (StopIfDisposing && process != null && !hasExited)
            {
                process.Kill();
                hasExited = true;
                isCancelled = true;
                isRunning = false;
            }

            if (disposing)
            {

            }

            if (process != null)
                process.Dispose();

            disposed = true;
        }

        #endregion IDisposable Implementation
    }
}
