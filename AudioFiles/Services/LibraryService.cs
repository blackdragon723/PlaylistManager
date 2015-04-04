using PlaylistManager.ApplicationServices.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter.Xml;
using PlaylistManager.ApplicationServices;
using PlaylistManager.ApplicationServices.Services.Delegates;
using PlaylistManager.ApplicationServices.Services.Interfaces;

namespace PlaylistManager.ApplicationServices
{
    // TODO split into Library Model and LibraryService
    public class LibraryService : ILibraryService
    {

        private bool _allowManualSet = true;
        internal void SetLibraryFiles(List<AudioFile> files)
        {
            if (!_allowManualSet) return;

            LibraryFiles = files;
            var id = LibraryFiles.Max(x => x.ID);
            IDGenerator.SetID(id);
            _allowManualSet = false;

            CreateWatchers(files);
        }


        // TODO video files -> mp3
        public List<DirectoryInfo> LibraryFolders { get; private set; }
        public List<AudioFile> LibraryFiles { get; private set; }
        private List<FileFolderWatcher> FileSystemWatchers { get; set; }

        public LibraryService()
        {
            LibraryFolders = new List<DirectoryInfo>();
            LibraryFiles = new List<AudioFile>();
            FileSystemWatchers = new List<FileFolderWatcher>();
        }

        /// <summary>
        /// Occurs when the library files are changed
        /// </summary>
        public event FilesChangedEventHandler OnFilesChanged;

        /// <summary>
        /// Notifies subscribers of a change in the library
        /// </summary>
        /// <param name="type">The type of change that occured</param>
        protected void FilesChanged(FileChangedType type)
        {
            if (OnFilesChanged != null)
            {
                OnFilesChanged(this, new FilesChangedEventArgs(type));
            }  
        }

        private static Dictionary<string, int> GetFolderCounts(List<AudioFile> files)
        {
            var directories = files.Select(x => x.ParentDirectory.FullName).Distinct().ToList();
            var counts = new Dictionary<string, int>();
            foreach (var d in directories)
            {
                var count = files.Count(x => x.ParentDirectory.FullName == d);
                counts.Add(d, count);
            }

            return counts;
        }

        private void CreateWatchers(List<AudioFile> files)
        {
            var counts = GetFolderCounts(files);
            foreach (var dir in counts)
            {
                if (dir.Value < 3)
                {
                    // Create watchers for individual files
                    foreach (var watcher in files.Where(x => x.ParentDirectory.FullName == dir.Key).Select(file => new FileFolderWatcher(file.FullPath, FileSystem_OnChanged, FileSystem_OnRenamed)))
                    {
                        FileSystemWatchers.Add(watcher);
                    }
                }
                else
                {
                    var watcher = new FileFolderWatcher(dir.Key, FileSystem_OnChanged, FileSystem_OnRenamed);
                    FileSystemWatchers.Add(watcher);
                }
            }
        }

        /// <summary>
        /// Adds a folder to the library and scans it
        /// </summary>
        /// <param name="dir">The directory to add</param>
        public void AddLibraryFolder(DirectoryInfo dir)
        {
            // No duplicates
            if (LibraryFolders.Exists(x => x.FullName == dir.FullName)) return;

            LibraryFolders.Add(dir);
            ScanDirectory(dir);
            FileSystemWatchers.Add(new FileFolderWatcher(dir.FullName, FileSystem_OnChanged, FileSystem_OnRenamed));
        }

        /// <summary>
        /// Checks and waits for a file to be free
        /// before adding it to the library
        /// </summary>
        /// <param name="fi">The file to add</param>
        public void AddFile(FileInfo fi)
        {
            // No duplicates
            if (!LibraryFiles.Exists(x => x.FileNameWithExtension == fi.FullName))
            {
                new Task(() =>
                {
                    while (Extensions.IsFileLocked(fi)) ;
                    var audioFile = new AudioFile(fi.FullName);
                    AddFile(audioFile);
                }).Start();
            }
        }

        public bool IsProcessing { get; set; }

        public void ProcessDragDrop(string[] paths)
        {
            IsProcessing = true;

            foreach (var path in paths)
            {
                if (path.IsDirectory())
                {
                    var directory = new DirectoryInfo(path);
                    ScanDirectory(directory);
                }
                else
                {
                    var fi = new FileInfo(path);
                    AddFile(fi);
                }
            }

            IsProcessing = false;
            FinishedProcessing();
        }

        public event FinishedProcessingEventHandler OnFinishedProceessing;

        protected void FinishedProcessing()
        {
            if (OnFinishedProceessing != null)
            {
                OnFinishedProceessing(this, new EventArgs());
            }
        }

        /// <summary>
        /// Adds an audio file to the library
        /// </summary>
        /// <param name="file">The file to add</param>
        private void AddFile(AudioFile file)
        {
            if (LibraryFiles.Exists(x => x.FileNameWithExtension == file.FileNameWithExtension)) return;

            LibraryFiles.Add(file);
            FilesChanged(FileChangedType.Added);

            //  Create individual watcher if file not part of a library directory
            if (LibraryFolders.Count(x => x.FullName == file.ParentDirectory.FullName) == 0)
            {
                FileSystemWatchers.Add(new FileFolderWatcher(file.FullPath, FileSystem_OnChanged, FileSystem_OnRenamed));
            }
        }

        /// <summary>
        /// Removes an audio file from the library
        /// </summary>
        /// <param name="fi">The file to remove</param>
        public void RemoveFile(AudioFile fi)
        {
            LibraryFiles.Remove(fi);
            FilesChanged(FileChangedType.Deleted);
        }

        /// <summary>
        /// Removes all the files with a specified path from the library
        /// </summary>
        /// <param name="path">The path of the files to remove</param>
        public void RemoveFile(string path)
        {
            LibraryFiles.RemoveAll(x => x.FullPath == path);
            FilesChanged(FileChangedType.Deleted);
        }

        private void FileSystem_OnChanged(object source, FileSystemEventArgs e)
        {
            // if its not a file (i.e. a directory) or it is a valid file
            if (Regex.Match(e.FullPath, @"\.\w*$").Success && !AudioFile.IsAudioFile(new FileInfo(e.FullPath))) return;

            switch (e.ChangeType)
            {
                // todo make new audiofile????
                case WatcherChangeTypes.Changed: break;
                case WatcherChangeTypes.Created:
                {
                    if (e.FullPath.isFile())
                    {
                        AddFile(new FileInfo(e.FullPath));
                    }
                    else
                    {
                        AddLibraryFolder(new DirectoryInfo(e.FullPath));
                    }
                    break;
                }
                case WatcherChangeTypes.Deleted:
                {
                    if (LibraryFiles.Any(x => x.FullPath == e.FullPath))
                    {
                        RemoveFile(e.FullPath);
                    }
                    else
                    {
                        RemoveDirectory(new DirectoryInfo(e.FullPath));
                    }
                    break;
                }
                default: return;
            }
        }

        private void FileSystem_OnRenamed(object source, RenamedEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Renamed) return;

            // File or Folder?
            if (e.FullPath.isFile())
            {
                var file = LibraryFiles.Single(x => x.FullPath == e.OldFullPath);
                file.UpdatePath(e.FullPath);
            }
            else
            {
                foreach (var file in LibraryFiles.Where(x => x.FullPath.Contains(e.OldFullPath)))
                {
                    var oldPath = file.FullPath;
                    var newPath = oldPath.Replace(e.OldFullPath, e.FullPath);

                    var actualFile = LibraryFiles.Single(x => x.FullPath == file.FullPath);
                    actualFile.UpdatePath(newPath);
                }

                foreach (var dir in LibraryFolders.Where(x => x.FullName.Contains(e.OldFullPath)))
                {
                    var oldPath = dir.FullName;
                    var newPath = oldPath.Replace(e.OldFullPath, e.FullPath);

                    var actualDir = LibraryFolders.Single(x => x.FullName == dir.FullName);
                    actualDir = new DirectoryInfo(newPath);
                }
            }
            FilesChanged(FileChangedType.Renamed);
        }

        private void RemoveDirectory(FileSystemInfo dir)
        {
            var filesToDelete = LibraryFiles.Where(x => x.FullPath.Contains(dir.FullName)).ToList();
            foreach (var file in filesToDelete)
            {
                LibraryFiles.Remove(file);
            }


            LibraryFolders.RemoveAll(x => x.FullName.Contains(dir.FullName));

            FilesChanged(FileChangedType.Deleted);
        }

        private async void ScanDirectoryAsync(DirectoryInfo dir)
        {
            var files = await Task.Run(() => EnumerateFolder(dir, AudioFile.SupportedFileExtensions).ToAudioFiles());
            
            var newFiles = files.Where(x =>
                LibraryFiles.All(y => x.FileNameWithExtension != y.FileNameWithExtension));

            newFiles.ToList().ForEach(AddFile);
        }

        private void ScanDirectory(DirectoryInfo dir)
        {
            var files = EnumerateFolder(dir, AudioFile.SupportedFileExtensions).ToAudioFiles();
   
            var newFiles = files.Where(x =>
                LibraryFiles.All(y => x.FileNameWithExtension != y.FileNameWithExtension));

            newFiles.ToList().ForEach(AddFile);
        }

        public IEnumerable<FileInfo> EnumerateFolder(DirectoryInfo dir, IEnumerable<string> extensions)
        {
            return extensions.AsParallel()
                .SelectMany(x =>
                    dir.EnumerateFiles(x, SearchOption.AllDirectories)).ToList();
        }
    }
}
