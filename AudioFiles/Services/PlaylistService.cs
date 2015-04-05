using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PlaylistManager.ApplicationServices.Models;
using PlaylistManager.ApplicationServices.Services.Interfaces;

namespace PlaylistManager.ApplicationServices
{
    public class PlaylistService : IPlaylistService
    {
        public List<Playlist> Playlists { get; private set; }
        private Playlist _activePlaylist;

        public PlaylistService()
        {
            Playlists = new List<Playlist>();
        }

        /// <summary>
        /// Updates the active playlist to the provided one
        /// </summary>
        /// <param name="name">The name of the playlist make active</param>
        public void UpdateActivePlaylist(string name)
        {
            try
            {
                _activePlaylist = Playlists.Find(x => x.Name == name);
            }
            catch
            {
                throw;
                // TODO fix this
            }
        }

        /// <summary>
        /// Gets a readonly copy of the files from the active playlist
        /// </summary>
        /// <returns>A readonly copy of the audio files from the active playlist</returns>
        public IEnumerable<AudioFile> RetrieveFilesFromActive()
        {
            return _activePlaylist.RetreiveFiles();
        }

        /// <summary>
        /// Creates .m3u files for all playlists in the collection
        /// </summary>
        /// <param name="destDir">Directory path to create the files</param>
        public void SaveAllPlaylists(string destDir)
        {
            foreach (var p in Playlists)
            {
                SavePlaylistFile(p, destDir);
            }
        }

        /// <summary>
        /// Creates .m3u files for all playlists with given names
        /// </summary>
        /// <param name="playlistNames">Names of the playlists to create files for</param>
        /// <param name="destDir">Directory path to create the files</param>
        public void SavePlaylistsFromName(List<string> playlistNames, string destDir)
        {
            var playlistsToCreate = new List<Playlist>();

            foreach (var playlist in playlistNames.Select(name => Playlists.Find(x => x.Name == name)))
            {
                if (playlist == null)
                {
                    // Shouldn't happen
                    throw new KeyNotFoundException("Playlist name wasn't found in the list");
                }

                playlistsToCreate.Add(playlist);
            }

            foreach (var p in playlistsToCreate)
            {
                SavePlaylistFile(p, destDir);
            }
        }


        public long ActivePlaylistSize()
        {
            /* TODO Thread this shit
            long size = 0;
            foreach (AudioFile f in _activePlaylist.RetreiveFiles())
            {
                size += f.Size;
            }
            return size;
             */
            return 0;
        }

        /* TEST IF NOT NEEDED, use CreatePlaylistsFromName(selected.name) instead
        /// <summary>
        /// Creates a .m3u files for the selected playlist only
        /// </summary>
        /// <param name="destDir">Directory path to create the files</param>
        public void CreatePlaylistFromSelected(string destDir)
        {
            CreatePlaylistFile(_activePlaylist, destDir);
        }
        */

        /// <summary>
        /// Generates a .m3u file from the given playlist and writes it to the specified location
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="destDir">The directory the file is to be located</param>
        private static void SavePlaylistFile(Playlist playlist, string destDir)
        {
            // TODO DUPLICATES
            const string extInf = "#EXTINF:{0},{1} - {2}";

            var outBuilder = new StringBuilder();
            outBuilder.AppendLine("#EXTM3U");

            foreach (var audioFile in playlist.RetreiveFiles())
            {
                var duration = audioFile.Tag.Duration;
                var artist = audioFile.Tag.Artist;
                var title = audioFile.Tag.Title;
                var fileName = audioFile.FullPath;

                //  #EXTINF:Time,Artist - Title"
                outBuilder.AppendLine(String.Format(extInf, Math.Round(duration.TotalSeconds), artist, title));
                outBuilder.AppendLine(fileName);
            }

            var dir = new DirectoryInfo(destDir); // Ensures correct formatting

            if (!dir.Exists)
            {
                dir.Create();
            }

            var destFile = new FileInfo(Path.Combine(dir.FullName + "\\" + playlist.Name  + ".m3u8"));

            try
            {
                File.WriteAllText(destFile.FullName, outBuilder.ToString());
            }
            catch (IOException)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a playlist from a m3u files and adds it to the collection
        /// </summary>
        /// <param name="path">The path to the m3u file</param>
        public void CreateAndAddPlaylistFromFile(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var count = Playlists.Count(x => x.Name == name);
            if (count != 0)
            {
                name += "_" + count;
            }
            var p = ParseM3UFile(new FileInfo(path), name);

            if (!p.RetreiveFiles().Any())
            {
                return;
            }

            p.CalculateSize();
            Playlists.Add(p);
        }

        // TODO Thread this shit parsing m3u with await and shit

        // create playlist from given m3u, name is from the name of the file, given without spaces
        private Playlist ParseM3UFile(FileInfo m3u, string name)
        {
            List<AudioFile> audioFiles;
            using (var sr = new StreamReader(m3u.FullName))
            {
                var data = sr.ReadToEnd();

                var cleanedData = Regex.Replace(data, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                if (cleanedData.Contains("#EXTM3U"))
                {
                    // Validate it is the first line
                    var firstLine = cleanedData.Substring(0, cleanedData.IndexOf("\n", StringComparison.Ordinal));
                    if (!firstLine.Contains("#EXTM3U"))
                    {
                        // TODO Invalid M3U
                        throw new Exception();
                    }

                    audioFiles = ParseExtM3U(data, m3u.DirectoryName);
                }
                else
                {
                    audioFiles = ParseStandardM3U(data, m3u.DirectoryName);
                }
            }

            return new Playlist(name, audioFiles);
        }

        /// <summary>
        /// Parses an extended M3U File
        /// </summary>
        /// <param name="data">The string of the M3U File</param>
        /// <returns>A list of AudioFiles from the M3U</returns>
        private List<AudioFile> ParseExtM3U(string data, string dir)
        {
            var ret = new List<AudioFile>();
            // Strip first line
            data = data.Remove(0, data.IndexOf("\n", StringComparison.Ordinal) + 1);
            // We have lines with no spaces, every odd line is ExtInf, even is the song
            using (StringReader sr = new StringReader(data))
            {
                string line;
                var count = 1;

                string artist = "", track = "";
                while (!String.IsNullOrWhiteSpace(line = sr.ReadLine()))
                {
                    // ExtInf
                    if (count % 2 != 0)
                    {
                        if (!Regex.Match(line, @"#EXTINF:\d+,(\w+\s*)*-\s*(\w+\s*)*").Success)
                        {
                            // TODO error something elegant tell file was invalid or someshit
                            continue;
                        }
                        artist = "";
                        track = "";

                        var parts = Regex.Split(line, @"#EXTINF:\d*,");
                        var meta = parts[1].Split('-');

                        artist = meta[0].Trim();
                        track = meta[1].Trim();
                    }
                    // Song Data
                    else
                    {
                        try
                        {
                            var file = ParseM3ULine(line, dir);

                            if (String.IsNullOrWhiteSpace(file.Tag.Artist))
                            {
                                file.EditId3Tag("Artist", artist);
                            }
                            if (String.IsNullOrWhiteSpace(file.Tag.Title))
                            {
                                file.EditId3Tag("Title", track);
                            }

                            ret.Add(file);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Exception caught, {0}", e.Message);
                            continue;
                        }
                    }
                    count++;
                }
            }

            return ret;
        }

        /// <summary>
        /// Parses a line of a m3u file and creates an audio file from it
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <param name="dir">Directory the m3u file resides in</param>
        /// <returns>The audio file that was parsed</returns>
        public AudioFile ParseM3ULine(string line, string dir)
        {
            // Split Number
            line.Replace("\n", "");
            line.Replace("\r", "");

            var di = new DirectoryInfo(dir);
            var fullPath = di.FullName + "\\" + line;
            // TODO Catch if not found
            return new AudioFile(fullPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private List<AudioFile> ParseStandardM3U(string data, string dir)
        {
            var ret = new List<AudioFile>();

            using (var sr = new StringReader(data))
            {
                string line;
                while (!String.IsNullOrWhiteSpace(line = sr.ReadLine()))
                {
                    ret.Add(ParseM3ULine(line, dir));
                }
            }
            
            return ret;
        }
    }
}
