using System.Threading.Tasks;

namespace RoboSharp.Interfaces
{
    public interface IRoboCommand
    {
        bool IsPaused { get; }
        string CommandOptions { get; }
        CopyOptions CopyOptions { get; set; }
        SelectionOptions SelectionOptions { get; set; }
        RetryOptions RetryOptions { get; set; }
        LoggingOptions LoggingOptions { get; set; }
        RoboSharpConfiguration Configuration { get; }
        event RoboCommand.FileProcessedHandler OnFileProcessed;
        event RoboCommand.CommandErrorHandler OnCommandError;
        event RoboCommand.ErrorHandler OnError;
        event RoboCommand.CommandCompletedHandler OnCommandCompleted;
        event RoboCommand.CopyProgressHandler OnCopyProgressChanged;
        event RoboCommand.GeneralOutputHandler OnGeneralOutputChanged;
        void Pause();
        void Resume();
        Task Start(string domain = "", string username = "", string password = "");
        void Stop();
        void Dispose();
    }
}