using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// <see cref="IFileCopier"/> that uses a <see cref="System.IO.FileStream"/> to perform the copy operation
    /// </summary>
    public class StreamedCopier : AbstractFileCopier
    {
        /// <summary>
        /// The default buffer size used by FileStream.CopyToAsync()
        /// </summary>
        public const int DefaultBufferSize = 81920;

        CancellationTokenSource _cancellationSource;
        bool _isCopying;
        bool _isMoving;
        bool _isPaused;
        bool _wasCancelled;
        DateTime _endDate;
        DateTime _startDate;

        /// <inheritdoc/>
        public StreamedCopier(IFilePair filePair, IDirectoryPair parent = null) : base(filePair, parent)
        {
        }

        /// <inheritdoc/>
        public StreamedCopier(FileInfo source, FileInfo destination, IDirectoryPair parent = null) : base(source, destination, parent)
        {
        }

        /// <inheritdoc/>
        public StreamedCopier(string source, string destination, IDirectoryPair parent = null) : base(source, destination, parent)
        {
        }

        /// <summary>
        /// Set the buffer size used for the copy operation
        /// </summary>
        public int BufferSize { get; set; } = DefaultBufferSize;

        /// <summary>
        /// TRUE is the copier was paused while it was running, otherwise false.
        /// </summary>
        public bool IsPaused
        {
            get { return _isPaused; }
            private set { SetProperty(ref _isPaused, value, nameof(IsPaused)); }
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

        /// <inheritdoc/>
        public override void Cancel()
        {
            if (IsCopying && !(_cancellationSource?.IsCancellationRequested ?? true))
            {
                _cancellationSource?.Cancel();
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> CopyAsync(bool overwrite = false)
        {
            if (_isCopying) throw new InvalidOperationException("Copy Operation already in progress");
            IsCopying = true;
            Refresh();
            if (!Source.Exists) throw new FileNotFoundException("Source File Not Found.", Source.FullName);
            if (!overwrite && Destination.Exists) throw new IOException("The destination already file exists");

            _cancellationSource = new CancellationTokenSource();
            Task updateTask = null;
            long totalBytesRead = 0;
            Progress = 0;
            WasCancelled = false;
            StartDate = DateTime.Now;

            // Progress Reporter
            updateTask = Task.Run(async () =>
            {
                while (!_cancellationSource.IsCancellationRequested && totalBytesRead < Source.Length)
                {
                    OnProgressUpdated((double)100 * totalBytesRead / Source.Length);
                    await Task.Delay(100, _cancellationSource.Token);
                }
            }, _cancellationSource.Token);

            int bSize = BufferSize;
            byte[] buffer = new byte[bSize];
            using var reader = new FileStream(Source.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, bSize, true);
            using var writer = new FileStream(Destination.FullName, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, bSize, true);
            int bytesRead = 0;
            try
            {
                while (!_cancellationSource.IsCancellationRequested && (bytesRead = await reader.ReadAsync(buffer, 0, bSize, _cancellationSource.Token).ConfigureAwait(false)) > 0)
                {
                    await writer.WriteAsync(buffer, 0, bytesRead, _cancellationSource.Token).ConfigureAwait(false);
                    totalBytesRead += bytesRead;
                    while (_isPaused && !_cancellationSource.IsCancellationRequested)
                        await Task.Delay(75).ConfigureAwait(false);
                }
                writer.Dispose();
                reader.Dispose();
                Destination.Refresh();
            }
            catch (OperationCanceledException)
            {
                WasCancelled = true;
                //await reader.FlushAsync().ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
                Destination.Refresh();
                if (totalBytesRead < Source.Length && Destination.Exists)
                    Destination.Delete();
                throw;
            }
            finally
            {
                IsCopying = false;
                IsPaused = false;
                EndDate = DateTime.Now;
                _cancellationSource.Cancel();
                await updateTask.ConfigureAwait(false);
                _isCopying = false;
                reader.Dispose();
                writer.Dispose();
                _cancellationSource.Dispose();
                _cancellationSource = null;
            }
            return Destination.Exists;
        }

        /// <inheritdoc/>
        public override async Task<bool> MoveAsync(bool overwrite = false)
        {
            if (_isMoving) throw new InvalidOperationException("Move Operation already in progress");
            _isMoving = true;
            try
            {
                if (await CopyAsync(overwrite))
                {
                    Source.Delete();
                    Source.Refresh();
                }
                return !Source.Exists && Destination.Exists;
            }
            finally
            {
                _isMoving = false;
            }
        }

        /// <inheritdoc/>
        public override void Pause()
        {
            if (_isCopying && !_isPaused)
                IsPaused = true;
        }

        /// <inheritdoc/>
        public override void Resume()
        {
            if (_isCopying && _isPaused)
                IsPaused = false;
        }
    }
}
