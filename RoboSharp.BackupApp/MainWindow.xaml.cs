using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace RoboSharp.BackupApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RoboCommand copy;

        public ObservableCollection<FileError> Errors = new ObservableCollection<FileError>();

        private Results.RoboCopyResultsList JobResults = new Results.RoboCopyResultsList();

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            JobResults.CollectionChanged += UpdateOverallLabel;
            ListBox_JobResults.ItemsSource = JobResults;
            ErrorGrid.ItemsSource = Errors;
            VersionManager.VersionCheck = VersionManager.VersionCheckType.UseWMI;
            var v = VersionManager.Version;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (copy != null)
            {
                copy.Stop();
                copy.Dispose();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OptionsGrid.IsEnabled = false;
            ProgressTab.IsSelected = true;
            ProgressGrid.IsEnabled = true;
            Backup();
        }

        public void Backup()
        {
            Debugger.Instance.DebugMessageEvent += DebugMessage;

            copy = new RoboCommand();
            copy.OnFileProcessed += copy_OnFileProcessed;
            copy.OnCommandError += copy_OnCommandError;
            copy.OnError += copy_OnError;
            copy.OnCopyProgressChanged += copy_OnCopyProgressChanged;
            copy.OnCommandCompleted += copy_OnCommandCompleted;
            // copy options
            copy.CopyOptions.Source = Source.Text;
            copy.CopyOptions.Destination = Destination.Text;

            // split user input by whitespace, mantaining those enclosed by quotes
            var fileFilterItems = Regex.Matches(FileFilter.Text, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value);

            copy.CopyOptions.FileFilter = fileFilterItems;
            copy.CopyOptions.CopySubdirectories = CopySubDirectories.IsChecked ?? false;
            copy.CopyOptions.CopySubdirectoriesIncludingEmpty = CopySubdirectoriesIncludingEmpty.IsChecked ?? false;
            if (!string.IsNullOrWhiteSpace(Depth.Text))
                copy.CopyOptions.Depth = Convert.ToInt32(Depth.Text);
            copy.CopyOptions.EnableRestartMode = EnableRestartMode.IsChecked ?? false;
            copy.CopyOptions.EnableBackupMode = EnableBackupMode.IsChecked ?? false;
            copy.CopyOptions.EnableRestartModeWithBackupFallback = EnableRestartModeWithBackupFallback.IsChecked ?? false;
            copy.CopyOptions.UseUnbufferedIo = UseUnbufferedIo.IsChecked ?? false;
            copy.CopyOptions.EnableEfsRawMode = EnableEfsRawMode.IsChecked ?? false;
            copy.CopyOptions.CopyFlags = CopyFlags.Text;
            copy.CopyOptions.CopyFilesWithSecurity = CopyFilesWithSecurity.IsChecked ?? false;
            copy.CopyOptions.CopyAll = CopyAll.IsChecked ?? false;
            copy.CopyOptions.RemoveFileInformation = RemoveFileInformation.IsChecked ?? false;
            copy.CopyOptions.FixFileSecurityOnAllFiles = FixFileSecurityOnAllFiles.IsChecked ?? false;
            copy.CopyOptions.FixFileTimesOnAllFiles = FixFileTimesOnAllFiles.IsChecked ?? false;
            copy.CopyOptions.Purge = Purge.IsChecked ?? false;
            copy.CopyOptions.Mirror = Mirror.IsChecked ?? false;
            copy.CopyOptions.MoveFiles = MoveFiles.IsChecked ?? false;
            copy.CopyOptions.MoveFilesAndDirectories = MoveFilesAndDirectories.IsChecked ?? false;
            copy.CopyOptions.AddAttributes = AddAttributes.Text;
            copy.CopyOptions.RemoveAttributes = RemoveAttributes.Text;
            copy.CopyOptions.CreateDirectoryAndFileTree = CreateDirectoryAndFileTree.IsChecked ?? false;
            copy.CopyOptions.FatFiles = FatFiles.IsChecked ?? false;
            copy.CopyOptions.TurnLongPathSupportOff = TurnLongPathSupportOff.IsChecked ?? false;
            if (!string.IsNullOrWhiteSpace(MonitorSourceChangesLimit.Text))
                copy.CopyOptions.MonitorSourceChangesLimit = Convert.ToInt32(MonitorSourceChangesLimit.Text);
            if (!string.IsNullOrWhiteSpace(MonitorSourceTimeLimit.Text))
                copy.CopyOptions.MonitorSourceTimeLimit = Convert.ToInt32(MonitorSourceTimeLimit.Text);

            // select options
            copy.SelectionOptions.OnlyCopyArchiveFiles = OnlyCopyArchiveFiles.IsChecked ?? false;
            copy.SelectionOptions.OnlyCopyArchiveFilesAndResetArchiveFlag = OnlyCopyArchiveFilesAndResetArchiveFlag.IsChecked ?? false;
            copy.SelectionOptions.IncludeAttributes = IncludeAttributes.Text;
            copy.SelectionOptions.ExcludeAttributes = ExcludeAttributes.Text;
            copy.SelectionOptions.ExcludeFiles = ExcludeFiles.Text;
            copy.SelectionOptions.ExcludeDirectories = ExcludeDirectories.Text;
            copy.SelectionOptions.ExcludeOlder = ExcludeOlder.IsChecked ?? false;
            copy.SelectionOptions.ExcludeJunctionPoints = ExcludeJunctionPoints.IsChecked ?? false;

            // retry options
            if (!string.IsNullOrWhiteSpace(RetryCount.Text))
                copy.RetryOptions.RetryCount = Convert.ToInt32(RetryCount.Text);
            if (!string.IsNullOrWhiteSpace(RetryWaitTime.Text))
                copy.RetryOptions.RetryWaitTime = Convert.ToInt32(RetryWaitTime.Text);

            // logging options
            copy.LoggingOptions.VerboseOutput = VerboseOutput.IsChecked ?? false;
            copy.LoggingOptions.NoFileSizes = NoFileSizes.IsChecked ?? false;
            copy.LoggingOptions.NoProgress = NoProgress.IsChecked ?? false;

            copy.Start();
        }

        void DebugMessage(object sender, Debugger.DebugMessageArgs e)
        {
            Console.WriteLine(e.Message);
        }

        void copy_OnCommandError(object sender, CommandErrorEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(e.Error);
                OptionsGrid.IsEnabled = true;
                ProgressGrid.IsEnabled = false;
            }));
        }

        void copy_OnCopyProgressChanged(object sender, CopyProgressEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                FileProgress.Value = e.CurrentFileProgress;
                FileProgressPercent.Text = string.Format("{0}%", e.CurrentFileProgress);
            }));
        }

        void copy_OnError(object sender, ErrorEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                Errors.Insert(0, new FileError { Error = e.Error });
                ErrorsTab.Header = string.Format("Errors ({0})", Errors.Count);
            }));
        }

        void copy_OnFileProcessed(object sender, FileProcessedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                CurrentOperation.Text = e.ProcessedFile.FileClass;
                CurrentFile.Text = e.ProcessedFile.Name;
                CurrentSize.Text = e.ProcessedFile.Size.ToString();
            }));
        }

        void copy_OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                OptionsGrid.IsEnabled = true;
                ProgressGrid.IsEnabled = false;
                
                var results = e.Results;
                Console.WriteLine("Files copied: " + results.FilesStatistic.Copied);
                Console.WriteLine("Directories copied: " + results.DirectoriesStatistic.Copied);
                JobResults.Add(e.Results);
            }));
        }

        private void PauseResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!copy.IsPaused)
            {
                copy.Pause();
                PauseResumeButton.Content = "Resume";
            }
            else
            {
                copy.Resume();
                PauseResumeButton.Content = "Pause";
            }
        }

        private void SourceBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            Source.Text = dialog.SelectedPath;
        }

        private void DestinationBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            Destination.Text = dialog.SelectedPath;
        }

        private void IsNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsInt(e.Text);
        }

        private void IsAttribute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[a-zA-Z]+$"))
                e.Handled = true;
            if ("bcefghijklmnpqrvwxyzBCEFGHIJKLMNPQRVWXYZ".Contains(e.Text))
                e.Handled = true;
            if (((TextBox)sender).Text.Contains(e.Text))
                e.Handled = true;
        }

        public static bool IsInt(string text)
        {
            Regex regex = new Regex("[^0-9]+$");
            return !regex.IsMatch(text);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (copy != null)
            {
                copy.Stop();
                copy.Dispose();
            }
        }

        private void UpdateSelectedItemsLabel(object sender, SelectionChangedEventArgs e)
        {
            Results.RoboCopyResults result = (Results.RoboCopyResults)this.ListBox_JobResults.SelectedItem;
            string NL = Environment.NewLine;
            lbl_SelectedItemTotals.Content = $"Selected Job:" +
                $"{NL}Source: {result.Source}" +
                $"{NL}Destination: {result.Destination}" +
                $"{NL}Total Directories: {result.DirectoriesStatistic.Total}" +
                $"{NL}Total Files: {result.FilesStatistic.Total}" +
                $"{NL}Total Size (bytes): {result.BytesStatistic.Total}" +
                $"{NL}{result.Status.ToString()}";
        }

        /// <summary>
        /// Runs every time the JobResults list is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateOverallLabel(object sender, EventArgs e)
        {
            string NL = Environment.NewLine;
            lbl_OverallTotals.Content = $"Job History:" +
                $"{NL}Total Directories: {JobResults.DirectoriesStatistic.Total}" +
                $"{NL}Total Files: {JobResults.FilesStatistic.Total}" +
                $"{NL}Total Size (bytes): {JobResults.BytesStatistic.Total}" +
                $"{NL}Any Jobs Cancelled: {(JobResults.Status.WasCancelled ? "YES" : "NO")}" +
                $"{NL}{JobResults.Status.ToString()}";
        }
    }

    public class FileError
    {
        public string Error { get; set; }
    }
}
