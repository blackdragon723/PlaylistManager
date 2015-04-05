using System.Collections.Generic;
using PlaylistManager.ApplicationServices.Models;

namespace PlaylistManager.ApplicationServices.Services.Interfaces
{
    public interface IPlaylistService
    {
        void UpdateActivePlaylist(string name);
        IEnumerable<AudioFile> RetrieveFilesFromActive();
        void SaveAllPlaylists(string destDir);
        void SavePlaylistsFromName(List<string> playlistNames, string destDir);
        void CreateAndAddPlaylistFromFile(string path);
        AudioFile ParseM3ULine(string line, string dir);

        List<Playlist> Playlists { get; }
    }
}
