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
    public partial class JobHistoryExpander : Expander
    {
        public JobHistoryExpander()
        {
            InitializeComponent();
        }

        public JobHistoryExpander(RoboSharp.Interfaces.IRoboCopyResultsList resultsList)
        {
            InitializeComponent();
            BindToList(resultsList);
        }

        public void UpdateDescriptionLblText(string HeaderText = "This contains a list of the results from all previous runs during this session.")
        {
            Dispatcher.Invoke(() => DescriptionLbl.Content = HeaderText);
        }

        public RoboSharp.Interfaces.IRoboCopyResultsList ResultsList { get; private set; }

        public void BindToList(RoboSharp.Interfaces.IRoboCopyResultsList resultsList)
        {

            OverallStats.BindToResultsList(resultsList);
            
            // Dispatcher is required to ensure this code runs on the UI thread, since the event was generated/reacted to potentially on a seperate thread.
            Dispatcher.Invoke(() =>
            {
                ResultsList = resultsList;
                ListBox_JobResults.ItemsSource = resultsList;
            });

            ListBox_JobResults.SelectionChanged += ListBox_JobResults_SelectionChanged;
        }

        private void ListBox_JobResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var res = (RoboSharp.Results.RoboCopyResults)ListBox_JobResults.SelectedItem;
            SelectedJobStats.BindToResults(res);

            Dispatcher.Invoke(() =>
            {
                if (res == null)
                    GroupBox_SelectedJob.Header = $"Selected Job Results";
                else
                    GroupBox_SelectedJob.Header = $"Selected Job Results: {res.JobName}";
            });
        }



        #region < Buttons >

        private void Remove_Selected_Click(object sender, RoutedEventArgs e)
        {
            Results.RoboCopyResults result = (Results.RoboCopyResults)this.ListBox_JobResults.SelectedItem;

            var list = (Results.RoboCopyResultsList)ResultsList;
            list.Remove(result);

        }

        #endregion

    }
}
