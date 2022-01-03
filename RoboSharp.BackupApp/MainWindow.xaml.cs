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
        #region < SingleJob Fields >

        RoboCommand copy;
        public ObservableCollection<FileError> SingleJobErrors = new ObservableCollection<FileError>();
        private Results.RoboCopyResultsList SingleJobResults = new Results.RoboCopyResultsList();

        #endregion

        #region < RoboQueue Fields >

        /// <summary> List of RoboCommand objects to start at same time </summary>
        private RoboSharp.RoboQueue RoboQueue = new RoboSharp.RoboQueue();
        public ObservableCollection<FileError> MultiJobErrors = new ObservableCollection<FileError>();

        #endregion

        #region < Init >

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;

            VersionManager.VersionCheck = VersionManager.VersionCheckType.UseWMI;
            var v = VersionManager.Version;
            //Button Setup
            btnAddToQueue.IsEnabled = true;
            btnStartJobQueue.IsEnabled = false;
            btnPauseQueue.IsEnabled = false;
            //Event subscribe
            RoboQueue.OnFileProcessed += copy_OnFileProcessed;
            RoboQueue.OnCommandError += copy_OnCommandError;
            RoboQueue.OnError += copy_OnError;
            RoboQueue.OnCopyProgressChanged += copy_OnCopyProgressChanged;
            RoboQueue.OnCommandCompleted += copy_OnCommandCompleted;
            RoboQueue.OnProgressEstimatorCreated += Copy_OnProgressEstimatorCreated;
            //Setup SingleJob Tab
            ListBox_JobResults.ItemsSource = SingleJobResults;
            SingleJobErrorGrid.ItemsSource = SingleJobErrors;
            SingleJobResults.CollectionChanged += ( _ , __ ) => UpdateOverallLabel(lbl_OverallTotals);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (copy != null)
            {
                copy.Stop();
                copy.Dispose();
            }
        }

        #endregion

        #region < Shared Methods >

        private RoboCommand GetCommand(bool BindEvents)
        {
            Debugger.Instance.DebugMessageEvent += DebugMessage;
            RoboCommand copy = new RoboCommand();
            if (BindEvents)
            {
                copy.OnFileProcessed += copy_OnFileProcessed;
                copy.OnCommandError += copy_OnCommandError;
                copy.OnError += copy_OnError;
                copy.OnCopyProgressChanged += copy_OnCopyProgressChanged;
                copy.OnCommandCompleted += copy_OnCommandCompleted;
                copy.OnProgressEstimatorCreated += Copy_OnProgressEstimatorCreated;//Progress Estimator
            }
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
            return copy;
        }

        void DebugMessage(object sender, Debugger.DebugMessageArgs e)
        {
            Console.WriteLine(e.Message);
        }

        public static bool IsInt(string text)
        {
            Regex regex = new Regex("[^0-9]+$", RegexOptions.Compiled);
            return !regex.IsMatch(text);
        }

        private void UpdateSelectedResultsLabel(object listboxSender, SelectionChangedEventArgs e, Label LabelToUpdate)
        {
            Results.RoboCopyResults result = (Results.RoboCopyResults)((ListBox)listboxSender).SelectedItem;
            string NL = Environment.NewLine;
            LabelToUpdate.Content = $"Selected Job:" +
                $"{NL}Source: {result?.Source ?? ""}" +
                $"{NL}Destination: {result?.Destination ?? ""}" +
                $"{NL}Total Directories: {result?.DirectoriesStatistic?.Total ?? 0}" +
                $"{NL}Total Files: {result?.FilesStatistic?.Total ?? 0}" +
                $"{NL}Total Size (bytes): {result?.BytesStatistic?.Total ?? 0}" +
                $"{NL}Speed (Bytes/Second): {result?.SpeedStatistic?.BytesPerSec ?? 0}" +
                $"{NL}Speed (MB/Min): {result?.SpeedStatistic?.MegaBytesPerMin ?? 0}" +
                $"{NL}Log Lines Count: {result?.LogLines?.Length ?? 0}" +
                $"{NL}{result?.Status.ToString() ?? ""}";
        }

        /// <summary>
        /// Runs every time the SingleJobResults list is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateOverallLabel(Label LabelToUpdate)
        {
            string NL = Environment.NewLine;
            LabelToUpdate.Content = $"Job History:" +
                $"{NL}Total Directories: {SingleJobResults.DirectoriesStatistic.Total}" +
                $"{NL}Total Files: {SingleJobResults.FilesStatistic.Total}" +
                $"{NL}Total Size (bytes): {SingleJobResults.BytesStatistic.Total}" +
                $"{NL}Speed (Bytes/Second): {SingleJobResults.SpeedStatistic.BytesPerSec}" +
                $"{NL}Speed (MB/Min): {SingleJobResults.SpeedStatistic.MegaBytesPerMin}" +
                $"{NL}Any Jobs Cancelled: {(SingleJobResults.Status.WasCancelled ? "YES" : "NO")}" +
                $"{NL}{SingleJobResults.Status}";
        }

        #endregion

        #region < Single Job Methods >

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            OptionsGrid.IsEnabled = false;
            SingleJobExpander_Progress.IsExpanded = true;
            SingleJobExpander_JobHistory.IsExpanded = false;
            SingleJobExpander_Errors.IsExpanded = false;
            SingleJobTab.IsSelected = true;
            ProgressGrid.IsEnabled = true;
            Backup();
        }

        public void Backup()
        {
            copy = GetCommand(true);
            copy.Start();
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (copy != null)
            {
                copy.Stop();
                copy.Dispose();
            }

        }

        /// <summary> Bind the ProgressEstimator to the text controls on the PROGRESS tab </summary>
        private void Copy_OnProgressEstimatorCreated(object sender, Results.ProgressEstimatorCreatedEventArgs e)
        {
            e.ResultsEstimate.ByteStats.PropertyChanged += ByteStats_PropertyChanged;
            e.ResultsEstimate.DirStats.PropertyChanged += DirStats_PropertyChanged;
            e.ResultsEstimate.FileStats.PropertyChanged += FileStats_PropertyChanged;
        }


        private void FileStats_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SingleProgressEstimator_Files.Dispatcher.Invoke(() => SingleProgressEstimator_Files.Text = ((RoboSharp.Results.Statistic)sender).ToString(true, true, "\n", true));
            //Dispatcher.Invoke(() =>
            //{
            //    string s = SingleProgressEstimator_Files.Text;
            //    SingleProgressEstimator_Files.Text = ((RoboSharp.Results.Statistic)sender).ToString(true, true, "\n", true);
            //    string n = SingleProgressEstimator_Files.Text;
            //});
        }

        private void DirStats_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SingleProgressEstimator_Directories.Dispatcher.Invoke(() => SingleProgressEstimator_Directories.Text = ((RoboSharp.Results.Statistic)sender).ToString(true, true, "\n", true));
        }

        private void ByteStats_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SingleProgressEstimator_Bytes.Dispatcher.Invoke(() => SingleProgressEstimator_Bytes.Text = ((RoboSharp.Results.Statistic)sender).ToString(true, true, "\n", true));
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
                SingleJobFileProgressBar.Value = e.CurrentFileProgress;
                FileProgressPercent.Text = string.Format("{0}%", e.CurrentFileProgress);
            }));
        }

        void copy_OnError(object sender, ErrorEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                SingleJobErrors.Insert(0, new FileError { Error = e.Error });
                SingleJobExpander_Errors.Header = string.Format("Errors ({0})", SingleJobErrors.Count);
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
                SingleJobResults.Add(e.Results);
            }));
        }

        private void UpdateSelectedItemsLabel(object sender, SelectionChangedEventArgs e) => UpdateSelectedResultsLabel((ListBox)sender, e, lbl_SelectedItemTotals);

        private void Remove_Selected_Click(object sender, RoutedEventArgs e)
        {
            Results.RoboCopyResults result = (Results.RoboCopyResults)this.ListBox_JobResults.SelectedItem;

            SingleJobResults.Remove(result);

            {
                if (ListBox_JobResults.Items.Count == 0)
                {
                    lbl_OverallTotals.Content = "";
                    lbl_SelectedItemTotals.Content = "";
                }
            }

        }

        #endregion

        #region < Multi-Job >

        private void btn_AddToQueue(object sender, RoutedEventArgs e)
        {
            if (!RoboQueue.IsRunning)
            {
                RoboQueue.AddCommand(GetCommand(false));
                btnAddToQueue.IsEnabled = true;
            }
            else
                RoboQueue.StopAll();
        }

        private async void btn_StartQueue(object sender, RoutedEventArgs e)
        {
            btnStartJobQueue.IsEnabled = false;
            btnPauseQueue.IsEnabled = true;
            btnAddToQueue.Content = "Stop Queued Jobs";
            await RoboQueue.StartAll();
            SingleJobResults.Clear();
            SingleJobResults.AddRange(RoboQueue.RunOperationResults);
            RoboQueue.ClearCommandList();
            btnPauseQueue.IsEnabled = false;
            btnAddToQueue.Content = "Add to Queue";
            btnStartJobQueue.IsEnabled = true;
        }

        private void btn_PauseResumeQueue(object sender, RoutedEventArgs e)
        {
            if (RoboQueue.IsRunning)
                RoboQueue.PauseAll();
            else
                RoboQueue.ResumeAll();
        }

        private void btn_RemoveSelected(object sender, RoutedEventArgs e)
        {

        }

        private void btn_UpdateSelected(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region < Options Page >

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

        #endregion

        #region < Form Stuff >

        private void IsNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsInt(e.Text);
        }

        private void IsAttribute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[a-zA-Z]+$", RegexOptions.Compiled))
                e.Handled = true;
            if ("bcefghijklmnpqrvwxyzBCEFGHIJKLMNPQRVWXYZ".Contains(e.Text))
                e.Handled = true;
            if (((TextBox)sender).Text.Contains(e.Text))
                e.Handled = true;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        #endregion
    }

    public class FileError
    {
        public string Error { get; set; }
    }
}
