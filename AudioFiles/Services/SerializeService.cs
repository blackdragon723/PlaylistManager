using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PlaylistManager.ApplicationServices.Models;
using PlaylistManager.ApplicationServices.Services;

namespace PlaylistManager.ApplicationServices
{
    public class SerializeService
    {
        private readonly LibraryService _libraryService;

        private string appDataPath;
        private string libraryJson = "library.json";
        private string playlistsJson = "playlists.json";

        public SerializeService(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        public void Load()
        {
            var json = ReadJson(libraryJson);
            var libraryFiles = JsonConvert.DeserializeObject<List<AudioFile>>(json);
        }

        public void Save()
        {
            var x = _libraryService.LibraryFiles;
            var json = JsonConvert.SerializeObject(x, Formatting.Indented);
            WriteJson(json, libraryJson);

            //SavePlaylists();
        }

        private void SaveLibrary()
        {
            

        }

        private void WriteJson(string json, string fileName)
        {
            var path = Path.Combine(appDataPath, fileName);
            using (var writer = new StreamWriter(path))
            {
                writer.Write(json);
            }
        }

        private string ReadJson(string fileName)
        {
            var path = Path.Combine(appDataPath, fileName);
            using (var reader = new StreamReader(path))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
