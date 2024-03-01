using System;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using RoboSharp.Extensions.CopyFileEx;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// Class that extends <see cref="FilePair"/> to implement Copy/Move async methods via CopyFileEx
    /// </summary>
    public class FileCopierEx : AbstractFileCopier
    {
        private bool _isCopied;
        private bool _isCopying;
        private bool _isPaused;
        private bool _isMoving;
        private bool _wasCancelled;
        private double _progress;
        private DateTime _startDate;
        private DateTime _endDate;
        private bool _disposed;
        private CancellationTokenSource _cancellationSource;

        /// <summary>
        /// Create a new FileCopier from the supplied file paths
        /// </summary>
        /// <inheritdoc cref="FilePair.FilePair(FileInfo, FileInfo, IDirectoryPair)"/>
        public FileCopierEx(FileInfo source, FileInfo destination, IDirectoryPair parent = null) : base(source, destination, parent)
        { }

        /// <summary>
        /// Create a new FileCopier from the supplied file paths
        /// </summary>
        /// <inheritdoc cref="FilePair.FilePair(string, string, IDirectoryPair)"/>
        public FileCopierEx(string source, string destination, IDirectoryPair parent = null) : base(source, destination, parent)
        { }

        /// <summary>
        /// Create a new FileCopier from the provided IFilePair
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <inheritdoc cref="FilePair.FilePair(IFilePair, IDirectoryPair)"/>
        public FileCopierEx(IFilePair filePair, IDirectoryPair parent = null) : base(filePair, parent)
        { }

        /// <summary>
        /// The options to use when performing the copy operation - Note : Does not apply to move operations.
        /// </summary>
        /// <remarks>
        ///  - <see cref="CopyFileExOptions.FAIL_IF_EXISTS"/> is set by the 'overwrite' parameter of the <see cref="CopyAsync(bool)"/> method
        ///  <br/> - <see cref="CopyFileExOptions.RESTARTABLE"/> must be set here if you wish to enable the <see cref="Pause"/> functionality.
        /// </remarks>
        public CopyFileExOptions CopyOptions { get; set; }

        /// <summary>
        /// TRUE is the copier was paused while it was running, otherwise false.
        /// </summary>
        public bool IsPaused
        {
            get { return _isPaused; }
            private set { SetProperty(ref _isPaused, value, nameof(IsPaused)); }
        }

        /// <summary>
        /// Copied Status -> True if the copy action has been performed.
        /// </summary>
        public bool IsCopied
        {
            get { return _isCopied; }
            private set { SetProperty(ref _isCopied, value, nameof(IsCopied)); }
        }

        /// <inheritdoc/>
        public bool IsCopying
        {
            get { return _isCopying; }
            private set { SetProperty(ref _isCopying, value, nameof(IsCopying)); }
        }

        /// <inheritdoc/>
        public bool WasCancelled
        {
            get { return _wasCancelled; }
            private set { SetProperty(ref _wasCancelled, value, nameof(WasCancelled)); }
        }

        /// <summary> </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            private set { SetProperty(ref _startDate, value, nameof(StartDate)); }
        }

        /// <summary> </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            private set
            {
                SetProperty(ref _endDate, value, nameof(EndDate));
                OnPropertyChanged(nameof(TimeToCompletion));
            }
        }

        /// <summary> 
        /// The time it took to complete the operation. 
        /// </summary>
        public TimeSpan TimeToCompletion
        {
            get
            {
                if (EndDate > StartDate) return EndDate - StartDate;
                return new TimeSpan();
            }
        }

        #region < Pause / Resume / Cancel >

        /// <summary>
        /// Stops the COPY operation using the 'STOP' argument. The copy task then enters an 'await Task.Delay(100)' loop until resumed or cancelled. Warning : During this period the file is not locked as CopyFileEx has released its hold.
        /// Upon being resumed, CopyFileEx will attempt to resume where it left off. 
        /// <para/> Only possible when <see cref="CopyFileExOptions.RESTARTABLE"/> is specified in the <see cref="CopyOptions"/>
        /// <br/> No effect on the MOVE operation.
        /// </summary>
        public override void Pause()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FileCopierEx));
            if (IsCopying && !_isMoving)
                IsPaused = CopyOptions.HasFlag(CopyFileExOptions.RESTARTABLE);
        }

        /// <summary>
        /// Resume a COPY operation
        /// </summary>
        public override void Resume()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FileCopierEx));
            if (IsCopying && IsPaused)
            {
                IsPaused = false;
            }
        }

        /// <summary>
        /// Determine if the copy operation can currently be cancelled
        /// </summary>
        /// <returns>TRUE if the operation is running and has not yet been cancelled. Otherwise false.</returns>
        public bool CanCancel() => !_disposed && IsCopying && !(_cancellationSource?.IsCancellationRequested ?? true);

        /// <summary>
        /// Determine if the copy operation can current be started
        /// </summary>
        /// <returns>TRUE if the operation can be started, FALSE is the object is disposed / currently copying</returns>
        public bool CanStart() => !_disposed && !IsCopying;

        /// <summary>
        /// Request Cancellation immediately.
        /// </summary>
        public override void Cancel()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FileCopierEx));
            if (CanCancel())
            {
                _cancellationSource?.Cancel();
            }
        }

        /// <summary>
        /// Request Cancellation after a number of <paramref name="milliseconds"/>
        /// </summary>
        public async void Cancel(int milliseconds)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FileCopierEx));
            if (CanCancel())
            {
                await Task.Delay(milliseconds);
                Cancel();
            }
        }

        #endregion

        /// <summary>
        /// Sets up the flags that allow the read/write tasks to run.
        /// </summary>
        private void SetStarted()
        {
            _cancellationSource = new CancellationTokenSource();
            StartDate = DateTime.Now;
            IsCopied = false;
            IsCopying = true;
            IsPaused = false;
            WasCancelled = false;
            Progress = 0;
        }

        /// <summary>
        /// Set <see cref="IsCopying"/> to FALSE <br/>
        /// set <see cref="EndDate"/> <br/>
        /// Dospose of cancellation token
        /// </summary>
        /// <param name="isCopied">set <see cref="IsCopied"/></param>
        private void SetEnded(bool isCopied)
        {
            IsCopying = false;
            WasCancelled = _cancellationSource?.IsCancellationRequested ?? false;
            IsCopied = isCopied;
            IsPaused = false;
            EndDate = DateTime.Now;
            _cancellationSource?.Dispose();
            _cancellationSource = null;
        }

        /// <inheritdoc cref="FileFunctions.CopyFileAsync(string, string, IProgress{double}, CopyFileExOptions, int, CancellationToken)"/>
        public override async Task<bool> CopyAsync(bool overwrite = false)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FileCopierEx));
            if (IsCopying) throw new InvalidOperationException("Copy/Move Operation Already in progress!");

            Source.Refresh();
            Destination.Refresh();
            SetStarted();
            
            bool copied = false;
            try
            {
                var options = overwrite ? CopyOptions &= ~CopyFileExOptions.FAIL_IF_EXISTS : CopyOptions | CopyFileExOptions.FAIL_IF_EXISTS;
                while (!_cancellationSource.IsCancellationRequested)
                {
                    if (IsPaused)
                    {
                        await Task.Delay(100);
                    }
                    else
                    {
                        bool result = await PerformCopy(options);
                        if (result) return true;
                    }
                }
            }
            finally
            {
                SetEnded(copied);
                if (copied)
                {
                    Source.Refresh();
                    Destination.Refresh();
                    if (Progress != 100) OnProgressUpdated(100);
                }
            }
            return IsCopied;
        }

        private async Task<bool> PerformCopy(CopyFileExOptions options)
        {
            bool result =false;
            Task updateTask = null;
            long fileSize = Source.Length;
            long totalBytesRead = 0;

            // Updater - asynchronous background task
            var updateToken = CancellationTokenSource.CreateLinkedTokenSource(_cancellationSource.Token);
            updateTask = Task.Run(async () =>
               {
                   while (totalBytesRead < Source.Length)
                   {
                       OnProgressUpdated((double)100 * totalBytesRead / fileSize);
                       await Task.Delay(100, updateToken.Token);
                       updateToken.Token.ThrowIfCancellationRequested();
                   }
               }, updateToken.Token);

            //Writer - consumes a thread
            try
            {
                var callback = FileFunctions.CreateCallback(progressRecorder, _cancellationSource.Token);
                result = await FileFunctions.CopyFileAsync(Source.FullName, Destination.FullName, options, callback, token: _cancellationSource.Token).ConfigureAwait(false);
            }
            catch(OperationCanceledException) when (IsPaused) { }
            finally
            {
                updateToken.Cancel();
                await updateTask.ContinueWith(t => { }).ConfigureAwait(false);
            }
            return result;

            CopyProgressCallbackResult progressRecorder(long size, long copied)
            {
                fileSize = size;
                totalBytesRead = copied;
                return IsPaused ? CopyProgressCallbackResult.STOP : CopyProgressCallbackResult.CONTINUE;
            }
        }


        /// <inheritdoc cref="FileFunctions.MoveFileAsync(string, string, IProgress{double}, int, bool, CancellationToken)"/>
        public override async Task<bool> MoveAsync(bool overWrite = false)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FileCopierEx));
            if (IsCopying) throw new InvalidOperationException("Copy/Move Operation Already in progress!");

            if (!File.Exists(Source.FullName))
            {
                throw new FileNotFoundException("File Not Found", Source.FullName);
            }
            bool destExists = File.Exists(Destination.FullName);
            if (destExists && !overWrite)
            {
                throw new IOException("Destination already exists");
            }

            SetStarted();
            _isMoving = true;
            bool moved = false;
            try
            {
                //Check if Source & Destination are on same physical drive
                if (this.IsLocatedOnSameDrive())
                {
                    Directory.CreateDirectory(Destination.DirectoryName);
                    if (destExists) Destination.Delete();
                    File.Move(Source.FullName, Destination.FullName);
                    moved = true;
                }
                else
                {
                    moved = await FileFunctions.MoveFileAsync(Source.FullName, Destination.FullName, new Progress<double>(OnProgressUpdated), 100, overWrite, _cancellationSource.Token);
                }
            }
            finally
            {
                _isMoving = false;
                SetEnded(isCopied: moved);
                if (moved)
                {
                    Source.Refresh();
                    Destination.Refresh();
                    if (Progress != 100) OnProgressUpdated(100);
                }
            }
            return moved;
        }

#region < Dispose >

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                Cancel();
                _cancellationSource?.Dispose();
                _cancellationSource = null;

                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        /// <summary>
        /// 
        /// </summary>
        ~FileCopierEx()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region < Static >

        /// <inheritdoc cref="FileCopierEx.FileCopierEx(FileInfo, FileInfo, IDirectoryPair)"/>
        public static FileCopierEx CreateCopier(FileInfo source, FileInfo destination, IDirectoryPair parent) => new FileCopierEx(source, destination, parent);

        /// <inheritdoc cref="FileCopierEx.FileCopierEx(FileInfo, FileInfo, IDirectoryPair)"/>
        public static FileCopierEx CreateCopier(FileInfo source, FileInfo destination, IProcessedDirectoryPair parent) => new FileCopierEx(source, destination, parent);

        /// <summary>Create a new FileCopier from the supplied file paths</summary>
        /// <inheritdoc cref="EvaluateSource(string)"/>
        /// <inheritdoc cref="EvaluateDestination(string)"/>
        /// <inheritdoc cref="FileCopierEx.FileCopierEx(FileInfo, FileInfo, IDirectoryPair)"/>
        public static FileCopierEx FromSourceAndDestination(string source, string destination, IDirectoryPair parent = null)
        {
            EvaluateSource(source);
            EvaluateDestination(destination);
            var sourceFile = new FileInfo(source);
            var destFile = new FileInfo(destination);
            if (parent is null) parent = new DirectoryPair(sourceFile.Directory, destFile.Directory);
            return new FileCopierEx(sourceFile, destFile, parent);
        }

        /// <summary>Create a new FileCopier from the supplied file paths</summary>
        /// <param name="destination">The Destination Directory</param>
        /// <inheritdoc cref="EvaluateSource(string)"/>
        /// <inheritdoc cref="FileCopierEx.FileCopierEx(FileInfo, FileInfo, IDirectoryPair)"/>
        /// <param name="parent"/><param name="source"/>
        public static FileCopierEx FromSourceAndDestination(string source, DirectoryInfo destination, IDirectoryPair parent = null)
        {
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            EvaluateSource(source);
            var sourceFile = new FileInfo(source);
            var destFile = new FileInfo(Path.Combine(destination.FullName, sourceFile.Name));
            if (parent is null) parent = new DirectoryPair(sourceFile.Directory, destination);
            return new FileCopierEx(sourceFile, destFile, parent);
        }

        /// <summary>Create a new FileCopier from the supplied file paths</summary>
        /// <param name="destination">The Destination Directory</param>
        /// <inheritdoc cref="FileCopierEx.FileCopierEx(FileInfo, FileInfo, IDirectoryPair)"/>
        /// <param name="parent"/><param name="source"/>
        public static FileCopierEx FromSourceAndDestination(FileInfo source, DirectoryInfo destination, IDirectoryPair parent = null)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            var destFile = new FileInfo(Path.Combine(destination.FullName, source.Name));
            if (parent is null) parent = new DirectoryPair(source.Directory, destination);
            return new FileCopierEx(source, destFile, parent);
        }

        /// <summary>
        /// Evaluate the <paramref name="source"/> Path to ensure its a fully qualified file path
        /// </summary>
        /// <param name="source">Fully Qualified Source File Path</param>
        /// <inheritdoc cref="EvaluateDestination(string)"/>
        public static void EvaluateSource(string source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("No Source Path Specified", nameof(source));
            if (!Path.IsPathRooted(source)) throw new ArgumentException("Source Path is not rooted", nameof(source));
            if (string.IsNullOrEmpty(Path.GetFileName(source))) throw new ArgumentException("No FileName Provided in Source", nameof(source));
        }

        /// <summary>
        /// Evaluate the <paramref name="destination"/> Path to ensure its a fully qualified file path.
        /// </summary>
        /// <param name="destination">Fully Qualified Destination File Path</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public static void EvaluateDestination(string destination)
        {
            if (destination is null) throw new ArgumentNullException(nameof(destination));
            if (string.IsNullOrWhiteSpace(destination)) throw new ArgumentException("No Destination Path Specified", nameof(destination));
            if (!Path.IsPathRooted(destination)) throw new ArgumentException("Destination Path is not rooted", nameof(destination));
            if (string.IsNullOrEmpty(Path.GetFileName(destination))) throw new ArgumentException("No Destination FileName Provided", nameof(destination));
        }

        /// <returns>TRUE if the path is fully qualified, otherwise false.</returns>
        /// <inheritdoc cref="EvaluateDestination(string)"/>
        public static bool TryEvaluateDestination(string destination, out Exception ex)
        {
            ex = null;
            try { EvaluateDestination(destination); return true; } catch (Exception e) { ex = e; return false; }
        }

        /// <returns>TRUE if the path is fully qualified, otherwise false.</returns>
        /// <inheritdoc cref="EvaluateSource(string)"/>
        public static bool TryEvaluateSource(string source, out Exception ex)
        {
            ex = null;
            try { EvaluateSource(source); return true; } catch (Exception e) { ex = e; return false; }
        }


        #endregion
    }
}