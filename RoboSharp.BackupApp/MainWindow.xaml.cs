﻿using System;
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
            //RoboQueue.OnFileProcessed += copy_OnFileProcessed;
            //RoboQueue.OnCommandError += copy_OnCommandError;
            //RoboQueue.OnError += copy_OnError;
            //RoboQueue.OnCopyProgressChanged += copy_OnCopyProgressChanged;
            //RoboQueue.OnCommandCompleted += copy_OnCommandCompleted;
            //RoboQueue.OnProgressEstimatorCreated += Copy_OnProgressEstimatorCreated;
            
            //Setup SingleJob Tab
            SingleJobExpander_JobHistory.BindToList(SingleJobResults);
            SingleJobErrorGrid.ItemsSource = SingleJobErrors;
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
                SingleJobExpander_Progress.BindToCommand(copy);
                copy.OnCommandError += copy_OnCommandError;
                copy.OnError += copy_OnError;
                copy.OnCommandCompleted += copy_OnCommandCompleted;
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

        #endregion

        #region < Single Job Methods >

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            copy = GetCommand(true);
            copy.Start();
            OptionsGrid.IsEnabled = false;
            SingleJobExpander_Progress.IsExpanded = true;
            SingleJobExpander_JobHistory.IsExpanded = false;
            SingleJobExpander_Errors.IsExpanded = false;
            SingleJobTab.IsSelected = true;
            SingleJobExpander_Progress.ProgressGrid.IsEnabled = true;
        }

        void copy_OnCommandError(object sender, CommandErrorEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MessageBox.Show(e.Error);
                OptionsGrid.IsEnabled = true;
                SingleJobExpander_Progress.ProgressGrid.IsEnabled = false;
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

        void copy_OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                OptionsGrid.IsEnabled = true;
                SingleJobExpander_Progress.ProgressGrid.IsEnabled = false;

                var results = e.Results;
                Console.WriteLine("Files copied: " + results.FilesStatistic.Copied);
                Console.WriteLine("Directories copied: " + results.DirectoriesStatistic.Copied);
                SingleJobResults.Add(e.Results);
            }));
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
