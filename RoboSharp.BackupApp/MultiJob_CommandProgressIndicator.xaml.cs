using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoboSharp.BackupApp
{
    /// <summary>
    /// Interaction logic for MultiJob_CommandProgressIndicator.xaml
    /// </summary>
    public partial class MultiJob_CommandProgressIndicator : Expander
    {
        public MultiJob_CommandProgressIndicator()
        {
            InitializeComponent();
        }

        public MultiJob_CommandProgressIndicator(RoboCommand cmd)
        {
            InitializeComponent();
            BindToCommand(cmd);
        }


        public RoboCommand Command { get; private set; }

        public void BindToCommand(RoboCommand cmd)
        {
            Command = cmd;
            if (cmd.ProgressEstimator != null)
            {
                BindToProgressEstimator(cmd.ProgressEstimator);
            }
            else
            {
                cmd.OnProgressEstimatorCreated += OnProgressEstimatorCreated;
            }
            cmd.OnCopyProgressChanged += OnCopyProgressChanged;
            cmd.OnFileProcessed += OnFileProcessed;
            cmd.OnCommandCompleted += OnCommandCompleted;
        }

        #region < Buttons >

        public void PauseResumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Command != null)
            {
                if (!Command.IsPaused)
                {
                    Command.Pause();
                    PauseResumeButton.Content = "Resume";
                }
                else
                {
                    Command.Resume();
                    PauseResumeButton.Content = "Pause";
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Command != null)
            {
                Command.Stop();
            }
        }

        #endregion

        #region < Progress Estimator >

        /// <summary> Bind the ProgressEstimator to the text controls on the PROGRESS tab </summary>
        private void OnProgressEstimatorCreated(object sender, Results.ProgressEstimatorCreatedEventArgs e) => BindToProgressEstimator(e.ResultsEstimate);

        private void BindToProgressEstimator(RoboSharp.Results.ProgressEstimator e)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressEstimator_Files.Text = "Files";
                ProgressEstimator_Directories.Text = "Directories";
                ProgressEstimator_Bytes.Text = "Bytes";
            });
            e.ByteStats.PropertyChanged += ByteStats_PropertyChanged;
            e.DirStats.PropertyChanged += DirStats_PropertyChanged;
            e.FileStats.PropertyChanged += FileStats_PropertyChanged;
        }

        private void FileStats_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ProgressEstimator_Files.Dispatcher.Invoke(() => ProgressEstimator_Files.Text = ((RoboSharp.Results.Statistic)sender).ToString(true, true, "\n", true));
        }

        private void DirStats_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ProgressEstimator_Directories.Dispatcher.Invoke(() => ProgressEstimator_Directories.Text = ((RoboSharp.Results.Statistic)sender).ToString(true, true, "\n", true));
        }

        private void ByteStats_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ProgressEstimator_Bytes.Dispatcher.Invoke(() => ProgressEstimator_Bytes.Text = ((RoboSharp.Results.Statistic)sender).ToString(true, true, "\n", true));

        }

        #endregion

        #region < On*Processed >

        void OnCopyProgressChanged(object sender, CopyProgressEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ProgressBar.Value = e.CurrentFileProgress;
                FileProgressPercent.Text = string.Format("{0}%", e.CurrentFileProgress);
            }));
        }

        void OnFileProcessed(object sender, FileProcessedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                CurrentOperation.Text = e.ProcessedFile.FileClass;
                CurrentFile.Text = e.ProcessedFile.Name;
                CurrentSize.Text = e.ProcessedFile.Size.ToString();
            }));
        }

        void OnCommandCompleted(object sender, RoboCommandCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ProgressGrid.IsEnabled = false;
                var results = e.Results;
                Console.WriteLine("Files copied: " + results.FilesStatistic.Copied);
                Console.WriteLine("Directories copied: " + results.DirectoriesStatistic.Copied);
            }));

            Command.OnProgressEstimatorCreated -= OnProgressEstimatorCreated;
            Command.OnCopyProgressChanged -= OnCopyProgressChanged;
            Command.OnFileProcessed -= OnFileProcessed;
            Command.OnCommandCompleted -= OnCommandCompleted;
        }
        #endregion
    }
}
