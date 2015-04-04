using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PlaylistManager.ApplicationServices
{
    public static class Extensions
    {
        public static List<FileInfo> GetAudioFiles(this DirectoryInfo directory)
        {
            var files = directory.EnumerateFiles("*", SearchOption.AllDirectories);

            return files.Where(file => file.IsAudioFile()).ToList();
        }

        public static bool IsAudioFile(this FileInfo file)
        {
            return AudioFile.IsAudioFile(file);
        }

        private static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string ToHumanReadable(this long bytes)
        {
            int i = 0;
            decimal value = (decimal)bytes;
            while (Math.Round(value / 1024) >= 1)
            {
                value /= 1024;
                i++;
            }

            return string.Format("{0:n1} {1}", value, SizeSuffixes[i]);
        }

        public static IEnumerable<AudioFile> ToAudioFiles(this IEnumerable<FileInfo> files)
        {
            var ret = new List<AudioFile>();

            foreach (var file in files)
            {
                while (IsFileLocked(file)) ;

                ret.Add(new AudioFile(file.FullName));
            }

            return ret;
        }

        public static bool isFile(this string path)
        {
            return !File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

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

        // Copy Pasted from SO fuck knows how it works
        /// <summary>
        /// Adds Range of Values to Observable Collection
        /// </summary>
        /// <typeparam name="AudioFile"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        // todo clean this shiz
        public static void AddRange<AudioFile>(this ObservableCollection<AudioFile> collection, IEnumerable<AudioFile> items)
        {
            var enumerable = items as List<AudioFile> ?? items.ToList();
            if (collection == null || items == null || !enumerable.Any())
            {
                return;
            }

            Type type = collection.GetType();

            type.InvokeMember("CheckReentrancy", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, collection, null);
            var itemsProp = type.BaseType.GetProperty("Items", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            var privateItems = itemsProp.GetValue(collection) as IList<AudioFile>;
            foreach (var item in enumerable)
            {
                privateItems.Add(item);
            }

            type.InvokeMember("OnPropertyChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
              collection, new object[] { new PropertyChangedEventArgs("Count") });

            type.InvokeMember("OnPropertyChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
              collection, new object[] { new PropertyChangedEventArgs("Item[]") });

            type.InvokeMember("OnCollectionChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
              collection, new object[] { new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset) });
        }

        public static void RemoveRange<AudioFile>(this ObservableCollection<AudioFile> collection, IEnumerable<AudioFile> items)
        {
            var enumerable = items as List<AudioFile> ?? items.ToList();
            if (collection == null || items == null || !enumerable.Any())
            {
                return;
            }

            Type type = collection.GetType();

            type.InvokeMember("CheckReentrancy", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, collection, null);
            var itemsProp = type.BaseType.GetProperty("Items", BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
            var privateItems = itemsProp.GetValue(collection) as IList<AudioFile>;
            foreach (var item in enumerable)
            {
                privateItems.Remove(item);
            }

            type.InvokeMember("OnPropertyChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
              collection, new object[] { new PropertyChangedEventArgs("Count") });

            type.InvokeMember("OnPropertyChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
              collection, new object[] { new PropertyChangedEventArgs("Item[]") });

            type.InvokeMember("OnCollectionChanged", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null,
              collection, new object[] { new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset) });
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
        {
            return new ObservableCollection<T>(list);
        }
    }
}
