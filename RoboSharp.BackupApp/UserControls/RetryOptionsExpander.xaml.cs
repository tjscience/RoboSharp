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
    /// Interaction logic for RetryOptionsExpander.xaml
    /// </summary>
    public partial class RetryOptionsExpander : Expander
    {
        public RetryOptionsExpander()
        {
            InitializeComponent();
        }

        public void LoadFromIRoboCommand(IRoboCommand copy)
        {
            RetryCount.Text = copy.RetryOptions.RetryCount.ToString();
            RetryWaitTime.Text = copy.RetryOptions.RetryWaitTime.ToString();
        }

        public void ApplyToIRoboCommand(IRoboCommand copy)
        {
            if (!string.IsNullOrWhiteSpace(RetryCount.Text))
                copy.RetryOptions.RetryCount = Convert.ToInt32(RetryCount.Text);
            if (!string.IsNullOrWhiteSpace(RetryWaitTime.Text))
                copy.RetryOptions.RetryWaitTime = Convert.ToInt32(RetryWaitTime.Text);
        }

        private void IsNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helpers.IsInt(e.Text);
        }
    }
}
