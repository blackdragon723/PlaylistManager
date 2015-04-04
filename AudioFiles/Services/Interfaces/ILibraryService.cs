using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PlaylistManager.ApplicationServices.Services.Delegates;

namespace PlaylistManager.ApplicationServices.Services.Interfaces
{
    public interface ILibraryService
    {
        event FilesChangedEventHandler OnFilesChanged;

        void AddLibraryFolder(DirectoryInfo dir);
        void AddFile(FileInfo fi);
        void RemoveFile(AudioFile fi);
        void RemoveFile(string path);

        void ProcessDragDrop(string[] paths); 

        IEnumerable<FileInfo> EnumerateFolder(DirectoryInfo dir,
            IEnumerable<string> extensions);

        List<DirectoryInfo> LibraryFolders { get; }
        List<AudioFile> LibraryFiles { get; }
    }
}
