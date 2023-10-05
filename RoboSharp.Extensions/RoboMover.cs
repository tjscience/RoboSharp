using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using RoboSharp;
using RoboSharp.Extensions;
using RoboSharp.Interfaces;
using RoboSharp.Results;
using RoboSharp.EventArgObjects;
using System.Threading.Tasks;
using System.Threading;
using RoboSharp.Extensions.Helpers;

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
        public RoboMover() : base() { }

        /// <inheritdoc/>
        public RoboMover(
            CopyOptions copyOptions = null,
            LoggingOptions loggingOptions = null,
            RetryOptions retryOptions = null,
            SelectionOptions selectionOptions = null,
            RoboSharpConfiguration configuration = null
             ) : base(copyOptions, loggingOptions, retryOptions, selectionOptions, configuration)
        { }

        /// <inheritdoc/>
        public RoboMover(
            string source, string destination,
            CopyActionFlags copyActionFlags = CopyActionFlags.Default,
            SelectionFlags selectionFlags = SelectionFlags.Default,
            LoggingFlags loggingFlags = LoggingFlags.RoboSharpDefault
             ) : base(source, destination, copyActionFlags, selectionFlags, loggingFlags)
        { }

        /// <summary>
        /// Create a RoboMover object that clones the options of the input IRoboCommand
        /// </summary>
        /// <param name="cmd">The IRoboCommand to convert to a RoboMover object</param>
        public RoboMover(IRoboCommand cmd) : base(cmd)
        { }

        private RoboCommand standardCommand;
        private Task runningTask;
        private CancellationTokenSource cancelRequest;
        private PairEvaluator PairEvaluator;
        private ResultsBuilder resultsBuilder;

        /// <summary>
        /// Object that provides RoboMover specific options
        /// </summary>
        public RoboMoverOptions RoboMoverOptions { get; } = new RoboMoverOptions();

        /// <inheritdoc cref="RoboCommand.JobOptions"/>
        public override JobOptions JobOptions { get; } = new JobOptions();

        // Safeguard against accidental deletion/movement of the following special folders
        static readonly string[] DisallowedRootDirectories = new string[] { 
            "System Volume Information" , 
            "Windows", "System32",
            "Program Files", "Program Files (x86)",
            "Users", "Program Data"
        };

        /// <summary> Check the directory name to determine if it should be ignored, such as the 'System Volume Information' folder.  </summary>
        /// <remarks>Meant to only filter out special root directories.</remarks>
        /// <returns> True if the directory name is not in the list of disallowed names. </returns>
        public static bool IsAllowedRootDirectory(DirectoryInfo d)
        {
            return !DisallowedRootDirectories.Contains(d.Name, StringEqualityComparer.InvariantCultureIgnoreCase);
        }

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

                if (!isMoving | !onSameDrive | CopyOptions.Mirror | JobOptions.PreventCopyOperation | CopyOptions.CreateDirectoryAndFileTree)
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
            catch (Exception ex)
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
                PairEvaluator = null;
                resultsBuilder = null;
                IsRunning = false;
            }
        }

        /// <summary> Run the RoboCommand - used when the source and destination are on separate drives </summary>
        private async Task<RoboCopyResults> RunAsRoboCopy(string domain, string username, string password)
        {
            standardCommand = new RoboCommand(command: this, LinkConfiguration: true, LinkRetryOptions: true, LinkSelectionOptions: true, LinkLoggingOptions: true, LinkJobOptions: false);
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
            PairEvaluator = new PairEvaluator(this);
            resultsBuilder = new ResultsBuilder(this);
            base.IProgressEstimator = resultsBuilder.ProgressEstimator;
            RaiseOnProgressEstimatorCreated(resultsBuilder.ProgressEstimator);
            var sourcePair = new DirectoryPair(source, dest);
            sourcePair.ProcessResult = new ProcessedFileInfo(
                directory: source,
                command: this,
                status: dest.Exists ? ProcessedDirectoryFlag.ExistingDir : ProcessedDirectoryFlag.NewDir,
                size: sourcePair.SourceFiles.Count());

            resultsBuilder.AddDir(sourcePair.ProcessResult);
            runningTask = Task.Run(() => ProcessDirectory(sourcePair, 1));
            await runningTask;
            return resultsBuilder.GetResults();
        }

        private void ProcessDirectory(DirectoryPair directoryPair, int currentDepth)
        {
            bool evaluateExtras = CopyOptions.Purge | LoggingOptions.ReportExtraFiles | LoggingOptions.VerboseOutput;

            // Extra Directories
            if (CopyOptions.Depth != 1 && (PairEvaluator.CanDigDeeper(currentDepth) | evaluateExtras))
            {
                IEnumerable<DirectoryPair> childDirs = directoryPair.IsRootDestination() ?
                    directoryPair.ExtraDirectories.Where(d => IsAllowedRootDirectory(d.Source)) :
                    directoryPair.ExtraDirectories;

                foreach (var extraDir in childDirs)
                {
                    if (cancelRequest.IsCancellationRequested) break;
                    ProcessExtraDirectory(extraDir, currentDepth + 1);
                }
            }

            // Extra Files
            if (evaluateExtras)
            {
                foreach (var extraFile in directoryPair.ExtraFiles)
                {
                    if (cancelRequest.IsCancellationRequested) break;
                    ProcessExtraFile(extraFile);
                }
            }

            // Source Files
            IEnumerable<FilePair> sourceFiles = LoggingOptions.ReportExtraFiles ? directoryPair.SourceFiles : PairEvaluator.FilterFilePairs(directoryPair.SourceFiles);
            foreach (var file in sourceFiles)
            {
                if (cancelRequest.IsCancellationRequested) break;
                ProcessSourceFile(file);
            }

            // Iterate through source dirs
            if (PairEvaluator.CanDigDeeper(currentDepth))
            {
                IEnumerable<DirectoryPair> childDirs = directoryPair.IsRootSource() ?
                    directoryPair.SourceDirectories.Where(d => IsAllowedRootDirectory(d.Source)) :
                    directoryPair.SourceDirectories;

                foreach (var dir in childDirs)
                {
                    if (cancelRequest.IsCancellationRequested) break;
                    bool processDir = PairEvaluator.ShouldCopyDir(dir);
                    _ = dir.TrySetSizeAndPath(false);

                    resultsBuilder.AddDir(dir.ProcessResult);
                    RaiseOnFileProcessed(dir.ProcessResult);
                    
                    if (processDir)
                    {
                        bool isMoved = false;
                        if (RoboMoverOptions.QuickMove && !dir.Destination.Exists)
                        {
                            try
                            {
                                dir.Source.MoveTo(dir.Destination.FullName);
                                dir.Refresh();
                                isMoved = !dir.Source.Exists;
                            }
                            catch (Exception e)
                            {
                                RaiseOnCommandError("Unable to perform QuickMove on path: " + dir.Source.FullName, e);
                            }
                        }
                        if (!isMoved) ProcessDirectory(dir, currentDepth + 1);
                    }
                }
            }

            // Delete the source directory as part of the 'Move' command
            if (
                !LoggingOptions.ListOnly &&
                CopyOptions.MoveFilesAndDirectories &&
                !directoryPair.IsRootSource() &&
                directoryPair.Source.Exists &&
                directoryPair.Source.EnumerateFileSystemInfos().None()
                )
            {
                try
                {
                    directoryPair.Source.Delete();
                }
                catch (Exception e)
                {
                    RaiseOnCommandError(e);
                }
            }
        }

        /// <summary> Processes an EXTRA directory tree from the destination, potentially purging it.</summary>
        private void ProcessExtraDirectory(DirectoryPair pair, int currentDepth) 
        {
            if (!pair.Destination.Exists) return;
            bool isRootDir = pair.Destination.IsRootDir();
            bool shouldPurge = PairEvaluator.ShouldPurge(pair);

#if NETFRAMEWORK || NETSTANDARD
            if (pair.ProcessResult is null)
                pair.ProcessResult = new ProcessedFileInfo(directory: pair.Destination, this, ProcessedDirectoryFlag.ExtraDir);
#else
            pair.ProcessResult ??= new ProcessedFileInfo(directory: pair.Destination, this, ProcessedDirectoryFlag.ExtraDir);
#endif
            resultsBuilder.AddDir(pair.ProcessResult);

            if (CopyOptions.Purge | LoggingOptions.ReportExtraFiles)
            {
                //Process Files
                IEnumerable<FilePair> files = pair.ExtraFiles;
                foreach (var file in files)
                {
                    if (cancelRequest.IsCancellationRequested) break;
                    ProcessExtraFile(file);
                }


                // Dig into subdirectories
                if (PairEvaluator.CanDigDeeper(currentDepth))
                {
                    foreach (var dir in pair.ExtraDirectories)
                    {
                        if (cancelRequest.IsCancellationRequested) break;
                        ProcessExtraDirectory(dir, currentDepth + 1);
                    }
                }
            }
            if (CopyOptions.Purge)
            {
                // Delete the current directory
                try
                {
                    if (shouldPurge && pair.Destination.Exists && !isRootDir && pair.Destination.EnumerateFileSystemInfos().None())
                        pair.Destination.Delete();
                }
                catch (Exception e)
                {
                    RaiseOnCommandError("Unable to purge directory : " + pair.Destination.FullName, e);
                }
            }
        }

        private void ProcessExtraFile(FilePair extraFile)
        {
#if NETFRAMEWORK || NETSTANDARD
            if (extraFile.ProcessResult is null)
                extraFile.ProcessResult = new ProcessedFileInfo(file: extraFile.Destination, this, ProcessedFileFlag.ExtraFile);
#else
            extraFile.ProcessResult ??= new ProcessedFileInfo(file: extraFile.Destination, this, ProcessedFileFlag.ExtraFile);
#endif
            bool shouldPurge = PairEvaluator.ShouldPurge(extraFile);
            if (shouldPurge && !LoggingOptions.ListOnly)
            {
                try
                {
                    extraFile.Destination.Delete();
                    resultsBuilder.AddFilePurged(extraFile.ProcessResult);
                }
                catch (Exception e)
                {
                    RaiseOnCommandError("Unable to Delete File : " + extraFile.ProcessResult.Name, e);
                }
            }
            else
            {
                resultsBuilder.AddFileExtra(extraFile.ProcessResult);
            }
        }

        private void ProcessSourceFile(FilePair file)
        {
            try
            {
                bool shouldMove = PairEvaluator.ShouldCopyFile(file);
                base.RaiseOnFileProcessed(file.ProcessResult);

                if (shouldMove)
                {
                    if (!LoggingOptions.ListOnly)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(file.Destination.FullName));
                        resultsBuilder.SetCopyOpStarted(file.ProcessResult);
                        if (file.Destination.Exists) file.Destination.Delete();
                        File.Move(file.Source.FullName, file.Destination.FullName);
                    }
                    resultsBuilder.AddFileCopied(file.ProcessResult);
                }
                else
                {
                    resultsBuilder.AddFileSkipped(file.ProcessResult);
                }
            }
            catch (Exception e)
            {
                RaiseOnCommandError("Unable to Move File : " + file.ProcessResult.Name, e);
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
