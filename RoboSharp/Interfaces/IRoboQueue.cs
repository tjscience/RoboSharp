using RoboSharp.Interfaces;
using RoboSharp.Results;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RoboSharp.Interfaces
{
    /// <summary>
    /// Interface for RoboQueue
    /// </summary>
    public interface IRoboQueue : IDisposable, INotifyPropertyChanged, IEnumerable<IRoboCommand>
    {
        #region < Properties >

        /// <inheritdoc cref="RoboQueue.AnyCancelled"/>
        bool AnyCancelled { get; }

        /// <inheritdoc cref="RoboQueue.AnyPaused"/>
        bool AnyPaused { get; }

        /// <inheritdoc cref="RoboQueue.AnyRunning"/>
        bool AnyRunning { get; }

        /// <inheritdoc cref="RoboQueue.Commands"/>
        ReadOnlyCollection<IRoboCommand> Commands { get; }

        /// <inheritdoc cref="RoboQueue.CopyOperationCompleted"/>
        bool CopyOperationCompleted { get; }

        /// <inheritdoc cref="RoboQueue.IsCopyOperationRunning"/>
        bool IsCopyOperationRunning { get; }

        /// <inheritdoc cref="RoboQueue.IsListOnlyRunning"/>
        bool IsListOnlyRunning { get; }

        /// <inheritdoc cref="RoboQueue.IsPaused"/>
        bool IsPaused { get; }

        /// <inheritdoc cref="RoboQueue.IsRunning"/>
        bool IsRunning { get; }

        /// <inheritdoc cref="RoboQueue.JobsComplete"/>
        int JobsComplete { get; }

        /// <inheritdoc cref="RoboQueue.JobsCompletedSuccessfully"/>
        int JobsCompletedSuccessfully { get; }

        /// <inheritdoc cref="RoboQueue.JobsCurrentlyRunning"/>
        int JobsCurrentlyRunning { get; }

        /// <inheritdoc cref="RoboQueue.JobsStarted"/>
        int JobsStarted { get; }

        /// <inheritdoc cref="RoboQueue.ListCount"/>
        int ListCount { get; }

        /// <inheritdoc cref="RoboQueue.ListOnlyCompleted"/>
        bool ListOnlyCompleted { get; }

        /// <inheritdoc cref="RoboQueue.ListResults"/>
        IRoboQueueResults ListResults { get; }

        /// <inheritdoc cref="RoboQueue.MaxConcurrentJobs"/>
        int MaxConcurrentJobs { get; set; }

        /// <inheritdoc cref="RoboQueue.Name"/>
        string Name { get; }

        /// <inheritdoc cref="RoboQueue.ProgressEstimator"/>
        IProgressEstimator ProgressEstimator { get; }

        /// <inheritdoc cref="RoboQueue.RunResults"/>
        IRoboQueueResults RunResults { get; }

        /// <inheritdoc cref="RoboQueue.WasCancelled"/>
        bool WasCancelled { get; }

        #endregion

        #region < Events >


        /// <inheritdoc cref="RoboQueue.OnCommandCompleted"/>
        event RoboCommand.CommandCompletedHandler OnCommandCompleted;

        /// <inheritdoc cref="RoboQueue.OnCommandError"/>
        event RoboCommand.CommandErrorHandler OnCommandError;

        /// <inheritdoc cref="RoboQueue.OnCommandStarted"/>
        event RoboQueue.CommandStartedHandler OnCommandStarted;

        /// <inheritdoc cref="RoboQueue.OnCopyProgressChanged"/>
        event RoboCommand.CopyProgressHandler OnCopyProgressChanged;

        /// <inheritdoc cref="RoboQueue.OnError"/>
        event RoboCommand.ErrorHandler OnError;

        /// <inheritdoc cref="RoboQueue.OnFileProcessed"/>
        event RoboCommand.FileProcessedHandler OnFileProcessed;

        /// <inheritdoc cref="RoboQueue.OnProgressEstimatorCreated"/>
        event RoboQueue.ProgressUpdaterCreatedHandler OnProgressEstimatorCreated;

        /// <inheritdoc cref="RoboQueue.RunCompleted"/>
        event RoboQueue.RunCompletedHandler RunCompleted;

        /// <inheritdoc cref="RoboQueue.RunResultsUpdated"/>
        event RoboCopyResultsList.ResultsListUpdated RunResultsUpdated;

        /// <inheritdoc cref="RoboQueue.TaskFaulted"/>
        event UnhandledExceptionEventHandler TaskFaulted;

        #endregion

        #region < Methods >


        /// <inheritdoc cref="RoboQueue.PauseAll"/>
        void PauseAll();

        /// <inheritdoc cref="RoboQueue.ResumeAll"/>
        void ResumeAll();

        /// <inheritdoc cref="RoboQueue.StartAll"/>
        Task<IRoboQueueResults> StartAll(string domain = "", string username = "", string password = "");

        /// <inheritdoc cref="RoboQueue.StartAll_ListOnly"/>
        Task<IRoboQueueResults> StartAll_ListOnly(string domain = "", string username = "", string password = "");

        /// <inheritdoc cref="RoboQueue.StopAll"/>
        void StopAll();

        #endregion
    }
}
