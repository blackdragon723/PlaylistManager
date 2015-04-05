using System;

namespace PlaylistManager.ApplicationServices
{
    public enum FileChangedType
    {
        Renamed,
        Deleted,
        Added
    };

    public class FilesChangedEventArgs : EventArgs
    {
        public FilesChangedEventArgs(FileChangedType type)
        {
            ChangeType = type;
        }
        public FileChangedType ChangeType { get; private set; }
    }
}
