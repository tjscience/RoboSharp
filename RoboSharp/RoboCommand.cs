using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RoboSharp
{
    public class RoboCommand : IDisposable
    {
        #region Private Vars

        private Process process;
        private Task backupTask;
        private bool hasError;
        private bool hasExited;
        private bool isPaused;
        private CopyOptions copyOptions = new CopyOptions();
        private SelectionOptions selectionOptions = new SelectionOptions();
        private RetryOptions retryOptions = new RetryOptions();
        private LoggingOptions loggingOptions = new LoggingOptions();
        private RoboSharpConfiguration configuration = new RoboSharpConfiguration();

        #endregion Private Vars

        #region Public Vars
        public bool IsPaused { get { return isPaused; } }
        public string CommandOptions { get { return GenerateParameters(); } }    
        public CopyOptions CopyOptions
        {
            get { return copyOptions; }
            set { copyOptions = value; }
        }
        public SelectionOptions SelectionOptions
        {
            get { return selectionOptions; }
            set { selectionOptions = value; }
        }
        public RetryOptions RetryOptions
        {
            get { return retryOptions; }
            set { retryOptions = value; }
        }
        public LoggingOptions LoggingOptions
        {
            get { return loggingOptions; }
            set { loggingOptions = value; }
        }

        public RoboSharpConfiguration Configuration
        {
            get { return configuration; }
        }

        #endregion Public Vars

        #region Events

        public delegate void FileProcessedHandler(object sender, FileProcessedEventArgs e);
        public event FileProcessedHandler OnFileProcessed;
        public delegate void CommandErrorHandler(object sender, ErrorEventArgs e);
        public event CommandErrorHandler OnCommandError;
        public delegate void ErrorHandler(object sender, ErrorEventArgs e);
        public event ErrorHandler OnError;
        public delegate void CommandCompletedHandler(object sender, RoboCommandCompletedEventArgs e);
        public event CommandCompletedHandler OnCommandCompleted;
        public delegate void CopyProgressHandler(object sender, CopyProgressEventArgs e);
        public event CopyProgressHandler OnCopyProgressChanged;

        #endregion

        public RoboCommand()
        {
            
        }

        void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
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
                        if (OnError != null && Regex.IsMatch(data, $" {Configuration.ErrorToken} " + @"\d{1,3} \(0x\d{8}\) "))
                        {
                            var errorCode = ApplicationConstants.ErrorCodes.FirstOrDefault(x => data.Contains(x.Key));

                            if(errorCode.Key != null)
                            {
                                OnError(this, new ErrorEventArgs(string.Format("{0}{1}{2}", data, Environment.NewLine, errorCode.Value)));
                            }
                            else
                            {
                                OnError(this, new ErrorEventArgs(data));
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

        public void Pause()
        {
            if (process != null && isPaused == false)
            {
                Debugger.Instance.DebugMessage("RoboCommand execution paused.");
                process.Suspend();
                isPaused = true;
            }
        }

        public void Resume()
        {
            if (process != null && isPaused == true)
            {
                Debugger.Instance.DebugMessage("RoboCommand execution resumed.");
                process.Resume();
                isPaused = false;
            }
        }

        public Task Start(string domain = "", string username = "", string password = "")
        {
            Debugger.Instance.DebugMessage("RoboCommand started execution.");
            hasError = false;
	    
	    var tokenSource = new CancellationTokenSource();
	    CancellationToken cancellationToken = tokenSource.Token;

            // make sure source path is valid
            if(!Directory.Exists(CopyOptions.Source))
            {
                Debugger.Instance.DebugMessage("The Source directory does not exist.");
                hasError = true;
                OnCommandError?.Invoke(this, new ErrorEventArgs("The Source directory does not exist."));
                Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
		tokenSource.Cancel(true);
            }

            #region Create Destination Directory

            try
            {
                var dInfo = Directory.CreateDirectory(CopyOptions.Destination);
                if (!dInfo.Exists)
                {
                    Debugger.Instance.DebugMessage("The destination directory does not exist.");
                    hasError = true;
                    OnCommandError?.Invoke(this, new ErrorEventArgs("The Destination directory is invalid."));
                    Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
                    tokenSource.Cancel(true);
                }
            }
            catch (Exception ex)
            {
                Debugger.Instance.DebugMessage(ex.Message);
                hasError = true;
                OnCommandError?.Invoke(this, new ErrorEventArgs("The Destination directory is invalid."));
                Debugger.Instance.DebugMessage("RoboCommand execution stopped due to error.");
                tokenSource.Cancel(true);
            }

            #endregion

            backupTask = Task.Run(() =>
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
                process.Exited += Process_Exited;
                Debugger.Instance.DebugMessage("RoboCopy process started.");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                Debugger.Instance.DebugMessage("RoboCopy process exited.");
            },cancellationToken);

            backupTask.ContinueWith((continuation) =>
            {
                if (!hasError)
                {
                    // backup is complete
                    if (OnCommandCompleted != null)
                    {
                        OnCommandCompleted(this, new RoboCommandCompletedEventArgs());
                    }
                }

                Stop();
            });

            return backupTask;
        }

        void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (OnCommandError != null && !e.Data.IsNullOrWhiteSpace())
            {
                hasError = true;
                OnCommandError(this, new ErrorEventArgs(e.Data));
            }
        }
        
	    void Process_Exited(object sender, System.EventArgs e)
	    {
		    hasExited = true;
    	}
        
        public void Stop()
        {
            if (process != null && CopyOptions.RunHours.IsNullOrWhiteSpace() && !hasExited)
            {
                process.Kill();
                hasExited = true;
                process.Dispose();
                process = null;
            }
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
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

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
