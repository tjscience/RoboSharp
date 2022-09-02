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
    public partial class SingleJobStats : ScrollViewer
    {
        public SingleJobStats()
        {
            InitializeComponent();
        }

        public SingleJobStats(RoboSharp.Results.RoboCopyResults ResultsObj)
        {
            InitializeComponent();
            BindToResults(ResultsObj);
        }

        public RoboSharp.Interfaces.IStatistic ByteStat { get; private set; }
        public RoboSharp.Interfaces.IStatistic DirStat { get; private set; }
        public RoboSharp.Interfaces.IStatistic FileStat { get; private set; }
        public bool IsResultsListBound { get; private set; } = false;

        private RoboSharp.Interfaces.IRoboCopyResultsList ResultsList { get; set; }
        private RoboSharp.Results.RoboCopyResults ResultsObj { get; set; }


        /// <summary>
        /// Bind to a static ResultsObject
        /// </summary>
        /// <param name="resultsObj"></param>
        public void BindToResults(RoboSharp.Results.RoboCopyResults resultsObj)
        {
            Unbind();

            //Set starting values 
            ResultsObj = resultsObj;
            ByteStat = resultsObj?.BytesStatistic;
            DirStat = resultsObj?.DirectoriesStatistic;
            FileStat = resultsObj?.FilesStatistic;
            IsResultsListBound = false;

            DirectoriesStatistic_PropertyChanged(null, null);
            FilesStatistic_PropertyChanged(null, null);
            BytesStatistic_PropertyChanged(null, null);

        }


        /// <summary>
        /// Bind to a ResultsList
        /// </summary>
        /// <param name="list"></param>
        public void BindToResultsList(RoboSharp.Interfaces.IRoboCopyResultsList list)
        {
            Unbind();

            //Set starting values 
            ResultsList = list;
            ByteStat = list.BytesStatistic;
            DirStat = list.DirectoriesStatistic;
            FileStat = list.FilesStatistic;
            
            IsResultsListBound = true;

            // Initialize the values
            DirectoriesStatistic_PropertyChanged(null, null);
            FilesStatistic_PropertyChanged(null, null);
            BytesStatistic_PropertyChanged(null, null);

            //Update Summary Event
            ShowResultsListSummary(null, new System.ComponentModel.PropertyChangedEventArgs(""));
            list.CollectionChanged += (o,e) => ShowResultsListSummary(o, null);
            
            ////Bind in case updates
            DirStat.PropertyChanged += DirectoriesStatistic_PropertyChanged;
            FileStat.PropertyChanged += FilesStatistic_PropertyChanged;
            ByteStat.PropertyChanged += BytesStatistic_PropertyChanged;
        }

        private void Unbind()
        {
            if (ResultsList != null | ResultsObj != null)
            {
                DirStat.PropertyChanged -= DirectoriesStatistic_PropertyChanged;
                FileStat.PropertyChanged -= FilesStatistic_PropertyChanged;
                ByteStat.PropertyChanged -= BytesStatistic_PropertyChanged;

                DirStat.PropertyChanged -= ShowResultsListSummary;
                FileStat.PropertyChanged -= ShowResultsListSummary;
                ByteStat.PropertyChanged -= ShowResultsListSummary;
            }
            ResultsList = null;
            ResultsObj = null;
        }

        private void DirectoriesStatistic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateLabel(lbl_SelectedItem_Dirs, DirStat);
        private void FilesStatistic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateLabel(lbl_SelectedItem_Files, FileStat);
        private void BytesStatistic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateLabel(lbl_SelectedItem_Bytes, ByteStat);

        private void UpdateLabel(Label lbl, RoboSharp.Interfaces.IStatistic stat)
        {
            // Dispatcher is required to ensure this code runs on the UI thread, since all processing up to this point was potentially done on a worker thread.
            Dispatcher.Invoke(
                () =>
                {
                    lbl.Content = stat?.ToString(false, true, "\n", false) ?? "";
                    if (!IsResultsListBound) ShowSelectedJobSummary();
                });
        }

        /// <summary>
        /// Show the Summary of the Selected job
        /// </summary>
        private void ShowSelectedJobSummary()
        {
            Results.RoboCopyResults result = ResultsObj;
            string NL = Environment.NewLine;
            Dispatcher.Invoke(() =>
            {
                lbl_SelectedItem_Totals.Content = 
                    $"Source: {result?.Source ?? ""}" +
                    $"{NL}Destination: {result?.Destination ?? ""}" +
                    $"{NL}Total Directories: {result?.DirectoriesStatistic?.Total ?? 0}" +
                    $"{NL}Total Files: {result?.FilesStatistic?.Total ?? 0}" +
                    $"{NL}Total Size (bytes): {result?.BytesStatistic?.Total ?? 0}" +
                    $"{NL}Speed (Bytes/Second): {result?.SpeedStatistic?.BytesPerSec ?? 0}" +
                    $"{NL}Speed (MB/Min): {result?.SpeedStatistic?.MegaBytesPerMin ?? 0}" +
                    $"{NL}Log Lines Count: {result?.LogLines?.Length ?? 0}" +
                    $"{NL}{result?.Status.ToString() ?? ""}";

            });
        }

        /// <summary>
        /// Show Summary from a ResultsList object
        /// </summary>
        private void ShowResultsListSummary(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e == null || (e.PropertyName != "" && e.PropertyName != "Total")) return;
            string NL = Environment.NewLine;
            if (ResultsList == null || ResultsList.Count == 0)
            {
                Dispatcher.Invoke( () => lbl_SelectedItem_Totals.Content = "Job History: None");
            }
            else
            {
                Dispatcher.Invoke(() => lbl_SelectedItem_Totals.Content =
                    $"Total Directories: {ResultsList.DirectoriesStatistic.Total}" +
                    $"{NL}Total Files: {ResultsList.FilesStatistic.Total}" +
                    $"{NL}Total Size (bytes): {ResultsList.BytesStatistic.Total}" +
                    $"{NL}Speed (Bytes/Second): {ResultsList.SpeedStatistic.BytesPerSec}" +
                    $"{NL}Speed (MB/Min): {ResultsList.SpeedStatistic.MegaBytesPerMin}" +
                    $"{NL}Any Jobs Cancelled: {(ResultsList.Status.WasCancelled ? "YES" : "NO")}" +
                    $"{NL}{ResultsList.Status}");
            }
        }

    }
}

