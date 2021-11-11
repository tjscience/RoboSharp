using System.Threading.Tasks;

namespace RoboSharp.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRoboCommand
    {
        #region Properties
        
        /// <inheritdoc cref="RoboCommand.IsPaused"/>
        bool IsPaused { get; }
        
        /// <inheritdoc cref="RoboCommand.IsPaused"/>
        string CommandOptions { get; }
        
        /// <inheritdoc cref="RoboCommand.CopyOptions"/>
        CopyOptions CopyOptions { get; set; }
        
        /// <inheritdoc cref="RoboCommand.SelectionOptions"/>
        SelectionOptions SelectionOptions { get; set; }
        
        /// <inheritdoc cref="RoboCommand.RetryOptions"/>
        RetryOptions RetryOptions { get; set; }
        
        /// <inheritdoc cref="RoboCommand.LoggingOptions"/>
        LoggingOptions LoggingOptions { get; set; }
        
        /// <inheritdoc cref="RoboCommand.Configuration"/>
        RoboSharpConfiguration Configuration { get; }

        #endregion Properties

        #region Events

        /// <inheritdoc cref="RoboCommand.OnFileProcessed"/>
        event RoboCommand.FileProcessedHandler OnFileProcessed;
        
        /// <inheritdoc cref="RoboCommand.OnCommandError"/>
        event RoboCommand.CommandErrorHandler OnCommandError;
        
        /// <inheritdoc cref="RoboCommand.OnError"/>
        event RoboCommand.ErrorHandler OnError;
        
        /// <inheritdoc cref="RoboCommand.OnCommandCompleted"/>
        event RoboCommand.CommandCompletedHandler OnCommandCompleted;
        
        /// <inheritdoc cref="RoboCommand.OnCopyProgressChanged"/>
        event RoboCommand.CopyProgressHandler OnCopyProgressChanged;

        #endregion Events

        #region Methods

        /// <inheritdoc cref="RoboCommand.Pause"/>
        void Pause();
        
        /// <inheritdoc cref="RoboCommand.Resume"/>
        void Resume();
        
        /// <inheritdoc cref="RoboCommand.Start(string, string, string)"/>
        Task Start(string domain = "", string username = "", string password = "");
        
        /// <inheritdoc cref="RoboCommand.Stop"/>
        void Stop();
        
        /// <inheritdoc cref="RoboCommand.Dispose()"/>
        void Dispose();
        
        #endregion Methods
    }
}