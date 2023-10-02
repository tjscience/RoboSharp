using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using RoboSharp;
using RoboSharp.Extensions;
using RoboSharp.Interfaces;
using RoboSharp.Results;
using RoboSharp.EventArgObjects;
using System.Threading.Tasks;
using System.Threading;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Use this IRoboCommand when Moving files from one directory to another when the Source and Destination are on the same root path.<br/>
    /// <br/> Note: If the source and destination do not have the same root path, will use a standard RoboCommand instead.
    /// <br/> Utilizes File.Move() to facilitate the movement of files on the same drive.
    /// </summary>
    public class RoboMover : AbstractIRoboCommand
    {
        /// <inheritdoc/>
        public RoboMover() : base()
        {
            Evaluator = new PairEvaluator(this);
        }

        /// <inheritdoc/>
        public RoboMover(
            CopyOptions copyOptions = null, 
            LoggingOptions loggingOptions = null,
            RetryOptions retryOptions = null,
            SelectionOptions selectionOptions = null,
            RoboSharpConfiguration configuration = null
             ) : base(copyOptions, loggingOptions, retryOptions, selectionOptions, configuration)
        {
            Evaluator = new PairEvaluator(this);
        }

        /// <inheritdoc/>
        public RoboMover(
            string source, string destination,
            CopyActionFlags copyActionFlags = CopyActionFlags.Default,
            SelectionFlags selectionFlags = SelectionFlags.Default,
            LoggingFlags loggingFlags = LoggingFlags.RoboSharpDefault
             ) : base(source, destination, copyActionFlags, selectionFlags, loggingFlags)
        {
            Evaluator = new PairEvaluator(this);
        }

        /// <summary>
        /// Create a RoboMover object that clones the options of the input IRoboCommand
        /// </summary>
        /// <param name="cmd">The IRoboCommand to convert to a RoboMover object</param>
        public RoboMover(IRoboCommand cmd) : base(cmd)
        {
            Evaluator = new PairEvaluator(this);
        }

        private RoboCommand standardCommand;
        private Task runningTask;
        private CancellationTokenSource cancelRequest;
        private readonly PairEvaluator Evaluator;
        ResultsBuilder resultsBuilder;

        public RoboMoverOptions RoboMoverOptions { get; set; } = new RoboMoverOptions();

        public override JobOptions JobOptions { get; } = new JobOptions();

        /// <inheritdoc/>
        public override async Task Start(string domain = "", string username = "", string password = "")
        {
            if (IsRunning) throw new InvalidOperationException("Cannot execute Start command - Process has already started.");
            IsRunning = true;
            cancelRequest = new CancellationTokenSource();
            RoboCopyResults results = null;
            bool success = false;
            try
            {
                bool onSameDrive = Path.GetPathRoot(CopyOptions.Source).Equals(Path.GetPathRoot(CopyOptions.Destination), StringComparison.InvariantCultureIgnoreCase);
                bool isMoving = CopyOptions.MoveFiles | CopyOptions.MoveFilesAndDirectories;

                if (!isMoving | !onSameDrive | JobOptions.PreventCopyOperation)
                {
                    results = await RunAsRoboCopy(domain, username, password);
                    success = true;
                }
                else
                {
                    // Load the job file
                    if (File.Exists(JobOptions.LoadJobFilePath))
                    {
                        var jobfile = JobFile.ParseJobFile(JobOptions.LoadJobFilePath);
                        CopyOptions.Merge(jobfile.CopyOptions);
                        LoggingOptions.Merge(jobfile.LoggingOptions);
                        RetryOptions.Merge(jobfile.RetryOptions);
                        SelectionOptions.Merge(jobfile.SelectionOptions);
                        if (String.IsNullOrWhiteSpace(Name)) Name = jobfile.Job_Name;
                    }
                    // Save the job file needed
                    if (Path.IsPathRooted(JobOptions.FilePath))
                    {
                        var jobFile = new JobFile(this, JobOptions.FilePath);
                        await jobFile.Save();
                    }
                    // Run
                    results = await RunAsRoboMover(domain, username, password);
                    success = results != null; 
                }
                
            }
            catch(Exception ex)
            {
                RaiseOnCommandError(new CommandErrorEventArgs(ex));
                throw;
            }
            finally
            {
                if (success)
                {
                    RaiseOnCommandCompleted(results);
                    if (LoggingOptions.ListOnly)
                    {
                        ListOnlyResults = results;
                    }
                    RunResults = results;
                }
                standardCommand = null;
                runningTask = null;
                cancelRequest = null;
                IsRunning = false;
            }
        }

        /// <summary> Run the RoboCommand - used when the source and destination are on separate drives </summary>
        private async Task<RoboCopyResults> RunAsRoboCopy(string domain, string username, string password)
        {
            standardCommand = new RoboCommand(command: this, LinkConfiguration:true, LinkRetryOptions:true, LinkSelectionOptions:true, LinkLoggingOptions:true, LinkJobOptions: false);
            standardCommand.OnCommandCompleted += StandardCommand_OnCommandCompleted;
            standardCommand.OnCommandError += StandardCommand_OnCommandError;
            standardCommand.OnCopyProgressChanged += StandardCommand_OnCopyProgressChanged;
            standardCommand.OnError += StandardCommand_OnError;
            standardCommand.OnFileProcessed += StandardCommand_OnFileProcessed;
            standardCommand.OnProgressEstimatorCreated += StandardCommand_OnProgressEstimatorCreated;

            runningTask = standardCommand.Start(domain, username, password).ContinueWith(t =>
            {
                standardCommand.OnCommandCompleted -= StandardCommand_OnCommandCompleted;
                standardCommand.OnCommandError -= StandardCommand_OnCommandError;
                standardCommand.OnCopyProgressChanged -= StandardCommand_OnCopyProgressChanged;
                standardCommand.OnError -= StandardCommand_OnError;
                standardCommand.OnFileProcessed -= StandardCommand_OnFileProcessed;
                standardCommand.OnProgressEstimatorCreated -= StandardCommand_OnProgressEstimatorCreated;
            });
            await runningTask;
            return standardCommand.GetResults();
        }

        private async Task<RoboCopyResults> RunAsRoboMover(string domain, string username, string password)
        {
            var authResult = Authentication.AuthenticateSourceAndDestination(this, domain, username, password);
            if (!authResult.Success)
            {
                RaiseOnCommandError(authResult.CommandErrorArgs);
                return null;
            }

            // Validate Source & Destination - should be able to create directory objects
            DirectoryInfo source = null; DirectoryInfo dest = null;
            bool ok = true;
            try { source = new DirectoryInfo(CopyOptions.Source); }
            catch (Exception e)
            {
                RaiseOnCommandError("CopyOptions.Source is invalid.", e);
                ok = false;
            }

            try { dest = new DirectoryInfo(CopyOptions.Destination); }
            catch (Exception e)
            {
                RaiseOnCommandError("CopyOptions.Destination is invalid.", e);
                ok = false;
            }
            if (!ok) return null;

            // Move the files
            resultsBuilder = new ResultsBuilder(this);
            base.IProgressEstimator = resultsBuilder.ProgressEstimator;
            RaiseOnProgressEstimatorCreated(resultsBuilder.ProgressEstimator);
            runningTask = ProcessDirectory(new DirectoryPair(source, dest), 1);
            await runningTask;
            return resultsBuilder.GetResults();
        }

        private async Task ProcessDirectory(DirectoryPair directoryPair, int currentDepth)
        {
            var filePairs = directoryPair.EnumerateFilePairs(FilePair.CreatePair);

            // Files
            foreach (var file in filePairs)
            {
                if (cancelRequest.IsCancellationRequested) break;
                try
                {
                    bool shouldMove = Evaluator.ShouldCopyFile(file);
                    bool shouldPurge = Evaluator.ShouldPurge(file);
                    base.RaiseOnFileProcessed(file.ProcessResult);
                    resultsBuilder.AddFile(file.ProcessResult);

                    if (shouldMove)
                    {
                        if (!LoggingOptions.ListOnly)
                        {
                            resultsBuilder.SetCopyOpStarted(file.ProcessResult);
                            File.Move(file.Source.FullName, file.Destination.FullName);
                        }
                        resultsBuilder.AddFileCopied(file.ProcessResult);
                    }
                    else if (file.IsExtra())
                    {
                        if (CopyOptions.Purge && !LoggingOptions.ListOnly)
                        {
                            file.Destination.Delete();
                            resultsBuilder.AddFilePurged(file.ProcessResult);
                        }
                        else
                        {
                            resultsBuilder.AddFileSkipped(file.ProcessResult);
                        }
                    }
                    else
                    {
                        resultsBuilder.AddFileSkipped(file.ProcessResult);
                    }
                }
                catch (Exception e)
                {
                    RaiseOnCommandError(e);
                }
            }

            // Iterate through dirs
            if (!CopyOptions.ExceedsAllowedDepth(currentDepth + 1))
            {
                foreach (var dir in directoryPair.GetDirectoryPairs(DirectoryPair.CreatePair))
                {
                    if (cancelRequest.IsCancellationRequested) break;
                    if (!CopyOptions.IsRecursive()) break;
                    bool processDir = Evaluator.ShouldCopyDir(dir);
                    bool shouldPurge = Evaluator.ShouldPurge(dir);

                    dir.ProcessResult.Size = shouldPurge ? dir.Destination.GetFileSystemInfos().Length : dir.Source.GetFileSystemInfos().Length;
                    resultsBuilder.AddDir(dir.ProcessResult);
                    RaiseOnFileProcessed(dir.ProcessResult);
                    if (shouldPurge)
                    {
                        PurgeDirectory(dir);
                    }
                    else if (processDir)
                    {
                        bool isMoved = false;
                        if (RoboMoverOptions.QuickMove && !dir.Destination.Exists)
                        {
                            try
                            {
                                dir.Source.MoveTo(dir.Destination.FullName);
                                dir.Source.Refresh();
                                isMoved = !dir.Source.Exists;
                            }
                            catch (Exception e)
                            {
                                RaiseOnCommandError("Unable to perform QuickMove on path: " + dir.Source.FullName, e);
                            }
                        }
                        if (!isMoved) await ProcessDirectory(dir, currentDepth + 1);
                    }
                }
            }

            // Delete the source directory as part of the 'Move' command
            if (CopyOptions.MoveFilesAndDirectories && directoryPair.Source.Exists && directoryPair.Source.EnumerateFileSystemInfos().None())
            {
                try
                {
                    directoryPair.Source.Delete();
                }
                catch(Exception e)
                {
                    RaiseOnCommandError(e);
                }
            }
        }

        /// <summary> Purges a directory tree from the destination </summary>
        private void PurgeDirectory(DirectoryPair pair)
        {
            if (!pair.Destination.Exists) return;
            if (RoboMoverOptions.QuickMove)
            {
                pair.Destination.Delete(true);
            }
            else
            {
                foreach (var file in pair.Destination.GetFiles())
                {
                    if (cancelRequest.IsCancellationRequested) break;
                    ProcessedFileInfo pInfo = new ProcessedFileInfo(file, this, ProcessedFileFlag.ExtraFile);
                    try
                    {
                        file.Delete();
                        resultsBuilder.AddFilePurged(pInfo);
                    }
                    catch (Exception e)
                    {
                        RaiseOnCommandError("Unable to purge file : " + file.FullName, e);
                        resultsBuilder.AddFileFailed(pInfo);
                    }
                }
                foreach (var dir in pair.EnumerateDirectoryPairs())
                {
                    if (cancelRequest.IsCancellationRequested) break;
                    PurgeDirectory(dir);
                }
                try
                {
                    if (pair.Destination.Exists && pair.Destination.EnumerateFileSystemInfos().None())
                        pair.Destination.Delete();
                }
                catch (Exception e)
                {
                    RaiseOnCommandError("Unable to purge directory : " + pair.Destination.FullName, e);
                }
            }
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            if (!IsRunning) return;
            try
            {
                cancelRequest?.Cancel();
            }
            finally
            {
                standardCommand?.Stop();
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            standardCommand?.Dispose();
        }

        #region < Event Handlers >

        private void StandardCommand_OnProgressEstimatorCreated(IRoboCommand sender, ProgressEstimatorCreatedEventArgs e)
        {
            this.IProgressEstimator = e.ResultsEstimate;
            base.RaiseOnProgressEstimatorCreated(e.ResultsEstimate);
        }

        private void StandardCommand_OnFileProcessed(IRoboCommand sender, FileProcessedEventArgs e)
        {
            base.RaiseOnFileProcessed(e.ProcessedFile);
        }

        private void StandardCommand_OnError(IRoboCommand sender, RoboSharp.ErrorEventArgs e)
        {
            base.RaiseOnError(e);
        }

        private void StandardCommand_OnCopyProgressChanged(IRoboCommand sender, CopyProgressEventArgs e)
        {
            base.RaiseOnCopyProgressChanged(e);
        }

        private void StandardCommand_OnCommandError(IRoboCommand sender, CommandErrorEventArgs e)
        {
            base.RaiseOnCommandError(e);
        }

        private void StandardCommand_OnCommandCompleted(IRoboCommand sender, RoboCommandCompletedEventArgs e)
        {
            //base.RaiseOnCommandCompleted(e);
        }

        #endregion

    }
}
