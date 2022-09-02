using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Win32;
using RoboSharp.Interfaces;
using System.Diagnostics;

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
        private int[] AllowedJobCounts = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        #endregion

        #region < Init >

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            VersionManager.VersionCheck = VersionManager.VersionCheckType.UseWMI;
            var v = VersionManager.Version;

            //Button Setup
            btnAddToQueue.IsEnabled = true;
            btnStartJobQueue.IsEnabled = false;
            btnPauseQueue.IsEnabled = false;

            //Setup SingleJob Tab
            SingleJobExpander_JobHistory.BindToList(SingleJobResults);
            SingleJobErrorGrid.ItemsSource = SingleJobErrors;

            //RoboQueue Setup
            ListBox_RoboQueueJobs_MultiJobPage.ItemsSource = RoboQueue;
            ListBox_RoboQueueJobs_OptionsPage.ItemsSource = RoboQueue;
            MultiJobErrorGrid.ItemsSource = MultiJobErrors;

            MultiJob_ListOnlyResults.UpdateDescriptionLblText("List of results from this List-Only Operation.\nThis list is reset every time the queue is restarted.");
            MultiJob_RunResults.UpdateDescriptionLblText("List of results from this Copy/Move Operation.\nThis list is reset every time the queue is restarted.");

            cmbConcurrentJobs_OptionsPage.ItemsSource = AllowedJobCounts;
            cmbConcurrentJobs_MultiJobPage.ItemsSource = AllowedJobCounts;
            cmbConcurrentJobs_MultiJobPage.SelectedItem = RoboQueue.MaxConcurrentJobs;

            RoboQueue.RunResultsUpdated += RoboQueue_RunOperationResultsUpdated;
            RoboQueue.ListResultsUpdated += RoboQueue_ListOnlyResultsUpdated;

            RoboQueue.OnCommandError += RoboQueue_OnCommandError;
            RoboQueue.OnError += RoboQueue_OnError; ;
            RoboQueue.OnCommandCompleted += RoboQueue_OnCommandCompleted; ;
            RoboQueue.OnProgressEstimatorCreated += RoboQueue_OnProgressEstimatorCreated;
            RoboQueue.OnCommandStarted += RoboQueue_OnCommandStarted;
            RoboQueue.CollectionChanged += RoboQueue_CollectionChanged;
            //RoboQueue.OnCopyProgressChanged += copy_OnCopyProgressChanged; // Not In Use for this project -> See MultiJob_CommandProgressIndicator
            //RoboQueue.OnFileProcessed += RoboQueue_OnFileProcessed; // Not In Use for this project -> See MultiJob_CommandProgressIndicator
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

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
            try
            {
                // copy options
                copy.Name = JobName.Text;
                copy.CopyOptions.Source = Source.Text;
                copy.CopyOptions.Destination = Destination.Text;
                
                CopyOptionsExpander.ApplyToIRoboCommand(copy);
                
                // select options
                SelectionOptionsExpander.ApplyToIRoboCommand(copy);

                // retry options
                RetryOptionsExpander.ApplyToIRoboCommand(copy);

                // logging options
                LoggingOptionsExpander.ApplyToIRoboCommand(copy);

                copy.StopIfDisposing = true;
            }catch
            {
                copy = null;
            }
            return copy;
        }

        private void LoadCommand(IRoboCommand copy)
        {
            if (copy == null) return;
            // copy options
            JobName.Text = copy.Name;
            Source.Text = copy.CopyOptions.Source;
            Destination.Text = copy.CopyOptions.Destination;

            // 
            CopyOptionsExpander.LoadFromIRoboCommand(copy);
            
            // select options
            SelectionOptionsExpander.LoadFromIRoboCommand(copy);


            // retry options
            RetryOptionsExpander.LoadFromIRoboCommand(copy);
            
            // logging options
            LoggingOptionsExpander.LoadFromIRoboCommand(copy);
        }

        [DebuggerHidden()]
        void DebugMessage(object sender, Debugger.DebugMessageArgs e)
        {
            Console.WriteLine(e.Message);
        }

        #endregion

        #region < Single Job Methods >

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            copy = GetCommand(true);
            if (copy == null) return;
            copy.Start();
            OptionsGrid.IsEnabled = false;
            SingleJobExpander_Progress.IsExpanded = true;
            SingleJobExpander_JobHistory.IsExpanded = false;
            SingleJobExpander_Errors.IsExpanded = false;
            SingleJobTab.IsSelected = true;
            SingleJobExpander_Progress.ProgressGrid.IsEnabled = true;
        }

        private void BtnLoadJob_Click(object sender, RoutedEventArgs e)
        {
            var FP = new OpenFileDialog();
            FP.Filter = RoboSharp.JobFile.JOBFILE_DialogFilter;
            FP.Multiselect = false;
            FP.Title = "Select RoboCopy Job File.";
            try
            {
                JobFile JF = null;
                bool? FilePicked = FP.ShowDialog(this);
                if (FilePicked ?? false)
                    JF = JobFile.ParseJobFile(FP.FileName);
                if (JF != null)
                {
                    var oldCmd = GetCommand(false);
                    oldCmd.MergeJobFile(JF);//Perform Merge Test
                    LoadCommand((IRoboCommand)JF);
                }
                else
                {
                    MessageBox.Show("Job File Not Loaded / Not Selected");
                }
            }
            catch
            {

            }
        }

        private async void BtnSaveJob_Click(object sender, RoutedEventArgs e)
        {
            var FP = new SaveFileDialog();
            FP.Filter = RoboSharp.JobFile.JOBFILE_DialogFilter;
            FP.Title = "Save RoboCopy Job File.";
            try
            {
                bool? FilePicked = FP.ShowDialog(this);
                if (FilePicked ?? false)
                {
                    await GetCommand(false).SaveAsJobFile(FP.FileName, true, true);
                }
                else
                {
                    MessageBox.Show("Job File Not Saved");
                }
            }
            catch
            {

            }

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

        #region < Button Events >

        private void btn_AddToQueue(object sender, RoutedEventArgs e)
        {
            if (!RoboQueue.IsRunning)
            {
                var copy = GetCommand(false);
                if (copy == null) return;
                RoboQueue.AddCommand(copy);
                btnStartJobQueue.IsEnabled = true;
                btnStartJobQueue_Copy.IsEnabled = true;
            }
        }

        private async void btn_StartQueue(object sender, RoutedEventArgs e)
        {
            if (!RoboQueue.IsRunning)
            {
                if (RoboQueue.ListCount > 0)
                {
                    MultiJobProgressBar_JobsComplete.Value = 0;
                    MultiJobProgressBar_JobsComplete.Minimum = 0;
                    MultiJobProgressBar_JobsComplete.Maximum = RoboQueue.ListCount;

                    btnAddToQueue.IsEnabled = false;
                    btnRemoveSelectedJob.IsEnabled = false;
                    btnRemoveSelectedJob_Copy.IsEnabled = false;
                    btnReplaceSelected.IsEnabled = false;
                    btnUpdateSelectedJob_Copy.IsEnabled = false;

                    btnStartJobQueue.IsEnabled = true;
                    btnStartJobQueue.Content = "Stop Queued Jobs";

                    btnStartJobQueue_Copy.IsEnabled = true;
                    btnStartJobQueue_Copy.Content = "Stop Queued Jobs";

                    btnPauseQueue.IsEnabled = true;

                    RoboQueueProgressStackPanel.Children.Clear();

                    MultiJobProgressTab.IsSelected = true;
                    MultiJobExpander_Progress.IsExpanded = true;

                    if (chkListOnly.IsChecked == true)
                    { 
                        await RoboQueue.StartAll_ListOnly();
                        MultiJob_ListOnlyResults.BindToList(RoboQueue.ListResults);
                    }
                    else
                    { 
                        await RoboQueue.StartAll();
                        MultiJob_RunResults.BindToList(RoboQueue.RunResults);
                    }

                    btnPauseQueue.IsEnabled = false;
                    btnAddToQueue.Content = "Add to Queue";
                    btnStartJobQueue.Content = "Start Queued Jobs";
                    btnStartJobQueue_Copy.Content = "Start Queued Jobs";

                    btnAddToQueue.IsEnabled = true;
                    btnRemoveSelectedJob.IsEnabled = true;
                    btnRemoveSelectedJob_Copy.IsEnabled = true;
                    btnReplaceSelected.IsEnabled = true;
                    btnUpdateSelectedJob_Copy.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Job Queue is Empty");
                }
            }


            else
                RoboQueue.StopAll();
        }

        private void btn_PauseResumeQueue(object sender, RoutedEventArgs e)
        {
            if (!RoboQueue.IsPaused)
            {
                RoboQueue.PauseAll();
                btnPauseQueue.Content = "Resume Job Queue";
            }
            else
            {
                RoboQueue.ResumeAll();
                btnPauseQueue.Content = "Pause Job Queue";
            }
        }

        /// <summary>
        /// Remove RoboCommand from the list
        /// </summary>
        private void btn_RemoveSelected(object sender, RoutedEventArgs e)
        {
            var cmd = (RoboCommand)ListBox_RoboQueueJobs_MultiJobPage.SelectedItem;
            RoboQueue.RemoveCommand(cmd);
        }

        /// <summary>
        /// Load into Options Page
        /// </summary>
        private void btn_LoadSelected(object sender, RoutedEventArgs e)
        {
            LoadCommand((RoboCommand)ListBox_RoboQueueJobs_MultiJobPage.SelectedItem);
        }

        /// <summary>
        /// Replace item in list with new RoboCommand
        /// </summary>
        private void btn_ReplaceSelected(object sender, RoutedEventArgs e)
        {
            int i = ListBox_RoboQueueJobs_MultiJobPage.SelectedIndex;
            if (i >= 0)
                RoboQueue.ReplaceCommand(GetCommand(false), i);
            else
                RoboQueue.AddCommand(GetCommand(false));
        }

        #endregion

        #region < Progress Estimator >

        private void RoboQueue_OnProgressEstimatorCreated(RoboQueue sender, EventArgObjects.ProgressEstimatorCreatedEventArgs e)
        {
            e.ResultsEstimate.ValuesUpdated += IProgressEstimatorValuesUpdated;
        }

        private void IProgressEstimatorValuesUpdated(Interfaces.IProgressEstimator sender, EventArgObjects.IProgressEstimatorUpdateEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateLabel(ProgressEstimator_Directories, e.DirectoriesStatistic);
                UpdateLabel(ProgressEstimator_Files, e.FilesStatistic);
                UpdateLabel(ProgressEstimator_Bytes, e.BytesStatistic);
            });
        }

        private void UpdateLabel(TextBlock lbl, RoboSharp.Interfaces.IStatistic stat)
        {
            Dispatcher.Invoke(() =>
            {
                lbl.Text = stat?.ToString(true, true, "\n", true) ?? "";
            });
        }

        #endregion

        #region < RoboQueue & Command Events >

        private void UpdateCommandsRunningBox()
        {
            Dispatcher.Invoke(() => MultiJob_JobRunningCount.Text = $"{RoboQueue.JobsCurrentlyRunning}");
        }

        /// <summary>
        /// Add MultiJob_CommandProgressIndicator to window
        /// </summary>
        private void RoboQueue_OnCommandStarted(RoboQueue sender, EventArgObjects.RoboQueueCommandStartedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RoboQueueProgressStackPanel.Children.Add(new MultiJob_CommandProgressIndicator(e.Command));
            });
            UpdateCommandsRunningBox();
        }

        /// <summary>
        /// Disable associated MultiJob_CommandProgressIndicator to window
        /// </summary>
        private void RoboQueue_OnCommandCompleted(IRoboCommand sender, RoboCommandCompletedEventArgs e)
        {
            UpdateCommandsRunningBox();
            Dispatcher.Invoke(() =>
            {
                MultiJob_JobsCompleteXofY.Text = $"{RoboQueue.JobsComplete} of {RoboQueue.ListCount}";
                MultiJobProgressBar_JobsComplete.Value = RoboQueue.JobsComplete;
            });
        }

        /// <summary>
        /// Removes unneeded FirstMultiProgressExpander controls
        /// </summary>
        private void RoboQueue_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    // Remove all FirstMultiProgressExpander
                    RoboQueueProgressStackPanel.Children.Clear();
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    //Remove associated MultiJob_CommandProgressIndicator
                    var PBars = RoboQueueProgressStackPanel.Children.OfType<MultiJob_CommandProgressIndicator>();
                    foreach (var element in PBars)
                    {
                        if (e.OldItems.Contains(element.Command))
                            RoboQueueProgressStackPanel.Children.Remove(element);
                        break;
                    }
                }
            });
        }

        /// <summary>
        /// Log the Error to the Errors expander
        /// </summary>
        private void RoboQueue_OnError(IRoboCommand sender, ErrorEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                MultiJobErrors.Insert(0, new FileError { Error = e.Error });
                MultiJobExpander_Errors.Header = string.Format("Errors ({0})", MultiJobErrors.Count);
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Occurs while a command is starting prior to Robocopy starting (for example, due to missing source location), but won't break the entire RoboQueue. 
        /// That single job will just not start, all others will.
        /// </remarks>
        private void RoboQueue_OnCommandError(IRoboCommand sender, CommandErrorEventArgs e)
        {
            btn_StartQueue(null, null); // Stop Everything

        }

        /// <summary>
        /// This listener is used to rebind the ListOnly results to the display listbox. 
        /// It had to be done like this due to INotifyCollectionChanged not updating the listbox due to threading issues
        /// </summary>
        private void RoboQueue_ListOnlyResultsUpdated(object sender, EventArgObjects.ResultListUpdatedEventArgs e)
        {
            MultiJob_ListOnlyResults.BindToList(e.ResultsList);
        }

        /// <summary>
        /// This listener is used to rebind the RunOperation results to the display listbox. 
        /// It had to be done like this due to INotifyCollectionChanged not updating the listbox due to threading issues
        /// </summary>
        private void RoboQueue_RunOperationResultsUpdated(object sender, EventArgObjects.ResultListUpdatedEventArgs e)
        {
            MultiJob_RunResults.BindToList(e.ResultsList);
        }

        #endregion

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

        private void chkListOnly_Checked(object sender, RoutedEventArgs e)
        {
            chkListOnly.IsChecked = true;
            chkListOnly_Copy.IsChecked = true;
        }

        private void chkListOnly_UnChecked(object sender, RoutedEventArgs e)
        {
            chkListOnly.IsChecked = false;
            chkListOnly_Copy.IsChecked = false;
        }

        private void MultiJob_ConcurrentAmountChanged(object sender, RoutedEventArgs e)
        {
            RoboQueue.MaxConcurrentJobs = (int)((ComboBox)sender).SelectedItem;
            cmbConcurrentJobs_OptionsPage.SelectedItem = RoboQueue.MaxConcurrentJobs;
            cmbConcurrentJobs_MultiJobPage.SelectedItem = RoboQueue.MaxConcurrentJobs;
        }

        private void RoboQueueListBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            var LB = (ListBox)sender;
            ListBox_RoboQueueJobs_OptionsPage.SelectedIndex = LB.SelectedIndex;
            ListBox_RoboQueueJobs_MultiJobPage.SelectedIndex = LB.SelectedIndex;
        }

        private void IsNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helpers.IsInt(e.Text);
        }


        #endregion
    }

    public class FileError
    {
        public string Error { get; set; }
    }
}
