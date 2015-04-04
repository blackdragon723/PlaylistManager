using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using PlaylistManager.ApplicationServices;

namespace PlaylistManager.WPF
{
    public static class Extensions
    {
        public static bool IsDirectory(this string path)
        {
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch
            {
                return false;
            }
        }

        public static IEnumerable<AudioFile> GetAudioFiles(this DirectoryInfo directory, bool onlyNoCoverart)
        {
            var ret = new List<AudioFile>();
            var files = directory.GetAllFiles();
            files.ToList().AsParallel().ForAll(x =>
            {
                try
                {
                    var audioFile = new AudioFile(x);
                    ret.Add(audioFile);
                }
                catch
                {
                    throw;
                }
            });

            return ret;
        }

        public static string[] GetDropFileNames(this IDataObject data)
        {
            return (string[])data.GetData(DataFormats.FileDrop);
        }

        public static IEnumerable<FileInfo> GetAllFiles(this DirectoryInfo directory)
        {
            return directory.EnumerateFiles("*.*", SearchOption.AllDirectories);
        }

        public static bool IsAudioFile(this string path)
        {
            return AudioFile.IsAudioFile(new FileInfo(path));
        }

        public static void DoubleBuffered(this DataGrid dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }
    }
}
