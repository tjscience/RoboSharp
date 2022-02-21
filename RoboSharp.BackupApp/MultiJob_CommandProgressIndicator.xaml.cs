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
using System.IO;

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

        public string JobName { 
            get => jobName;
            set 
            { 
                if (value != jobName)
                {
                    jobName = String.IsNullOrWhiteSpace(value) ? "" : value;
                    Dispatcher.Invoke(() => this.Header = $"Progress{(jobName == "" ? "" : $" - {jobName}")}");
                }
            }
             
        }   
        private string jobName;

        //Set to TRUE to enable logs for troubleshooting the objects provided by RoboCommand.OnCopyProgressChanged and OnFileProcessed.
        private bool debugMode { 
            get 
            {
                bool tmp = Dispatcher.Invoke(() =>
                {
                    if (this.Parent is null) return false;
                    var window = Window.GetWindow(this.Parent);
                    if (window.GetType() == typeof(RoboSharp.BackupApp.MainWindow))
                    {
                        return ((RoboSharp.BackupApp.MainWindow)window).chk_SaveEventLogs.IsChecked ?? false;
                    }
                    return false;
                });
                if (tmp && !ListObjSetupComplete) SetupListObjs();
                return tmp;
            }
        }

        private bool ListObjSetupComplete;
        private List<string> Dirs;
        private List<string> Files;
        private List<string> Dirs2;
        private List<string> Files2;
        private List<string> OrderLog_1;
        private List<string> OrderLog_2;


        public void BindToCommand(RoboCommand cmd)
        {
            Command = cmd;
            ListObjSetupComplete = false;
            if (cmd.IProgressEstimator != null)
            {
                BindToProgressEstimator(cmd.IProgressEstimator);
            }
            else
            {
                cmd.OnProgressEstimatorCreated += OnProgressEstimatorCreated;
            }
            if (debugMode == true)
            { 
                SetupListObjs();
            }
            cmd.OnCopyProgressChanged += OnCopyProgressChanged;
            cmd.OnFileProcessed += OnFileProcessed;
            cmd.OnCommandCompleted += OnCommandCompleted;
            JobName = cmd.Name;
        }

        private void SetupListObjs()
        {
            Dirs = new List<string> { "Dirs Reported by OnFileProcessed", "---------------------" };
            Files = new List<string> { "Files Reported by OnFileProcessed", "---------------------" };
            Dirs2 = new List<string> { "Unique Dirs Reported by CopyProgressChanged", "---------------------" };
            Files2 = new List<string> { "Unique Files Reported by CopyProgressChanged", "---------------------" };
            OrderLog_1 = new List<string> { "Files and Dirs In Order Reported by OnFileProcessed", "---------------------" };
            OrderLog_2 = new List<string> { "Files and Dirs In Order Reported by CopyProgressChanged", "---------------------" };
            ListObjSetupComplete = true;
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
        private void OnProgressEstimatorCreated(object sender, EventArgObjects.ProgressEstimatorCreatedEventArgs e) => BindToProgressEstimator(e.ResultsEstimate);

        private void BindToProgressEstimator(RoboSharp.Interfaces.IProgressEstimator e)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressEstimator_Files.Text = "Files";
                ProgressEstimator_Directories.Text = "Directories";
                ProgressEstimator_Bytes.Text = "Bytes";
            });
            e.ValuesUpdated += IProgressEstimatorValuesUpdated;
        }

        private void IProgressEstimatorValuesUpdated(Interfaces.IProgressEstimator sender, EventArgObjects.IProgressEstimatorUpdateEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressEstimator_Files.Text = e.FilesStatistic.ToString(true, true, "\n", true);
                ProgressEstimator_Bytes.Text = e.BytesStatistic.ToString(true, true, "\n", true);
                ProgressEstimator_Directories.Text = e.DirectoriesStatistic.ToString(true, true, "\n", true);
            });
        }

        #endregion

        #region < On*Processed >

        private string DirString(ProcessedFileInfo pf) => pf.FileClass + "(" + pf.Size + ") - " + pf.Name;
        private string FileString(ProcessedFileInfo pf) => pf.FileClass + "(" + pf.Size + ") - " + pf.Name;

        void OnCopyProgressChanged(object sender, CopyProgressEventArgs e) 
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                ProgressBar.Value = e.CurrentFileProgress;
                FileProgressPercent.Text = string.Format("{0}%", e.CurrentFileProgress);
            }));

            if (debugMode == true)
            {
                if (e.CurrentDirectory != null)
                {
                    var dirString = DirString(e.CurrentDirectory);
                    if (Dirs2.Count == 0 || dirString != Dirs2.Last())
                    {
                        Dirs2.Add(dirString);
                        OrderLog_2.Add(Environment.NewLine + DirString(e.CurrentDirectory));
                    }
                }
                if (e.CurrentFile != null)
                {
                    var fileString = FileString(e.CurrentFile);
                    if (Files2.Count == 0 || fileString != Files2.Last())
                    {
                        Files2.Add(fileString);
                        OrderLog_2.Add(FileString(e.CurrentFile));
                    }
                }
            }
        }

        void OnFileProcessed(object sender, FileProcessedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                CurrentOperation.Text = e.ProcessedFile.FileClass;
                CurrentFile.Text = e.ProcessedFile.Name;
                CurrentSize.Text = e.ProcessedFile.Size.ToString();
            }));

            if (debugMode == true)
            {
                if (e.ProcessedFile.FileClassType == FileClassType.NewDir)
                {
                    Dirs.Add(DirString(e.ProcessedFile));
                    OrderLog_1.Add(Environment.NewLine + DirString(e.ProcessedFile));
                }
                else if (e.ProcessedFile.FileClassType == FileClassType.File)
                {
                    Files.Add(FileString(e.ProcessedFile));
                    OrderLog_1.Add(FileString(e.ProcessedFile));
                }
            }
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

            try
            {
                if (debugMode == true)
                {
                    DirectoryInfo source = new DirectoryInfo(Command.CopyOptions.Source);
                    string path = System.IO.Path.Combine(source.Parent.FullName, "EventLogs") + "\\";
                    var PathDir = Directory.CreateDirectory(path);

                    Dirs.Add(""); Dirs.Add(""); Dirs.AddRange(Files);
                    File.AppendAllLines($"{path}{Command.Name}_OnFileProcessed.txt", Dirs);

                    Dirs2.Add(""); Dirs2.Add(""); Dirs2.AddRange(Files2);
                    File.AppendAllLines($"{path}{Command.Name}_CopyProgressChanged.txt", Dirs2);

                    File.AppendAllLines($"{path}{Command.Name}_OnFileProcessed_InOrder.txt", OrderLog_1);
                    File.AppendAllLines($"{path}{Command.Name}_CopyProgressChanged_InOrder.txt", OrderLog_2);
                }
                Directory.SetCurrentDirectory("C:\\");
            }
            catch { }
        }
        #endregion
    }
}
