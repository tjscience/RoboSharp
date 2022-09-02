using RoboSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for CopyOptionsExpander.xaml
    /// </summary>
    public partial class CopyOptionsExpander : Expander
    {
        public CopyOptionsExpander()
        {
            InitializeComponent();
        }

        public void ApplyToIRoboCommand(IRoboCommand copy)
        {
            
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
            if (!string.IsNullOrWhiteSpace(RunHoursStartTime.Text) && !string.IsNullOrWhiteSpace(RunHoursEndTime.Text))
            {
                var s = $"{RunHoursStartTime.Text}-{RunHoursEndTime.Text}";
                if (copy.CopyOptions.CheckRunHoursString(s))
                    copy.CopyOptions.RunHours = s;
                else
                {
                    MessageBox.Show("Invalid RunHours Format");
                    throw new Exception("Invalid RunHours Format");
                }
            }
        }

        public void LoadFromIRoboCommand(IRoboCommand copy)
        {
            string fileFilterItems = "";
            foreach (string s in copy.CopyOptions.FileFilter)
                fileFilterItems += s;
            FileFilter.Text = fileFilterItems;

            CopySubDirectories.IsChecked = copy.CopyOptions.CopySubdirectories;
            CopySubdirectoriesIncludingEmpty.IsChecked = copy.CopyOptions.CopySubdirectoriesIncludingEmpty;
            Depth.Text = copy.CopyOptions.Depth.ToString();
            EnableRestartMode.IsChecked = copy.CopyOptions.EnableRestartMode;
            EnableBackupMode.IsChecked = copy.CopyOptions.EnableBackupMode;
            EnableRestartModeWithBackupFallback.IsChecked = copy.CopyOptions.EnableRestartModeWithBackupFallback;
            UseUnbufferedIo.IsChecked = copy.CopyOptions.UseUnbufferedIo;
            EnableEfsRawMode.IsChecked = copy.CopyOptions.EnableEfsRawMode;
            CopyFlags.Text = copy.CopyOptions.CopyFlags;
            CopyFilesWithSecurity.IsChecked = copy.CopyOptions.CopyFilesWithSecurity;
            CopyAll.IsChecked = copy.CopyOptions.CopyAll;
            RemoveFileInformation.IsChecked = copy.CopyOptions.RemoveFileInformation;
            FixFileSecurityOnAllFiles.IsChecked = copy.CopyOptions.FixFileSecurityOnAllFiles;
            FixFileTimesOnAllFiles.IsChecked = copy.CopyOptions.FixFileTimesOnAllFiles;
            Purge.IsChecked = copy.CopyOptions.Purge;
            Mirror.IsChecked = copy.CopyOptions.Mirror;
            MoveFiles.IsChecked = copy.CopyOptions.MoveFiles;
            MoveFilesAndDirectories.IsChecked = copy.CopyOptions.MoveFilesAndDirectories;
            AddAttributes.Text = copy.CopyOptions.AddAttributes;
            RemoveAttributes.Text = copy.CopyOptions.RemoveAttributes;
            CreateDirectoryAndFileTree.IsChecked = copy.CopyOptions.CreateDirectoryAndFileTree;
            FatFiles.IsChecked = copy.CopyOptions.FatFiles;
            TurnLongPathSupportOff.IsChecked = copy.CopyOptions.TurnLongPathSupportOff;

            MonitorSourceChangesLimit.Text = copy.CopyOptions.MonitorSourceChangesLimit.ToString();
            MonitorSourceTimeLimit.Text = copy.CopyOptions.MonitorSourceTimeLimit.ToString();

        }

        private void IsNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Helpers.IsInt(e.Text);
        }

        void IsAttribute_PreviewTextInput(object sender, TextCompositionEventArgs e) => Helpers.IsAttribute_PreviewTextInput(sender, e);
        
        void IsCopyFlag_PreviewTextInput(object sender, TextCompositionEventArgs e) => Helpers.IsCopyFlag_PreviewTextInput(sender, e);

    }
}
