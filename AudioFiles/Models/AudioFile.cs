using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;


namespace PlaylistManager.ApplicationServices
{
    /// <summary>
    /// Encapsulates an audio file and provides methods for manipulation of the metadata
    /// </summary>
    public class AudioFile : BaseFile, IEquatable<AudioFile>, INotifyPropertyChanged
    {
        private readonly static string[] FileTypes = 
        {
            ".mp3", ".m4a", ".wav", ".flac", ".aiff"
        };

        public readonly static string[] SupportedFileExtensions = 
        {
            "*.mp3", "*.m4a", "*.wav", "*.flac", "*.aiff"
        };

        public static bool IsAudioFile(FileInfo file)
        {
            return FileTypes.Contains(file.Extension);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="path">Full path the the audio file</param>
        public AudioFile(string path) 
            : base(path)
        {
            if (!IsAudioFile(_fileInfo))
            {
                // TODO MAKE SURE THIS WORKS
                throw new InvalidOperationException("File must be one of the supported file types");
            }

            SetTag(path);
        }

        public AudioFile(FileInfo fi)
            : base(fi)
        {
            if (!IsAudioFile(_fileInfo))
            {
                // TODO MAKE SURE THIS WORKS
                throw new InvalidOperationException("File must be one of the supported file types");
            }

            SetTag(fi.FullName);
        }

        [JsonConstructor]
        public AudioFile(int id, string fullPath)
            : base(fullPath, id)
        {
            if (!IsAudioFile(_fileInfo))
            {
                // TODO MAKE SURE THIS WORKS
                throw new InvalidOperationException("File must be one of the supported file types");
            }

            SetTag(fullPath);
        }

        private void SetTag(string path)
        {
            Tag = new ID3Tag(path);

            if (Tag.Title == null)
            {
                Tag.Title = FileNameWithoutExtension;
            }
            RaisePropertyChanged("Tag");
        }

        /// <summary>
        /// Returns an instance of the ID3 Tag containing all the metadata
        /// </summary>
        [JsonIgnore]
        public ID3Tag Tag { get; private set; }

        /// <summary>
        /// Edits the specified metadata to the supplied value
        /// </summary>
        /// <param name="metaName">The name of the metadata property to change</param>
        /// <param name="newValue">The value to change it to</param>
        internal void EditId3Tag(string metaName, string newValue)
        {
            Tag.EditTag(metaName, newValue, Tag);
        }

        internal override void UpdatePath(string path)
        {
            base.UpdatePath(path);
            SetTag(path);
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region IEquatable

        public bool Equals(AudioFile other)
        {
            if (other == null)
                return false;

            return Tag != null && other.Tag == Tag;
        }

        public override int GetHashCode()
        {
            return Tag.GetHashCode();
        }

        #endregion

    }
}
