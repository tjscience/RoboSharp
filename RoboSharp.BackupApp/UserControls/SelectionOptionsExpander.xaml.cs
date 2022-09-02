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
    /// Interaction logic for SelectionOptionsExpander.xaml
    /// </summary>
    public partial class SelectionOptionsExpander : Expander
    {
        public SelectionOptionsExpander()
        {
            InitializeComponent();
        }

        public void ApplyToIRoboCommand(IRoboCommand copy)
        {
            copy.SelectionOptions.OnlyCopyArchiveFiles = OnlyCopyArchiveFiles.IsChecked ?? false;
            copy.SelectionOptions.OnlyCopyArchiveFilesAndResetArchiveFlag = OnlyCopyArchiveFilesAndResetArchiveFlag.IsChecked ?? false;
            copy.SelectionOptions.IncludeAttributes = IncludeAttributes.Text;
            copy.SelectionOptions.ExcludeAttributes = ExcludeAttributes.Text;
#pragma warning disable CS0618 // These are marked as obsolete, but remain here for testing purposes. Properties were marked as obsolete due to backend change, but functionality should remain the same.
            copy.SelectionOptions.ExcludeFiles = ExcludeFiles.Text;
            copy.SelectionOptions.ExcludeDirectories = ExcludeDirectories.Text;
#pragma warning restore CS0618 
            copy.SelectionOptions.ExcludeOlder = ExcludeOlder.IsChecked ?? false;
            copy.SelectionOptions.ExcludeJunctionPoints = ExcludeJunctionPoints.IsChecked ?? false;
            copy.SelectionOptions.ExcludeNewer = ExcludeNewer.IsChecked ?? false;

        }


        public void LoadFromIRoboCommand(IRoboCommand copy)
        {
            OnlyCopyArchiveFiles.IsChecked = copy.SelectionOptions.OnlyCopyArchiveFiles;
            OnlyCopyArchiveFilesAndResetArchiveFlag.IsChecked = copy.SelectionOptions.OnlyCopyArchiveFilesAndResetArchiveFlag;
            IncludeAttributes.Text = copy.SelectionOptions.IncludeAttributes;
            ExcludeAttributes.Text = copy.SelectionOptions.ExcludeAttributes;
#pragma warning disable CS0618 // These are marked as obsolete, but remain here for testing purposes. Properties were marked as obsolete due to backend change, but functionality should remain the same.
            ExcludeFiles.Text = copy.SelectionOptions.ExcludeFiles;
            ExcludeDirectories.Text = copy.SelectionOptions.ExcludeDirectories;
#pragma warning restore CS0618
            ExcludeOlder.IsChecked = copy.SelectionOptions.ExcludeOlder;
            ExcludeJunctionPoints.IsChecked = copy.SelectionOptions.ExcludeJunctionPoints;
            ExcludeNewer.IsChecked = copy.SelectionOptions.ExcludeNewer;
        }

        void IsAttribute_PreviewTextInput(object sender, TextCompositionEventArgs e) => Helpers.IsAttribute_PreviewTextInput(sender, e);
    }
}
