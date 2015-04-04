using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;

namespace PlaylistManager.ApplicationServices
{
    public abstract class BaseFile
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="path">Full path to the file to create.</param>
        protected BaseFile(string path)
        {
            // TODO AIF, PETER AND THE WOLF??
            _fileInfo = new FileInfo(path);
            _id = IDGenerator.Next();
        }

        protected BaseFile(FileInfo fi)
        {
            _fileInfo = fi;
            _id = IDGenerator.Next();
        }

        /// <summary>
        /// Constructer used when unserializing data
        /// </summary>
        /// <param name="path">Full path to the file</param>
        /// <param name="id">The integer ID of the file</param>
        protected BaseFile(string path, int id)
        {
            _id = id;
            _fileInfo = new FileInfo(path);
        }

        /// <summary>
        /// Backing variable for the file's FileInfo
        /// </summary>
        protected FileInfo _fileInfo;

        protected int _id;
        public int ID
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets an instance of the parent directory of the file
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo ParentDirectory
        {
            get
            {
                return _fileInfo.Directory;
            }
        }

        internal virtual void UpdatePath(string path)
        {
            _fileInfo = new FileInfo(path);
        }

        /// <summary>
        /// The full path to the file (including extension)
        /// </summary>
        public string FullPath
        {
            get
            {
                return _fileInfo.FullName;
            }
        }

        /// <summary>
        /// The file name (no path) with extension
        /// </summary>
        [JsonIgnore]
        public string FileNameWithExtension
        {
            get
            {
                return _fileInfo.Name;
            }
        }

        /// <summary>
        /// The file name (no path) without extension
        /// </summary>
        [JsonIgnore]
        public string FileNameWithoutExtension
        {
            get
            {
                return _fileInfo.Name.Substring(0, _fileInfo.Name.Length - _fileInfo.Extension.Length);
            }
        }

        /// <summary>
        /// The extension of the file (including .)
        /// </summary>
        [JsonIgnore]
        public string Extension
        {
            get
            {
                return _fileInfo.Extension;
            }
        }

        /// <summary>
        /// Gets the size of the file (in bytes)
        /// </summary>
        [JsonIgnore]
        public long Size
        {
            get
            {
                return _fileInfo.Length;
            }
        }
    }
}
