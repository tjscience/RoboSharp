using RoboSharp.Interfaces;
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

namespace RoboSharp.BackupApp.UserControls
{
    /// <summary>
    /// Interaction logic for LoggingOptionsExpander.xaml
    /// </summary>
    public partial class LoggingOptionsExpander : Expander
    {
        public LoggingOptionsExpander()
        {
            InitializeComponent();
        }

        public void LoadFromIRoboCommand(IRoboCommand copy)
        {
            // logging options
            VerboseOutput.IsChecked = copy.LoggingOptions.VerboseOutput;
            NoFileSizes.IsChecked = copy.LoggingOptions.NoFileSizes;
            NoProgress.IsChecked = copy.LoggingOptions.NoProgress;
            ChkListOnly.IsChecked = copy.LoggingOptions.ListOnly;
        }

        public void ApplyToIRoboCommand(IRoboCommand copy)
        {
            // logging options
            copy.LoggingOptions.VerboseOutput = VerboseOutput.IsChecked ?? false;
            copy.LoggingOptions.NoFileSizes = NoFileSizes.IsChecked ?? false;
            copy.LoggingOptions.NoProgress = NoProgress.IsChecked ?? false;
            copy.LoggingOptions.ListOnly = ChkListOnly.IsChecked ?? false;
        }

        private void IsNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helpers.IsInt(e.Text);
        }
    }
}
