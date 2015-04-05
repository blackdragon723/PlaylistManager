using System.IO;

namespace PlaylistManager.ApplicationServices.Models
{
    /// <summary>
    /// Encapsulates a FileSystemWatcher
    /// </summary>
    /// <seealso cref="System.IO.FileSystemWatcher"/>
    internal class FileFolderWatcher
    {
        private FileSystemWatcher _watcher;

        /// <summary>
        /// Creates a FileSystemWatcher to watch for file changes in a folder
        /// </summary>
        /// <param name="path">Path of the folder or file to watch</param>
        /// <param name="changedEvent">The delegate function for when a file or folder changes</param>
        /// <param name="renamedEvent">The delegate function for when a file or folder is renamed</param>
        
        public FileFolderWatcher(string path, FileSystemEventHandler changedEvent, RenamedEventHandler renamedEvent)
        {
            // For an individual file the filter is the same as the path
            _watcher = path.isFile() ? CreateWatcher(Path.GetDirectoryName(path), path, changedEvent, renamedEvent, false) : CreateWatcher(path, "*.*", changedEvent, renamedEvent);
        }

        private static FileSystemWatcher CreateWatcher(string path, string filter, FileSystemEventHandler changedEvent, 
                                                RenamedEventHandler renamedEvent, bool subDirectories = true)
        {
            var watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = filter,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.DirectoryName |
                               NotifyFilters.LastAccess | NotifyFilters.LastWrite,
                IncludeSubdirectories = subDirectories,
                EnableRaisingEvents = true
            };

            watcher.Renamed += renamedEvent;
            watcher.Changed += changedEvent;
            watcher.Created += changedEvent;
            watcher.Deleted += changedEvent;

            return watcher;
        }
    }
}
