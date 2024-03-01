using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboSharp.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractFileCopier : FilePair, INotifyPropertyChanged, IFileCopier
    {
        private double _progress;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The Progress Updated event
        /// </summary>
        public event EventHandler<CopyProgressEventArgs> ProgressUpdated;


        /// <summary>
        /// Create a new FileCopier from the supplied file paths
        /// </summary>
        /// <inheritdoc cref="FilePair.FilePair(FileInfo, FileInfo, IDirectoryPair)"/>
        protected AbstractFileCopier(FileInfo source, FileInfo destination, IDirectoryPair parent = null) : base(source, destination, parent)
        { }

        /// <summary>
        /// Create a new FileCopier from the supplied file paths
        /// </summary>
        /// <inheritdoc cref="FilePair.FilePair(string, string, IDirectoryPair)"/>
        protected AbstractFileCopier(string source, string destination, IDirectoryPair parent = null) : base(source, destination, parent)
        { }

        /// <summary>
        /// Create a new FileCopier from the provided IFilePair
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <inheritdoc cref="FilePair.FilePair(IFilePair, IDirectoryPair)"/>
        protected AbstractFileCopier(IFilePair filePair, IDirectoryPair parent = null) : base(filePair, parent)
        { }

        /// <inheritdoc/>
        public double Progress
        {
            get { return _progress; }
            protected set { SetProperty(ref _progress, value, nameof(Progress)); }
        }

        /// <summary>
        /// Raise the ProgressUpdated event
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void OnProgressUpdated(double progress)
        {
            Progress = progress;
            if (ProgressUpdated != null)
                OnProgressUpdated(new CopyProgressEventArgs(progress, this.ProcessedFileInfo, this.Parent?.ProcessedFileInfo));
        }

        /// <summary>
        /// Raise the ProgressUpdated event
        /// </summary>
        /// <param name="e"></param>
        protected void OnProgressUpdated(CopyProgressEventArgs e)
        {
            ProgressUpdated?.Invoke(this, e);
        }

        /// <summary>
        /// Cancel the Copy/Move operation
        /// </summary>
        public abstract void Cancel();

        /// <summary>
        /// Begin a task that copies a file asynchronously
        /// </summary>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public abstract Task<bool> CopyAsync(bool overwrite = false);

        /// <summary>
        /// Begin a task that copies a file asynchronously
        /// </summary>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public abstract Task<bool> MoveAsync(bool overwrite = false);

        /// <summary>
        /// Pause the copy operation
        /// </summary>
        public abstract void Pause();

        /// <summary>
        /// Resume the copy operation
        /// </summary>
        public abstract void Resume();

        /// <summary>
        /// Raise PropertyChanged
        /// </summary>
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Set the property and raise PropertyChanged event
        /// </summary>
        protected void SetProperty<T>(ref T field, T value, string propertyName)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
