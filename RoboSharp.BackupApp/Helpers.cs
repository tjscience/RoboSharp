using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace RoboSharp.BackupApp
{
    public static class Helpers
    {
        public static bool IsInt(string text)
        {
            Regex regex = new Regex("[^0-9]+$", RegexOptions.Compiled);
            return !regex.IsMatch(text);
        }

        public static void IsAttribute_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[a-zA-Z]+$", RegexOptions.Compiled))
                e.Handled = true;
            if ("bdfgijklmopquvwxyzBDFGIJKLMOPQUVWXYZ".Contains(e.Text))
                e.Handled = true;
            if (((TextBox)sender).Text.Contains(e.Text))
                e.Handled = true;
        }

        public static void IsCopyFlag_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[a-zA-Z]+$", RegexOptions.Compiled))
                e.Handled = true;
            if ("bcefghijklmnpqrvwxyzBCEFGHIJKLMNPQRVWXYZ".Contains(e.Text))
                e.Handled = true;
            if (((TextBox)sender).Text.Contains(e.Text))
                e.Handled = true;
        }
    }
}
