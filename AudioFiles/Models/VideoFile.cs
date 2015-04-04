using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistManager.ApplicationServices
{
    public class VideoFile : BaseFile
    {
        private static readonly string[] SupportedFileTypes =
        {
            "*.mp4", ".avi", ".mov"
        };

        internal static bool IsVideoFile(FileInfo file)
        {
            return SupportedFileTypes.Contains(file.Extension);
        }

        public VideoFile(string path)
            : base(path)
        {
        }

        #region IEquatable

        public bool Equals(VideoFile other)
        {
            if (other == null)
                return false;

            return FileNameWithExtension == other.FileNameWithExtension
                   && _fileInfo.Length == other._fileInfo.Length;
        }

        #endregion
    }
}
