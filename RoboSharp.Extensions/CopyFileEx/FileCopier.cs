using RoboSharp;
using RoboSharp.Interfaces;
using RoboSharp.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using RoboSharp.Extensions;
using System.Collections.Concurrent;

namespace RoboSharp.Extensions.CopyFileEx
{
    /// <summary>
    /// An File-Copier that reports file copy progress, and optimizes the 'Move' functionality if the file exists on the same drive.
    /// </summary>
    public class FileCopier : FilePair, IFileCopier, INotifyPropertyChanged
    {

        /// <summary>
        /// Create a new FileCopier from the supplied file paths
        /// </summary>
        /// <inheritdoc cref="FilePair.FilePair(FileInfo, FileInfo, IDirectoryPair)"/>
        public FileCopier(FileInfo source, FileInfo destination, IDirectoryPair parent) : base(source, destination, parent)
        { }

        /// <summary>
        /// Create a new FileCopier from the provided IFilePair
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public FileCopier(IFilePair filePair) : base(filePair.Source, filePair.Destination, filePair.Parent)
        { }

        /// <inheritdoc/>
        public event EventHandler<CopyProgressEventArgs> CopyProgressUpdated;
        
        /// <inheritdoc/>
        public event EventHandler<FileCopyCompletedEventArgs> CopyCompleted;
        
        /// <inheritdoc/>
        public event EventHandler<FileCopyFailedEventArgs> CopyFailed;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;


        #region < Static >

        /// <inheritdoc cref="FileCopier.FileCopier(FileInfo, FileInfo, IDirectoryPair)"/>
        public static FileCopier CreateCopier(FileInfo source, FileInfo destination, IDirectoryPair parent)
            => new FileCopier(source, destination, parent);

        /// <inheritdoc cref="FileCopier.FileCopier(FileInfo, FileInfo, IDirectoryPair)"/>
        /// <inheritdoc cref="EvaluateSource(string)"/>
        /// <inheritdoc cref="EvaluateDestination(string)"/>
        public static FileCopier FromPaths(string source, string destination, IDirectoryPair parent)
        {
            EvaluateSource(source);
            EvaluateDestination(destination);
            return new FileCopier(new FileInfo(source), new FileInfo(destination), parent ?? new DirectoryPair(source, destination));
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

        private bool IsCopiedField;
        private bool IsCopyingField;
        private bool WasCancelledField;
        private double ProgressField;
        private DateTime StartDateField;
        private DateTime EndDateField;
        private bool disposedValue;
        private CancellationTokenSource CancellationSource;
        private bool refCancelField;
        private Exception lastExceptionField;

        /// <summary>
        /// File Size in bytes
        /// </summary>
        public long Bytes => Source.Exists ? Source.Length : Destination.Length;

        /// <summary>
        /// TRUE is the copier was paused while it was running, otherwise false.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Copied Status -> True if the copy action has been performed.
        /// </summary>
        public bool IsCopied
        {
            get { return IsCopiedField; }
            private set { SetProperty(ref IsCopiedField, value, nameof(IsCopied)); }
        }

        /// <inheritdoc/>
        public bool IsCopying
        {
            get { return IsCopyingField; }
            private set { SetProperty(ref IsCopyingField, value, nameof(IsCopying)); }
        }

        /// <inheritdoc/>
        public bool WasCancelled
        {
            get { return WasCancelledField; }
            private set { SetProperty(ref WasCancelledField, value, nameof(WasCancelled)); }
        }

        /// <inheritdoc cref="CopyFileExFlags.COPY_FILE_REQUEST_COMPRESSED_TRAFFIC"/>
        public bool EnableCompression { get; set; }

        /// <inheritdoc cref="CopyFileExFlags.COPY_FILE_NO_BUFFERING"/>
        public bool NoBuffering { get; set; }

        /// <inheritdoc/>
        public double Progress
        {
            get { return ProgressField; }
            set { SetProperty(ref ProgressField, value, nameof(Progress)); }
        }
        
        /// <summary> </summary>
        public DateTime StartDate
        {
            get { return StartDateField; }
            private set { SetProperty(ref StartDateField, value, nameof(StartDate)); }
        }

        /// <summary> </summary>
        public DateTime EndDate
        {
            get { return EndDateField; }
            private set
            {
                SetProperty(ref EndDateField, value, nameof(EndDate));
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

        /// <summary> 
        /// The last exception that occured as part of a copy/move operation. 
        /// <br/> Expected to be null unless a copy/move operation failed 
        /// </summary>
        public Exception LastException { 
            get => lastExceptionField; 
            private set => SetProperty(ref lastExceptionField, value, nameof(LastException));
        }

        /// <summary> Raises the FileCopyProgressUpdated event </summary>
        protected virtual void OnFileCopyProgressUpdated(double progress)
        {
            Progress = progress;
            CopyProgressUpdated?.Invoke(this, new CopyProgressEventArgs(progress, ProcessResult, Parent.ProcessResult));
        }

        /// <summary> Raises the FileCopyCompleted event </summary>
        protected virtual void OnFileCopyCompleted()
        {
            CopyCompleted?.Invoke(this, new FileCopyCompletedEventArgs(this, StartDate, EndDate));
        }

        /// <summary> Raises the FileCopyFailed event </summary>
        protected virtual void OnFileCopyFailed(string error = "", Exception e = null, bool cancelled = false, bool failed = false)
        {
            CopyFailed?.Invoke(this, new FileCopyFailedEventArgs(this, error, e, failed, cancelled));
        }

        /// <summary> Raises the PropertyChanged event </summary>
        protected virtual void OnPropertyChanged(string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary> Set the property and raise PropertyChanged </summary>
        protected void SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (!field.Equals(value))
            {
                field = value;
                OnPropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Process the callback from CopyFileEx
        /// </summary>
        /// <inheritdoc cref="DllHooks.ProgressCallback"/>
        private CopyProgressCallbackResult CopyProgressHandler(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, object lpData)
        {
            //when a chunk is finished call the progress changed.
            if (dwCallbackReason == CopyProgressCallbackReason.CALLBACK_CHUNK_FINISHED)
            {
                OnFileCopyProgressUpdated(TotalBytesTransferred / (double)TotalFileSize * 100.0);
            }
            return CancellationSource.IsCancellationRequested ? CopyProgressCallbackResult.PROGRESS_CANCEL : CopyProgressCallbackResult.PROGRESS_CONTINUE;
        }

        #region < Pause / Resume / Cancel >

        /// <summary>
        /// Pause the copy action
        /// </summary>
        public void Pause()
        {
            if (IsCopying)
                IsPaused = true;
        }

        /// <summary>
        /// Resume if paused
        /// </summary>
        public void Resume()
        {
            if (IsCopying && IsPaused)
                IsPaused = false;
        }

        /// <summary>
        /// Determine if the copy operation can currently be cancelled
        /// </summary>
        /// <returns>TRUE if the operation is running and has not yet been cancelled. Otherwise false.</returns>
        public bool CanCancel() => !disposedValue && IsCopying && !(CancellationSource?.IsCancellationRequested ?? true);

        /// <summary>
        /// Determine if the copy operation can current be started
        /// </summary>
        /// <returns>TRUE if the operation can be started, FALSE is the object is disposed / currently copying</returns>
        public bool CanStart() => !disposedValue && !IsCopying;

        /// <summary>
        /// Request Cancellation immediately.
        /// </summary>
        public void Cancel()
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(FileCopier));
            if (CanCancel())
            {
                CancellationSource?.Cancel();
                refCancelField = true;
            }
        }

        /// <summary>
        /// Request Cancellation after a number of <paramref name="milliseconds"/>
        /// </summary>
        public async void CancelAfter(int milliseconds)
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(FileCopier));
            if (CanCancel())
            {
                await Task.Delay(milliseconds);
                Cancel();
            }
        }

        #endregion

        private const CopyFileExFlags COPY_FLAGS = CopyFileExFlags.COPY_FILE_RESTARTABLE | CopyFileExFlags.COPY_FILE_ALLOW_DECRYPTED_DESTINATION;
        private const CopyFileExFlags COPY_FLAGS_NO_OVERWRITE = COPY_FLAGS | CopyFileExFlags.COPY_FILE_FAIL_IF_EXISTS;

        /// <summary>
        /// Sets up the flags that allow the read/write tasks to run.
        /// </summary>
        private void SetStarted()
        {
            CancellationSource = new CancellationTokenSource();
            StartDate = DateTime.Now;
            IsCopied = false;
            IsCopying = true;
            WasCancelled = false;
            refCancelField = false;
        }

        /// <summary>
        /// Set <see cref="IsCopying"/> to FALSE <br/>
        /// set <see cref="EndDate"/> <br/>
        /// Dospose of cancellation token
        /// </summary>
        /// <param name="wasCancelled">set <see cref="WasCancelled"/></param>
        /// <param name="isCopied">set <see cref="IsCopied"/></param>
        private void SetEnded(bool wasCancelled, bool isCopied)
        {
            IsCopying = false;
            WasCancelled = wasCancelled;
            IsCopied = isCopied;
            EndDate = DateTime.Now;
            CancellationSource.Dispose();
            CancellationSource = null;
        }

        /// <inheritdoc cref="DllHooks.CopyFile(string, string, DllHooks.ProgressCallback, ref bool, object, CopyFileExFlags)"/>
        public async Task<bool> Copy(bool overwrite = false)
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(FileCopier));
            if (IsCopying) throw new InvalidOperationException("Copy/Move Operation Already in progress!");

            Source.Refresh();
            Destination.Refresh();
            SetStarted();
            if (!overwrite && File.Exists(Destination.FullName))
            {
                OnFileCopyFailed("Destination file already exists", cancelled: true);
                SetEnded(false, false);
                return false;
            }

            var flags = overwrite ? COPY_FLAGS : COPY_FLAGS_NO_OVERWRITE;
            if (NoBuffering) flags |= CopyFileExFlags.COPY_FILE_NO_BUFFERING;
            if (EnableCompression) flags |= CopyFileExFlags.COPY_FILE_REQUEST_COMPRESSED_TRAFFIC;
            bool success = false;
            try
            {
                success = await Task.Run(() => DllHooks.CopyFile(
                    Source.FullName,
                    Destination.FullName,
                    CopyProgressHandler,
                    ref refCancelField,
                    null,
                    flags));
            }
            catch (Exception e)
            {
                EndDate = DateTime.Now;
                LastException = e;
                OnFileCopyFailed(e.Message, e, failed: true);
            }
            finally
            {
                SetEnded(refCancelField, success);
                Source.Refresh();
                Destination.Refresh();
                if (success) OnFileCopyCompleted();
            }
            return IsCopied;
        }


        /// <inheritdoc cref="DllHooks.MoveFile(string, string, DllHooks.ProgressCallback, object, MoveFileFlags)"/>
        public async Task<bool> Move(bool overWrite = false)
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(FileCopier));
            if (IsCopying) throw new InvalidOperationException("Copy/Move Operation Already in progress!");

            if (!File.Exists(Source.FullName))
            {
                OnFileCopyFailed("Source does not exist", failed: true);
                return false;
            }
            bool destExists = File.Exists(Destination.FullName);
            if (destExists && !overWrite)
            {
                OnFileCopyFailed("Destination file already exists", failed: true);
                return false;
            }

            SetStarted();
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
                    moved = await Task.Run(() => DllHooks.MoveFile(
                    Source.FullName,
                    Destination.FullName,
                    CopyProgressHandler,
                    null,
                    MoveFileFlags.Default));
                }
            }
            catch (Exception e)
            {
                EndDate = DateTime.Now;
                LastException = e;
                OnFileCopyFailed(e.Message, e, failed: true);
            }
            finally
            {
                SetEnded(wasCancelled: CancellationSource?.IsCancellationRequested ?? false, isCopied: moved);
                if (moved)
                {
                    Source.Refresh();
                    Destination.Refresh();
                    OnFileCopyProgressUpdated(100);
                    OnFileCopyCompleted();
                }
            }
            return moved;
        }


        #region < Dispose >

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    refCancelField = true;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                Cancel();
                CancellationSource?.Dispose();
                CancellationSource = null;

                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        /// <summary>
        /// 
        /// </summary>
        ~FileCopier()
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
    }
}