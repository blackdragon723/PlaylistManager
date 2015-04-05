using System;
using Ookii.Dialogs.Wpf;
using PlaylistManager.ApplicationServices.Models;

namespace PlaylistManager.ApplicationServices.Services
{
    public static class DialogFactory
    {
        public static string AudioFilesFilter
        {
            get { return String.Format("Audio Files ({0})|{0}", string.Join("; ", AudioFile.SupportedFileExtensions)); }
        }

        public static string[] NewOpenFilesDialog(string filter)
        {
            var dialog = new VistaOpenFileDialog {Filter = filter, Multiselect = true};

            var success = dialog.ShowDialog();
            return success == true ? dialog.FileNames : null;
        }

        public static string NewOpenFolderDialog()
        {
            var dialog = new VistaFolderBrowserDialog();
            var success = dialog.ShowDialog();
            return success == true ? dialog.SelectedPath : null;
        }
    }
}
