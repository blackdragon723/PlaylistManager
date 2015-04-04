using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TagLib;

namespace PlaylistManager.ApplicationServices
{
    public class ID3Tag : IEquatable<ID3Tag>
    {
        /// <summary>
        /// Creates a new ID3 Tag instance
        /// </summary>
        /// <param name="path">The path to the audio file</param>
        public ID3Tag(string path)
        {
            _path = path;
            var tag = GetTag();
            Album = tag.Tag.Album;
            Artist = tag.Tag.FirstPerformer;
            Title = tag.Tag.Title;
        }

        private readonly string _path;

        private File GetTag()
        {
            return File.Create(_path);
        }

        public string Artist { get; set; }

        public string Title { get; set; }

        public string Album { get; set; }

        internal bool SaveToFile()
        {
            var saved = true;

            try
            {
                var id3 = GetTag();
                id3.Tag.Performers = new[] { Artist };
                id3.Tag.Album = Album;
                id3.Tag.Title = Title;
                GetTag().Save();
            }
            catch
            {
                saved = false;
            }

            return saved;
        }

        public TimeSpan Duration
        {
            get { return GetTag().Properties.Duration; }
        }

        public int Bitrate
        {
            get { return GetTag().Properties.AudioBitrate; }
        }

        internal void EditTag(string metaName, string newValue, ID3Tag sender)
        {
            var property = GetType().GetProperty(metaName);
            property.SetValue(sender, newValue);
        }

        

        #region IEquatable

        public bool Equals(ID3Tag other)
        {
            return other.Artist == Artist && other.Title == Title;
        }

        public override int GetHashCode()
        {
            var hashArtist = Artist == null ? 0 : Artist.GetHashCode();
            var hashTitle = Title == null ? 0 : Title.GetHashCode();

            return hashArtist ^ hashTitle;
        }

        #endregion
    }
}
