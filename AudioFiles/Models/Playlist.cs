using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlaylistManager.ApplicationServices.Models;

namespace PlaylistManager.ApplicationServices
{
    public class Playlist
    {
        private string _playlistName;
        private List<AudioFile> _files = new List<AudioFile>();
        private long _size = 0;

        #region Constructors and Overloads
        public Playlist()
        {
        }

        public Playlist(string name)
        {
            _playlistName = name;
        }

        public Playlist(List<AudioFile> files)
        {
            _files = files;
        }

        public Playlist(string name, List<AudioFile> files)
        {
            _playlistName = name;
            _files = files;
        }
        #endregion

        /// <summary>
        /// The name of the playlist
        /// </summary>
        public string Name
        {
            get
            {
                return _playlistName;
            }
            set
            {
                _playlistName = value;
            }
        }

        /// <summary>
        /// Size (in bytes) of the playlist, calculated in a separate thread whenever updated
        /// </summary>
        public long Size
        {
            get
            {
                return _size;
            }
            internal set
            {
                _size = value;
            }
        }

        public string SizeWithSuffix
        {
            get
            {
                return _size.ToHumanReadable();
            }
        }

        /// <summary>
        /// Gets a read only copy of the files
        /// </summary>
        /// <returns>An immutable collection of AudioFiles</returns>
        public IEnumerable<AudioFile> RetreiveFiles()
        {
            return _files.AsReadOnly();
        }

        /// <summary>
        /// Calculates the size, in bytes, of the playlist
        /// </summary>
        internal async void CalculateSize()
        {
            Size = await Task<long>.Run(() =>
            {
                return _files.Sum(x => x.Size);
            });
        }
    }
}
