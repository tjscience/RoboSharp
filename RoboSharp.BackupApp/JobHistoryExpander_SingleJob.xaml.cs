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

        public RoboSharp.Results.IStatistic ByteStat { get; private set; }
        public RoboSharp.Results.IStatistic DirStat { get; private set; }
        public RoboSharp.Results.IStatistic FileStat { get; private set; }
        public bool IsResultsListBound { get; private set; } = false;

        private RoboSharp.Results.IRoboCopyResultsList ResultsList { get; set; }
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
        public void BindToResultsList(RoboSharp.Results.IRoboCopyResultsList list)
        {
            Unbind();

            //Set starting values 
            //Set starting values 
            ResultsList = list;
            ByteStat = list.BytesStatistic;
            DirStat = list.DirectoriesStatistic;
            FileStat = list.FilesStatistic;
            DirectoriesStatistic_PropertyChanged(null, null);
            FilesStatistic_PropertyChanged(null, null);
            BytesStatistic_PropertyChanged(null, null);
            
            
            IsResultsListBound = true;

            ////Trigger List Update
            DirStat.PropertyChanged += ResultsList_CollectionChanged;
            FileStat.PropertyChanged += ResultsList_CollectionChanged;
            ByteStat.PropertyChanged += ResultsList_CollectionChanged;

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

                DirStat.PropertyChanged -= ResultsList_CollectionChanged;
                FileStat.PropertyChanged -= ResultsList_CollectionChanged;
                ByteStat.PropertyChanged -= ResultsList_CollectionChanged;
            }
            ResultsList = null;
            ResultsObj = null;
        }

        private void DirectoriesStatistic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateLabel(lbl_SelectedItem_Dirs, DirStat);
        private void FilesStatistic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateLabel(lbl_SelectedItem_Files, FileStat);
        private void BytesStatistic_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => UpdateLabel(lbl_SelectedItem_Bytes, ByteStat);

        private void UpdateLabel(Label lbl, RoboSharp.Results.IStatistic stat)
        {
            Dispatcher.Invoke(
                () =>
                {
                    lbl.Content = stat?.ToString(true, true, "\n", true) ?? "";
                    if (!IsResultsListBound) ShowTotals(); else ResultsList_CollectionChanged(null, null);
                });
        }

        private void ShowTotals()
        {
            Results.RoboCopyResults result = ResultsObj;
            string NL = Environment.NewLine;
            Dispatcher.Invoke(() => 

            lbl_SelectedItem_Totals.Content = $"Selected Job:" +
                $"{NL}Source: {result?.Source ?? ""}" +
                $"{NL}Destination: {result?.Destination ?? ""}" +
                $"{NL}Total Directories: {result?.DirectoriesStatistic?.Total ?? 0}" +
                $"{NL}Total Files: {result?.FilesStatistic?.Total ?? 0}" +
                $"{NL}Total Size (bytes): {result?.BytesStatistic?.Total ?? 0}" +
                $"{NL}Speed (Bytes/Second): {result?.SpeedStatistic?.BytesPerSec ?? 0}" +
                $"{NL}Speed (MB/Min): {result?.SpeedStatistic?.MegaBytesPerMin ?? 0}" +
                $"{NL}Log Lines Count: {result?.LogLines?.Length ?? 0}" +
                $"{NL}{result?.Status.ToString() ?? ""}"
                
                );
        }

        /// <summary>
        /// Runs every time the ResultsList is updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResultsList_CollectionChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e == null || e.PropertyName != "Total") return;
            string NL = Environment.NewLine;
            if (ResultsList == null || ResultsList.Count == 0)
            {
                Dispatcher.Invoke( () => lbl_SelectedItem_Totals.Content = "Job History: None");
            }
            else
            {
                Dispatcher.Invoke(() => lbl_SelectedItem_Totals.Content = $"Job History:" +
                    $"{NL}Total Directories: {ResultsList.DirectoriesStatistic.Total}" +
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

