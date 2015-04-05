using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace PlaylistManager.ApplicationServices.UnitTests
{
    [TestClass]
    public class PlaylistServiceTests
    {
        //private string m3u = @"X:\Media\Music\As I Lay Dying\2012 - Awakened (Deluxe Edition)\00-as_i_lay_dying-awakened-(deluxe_edition)-2012.m3u";
        private PlaylistService service;
        public PlaylistServiceTests()
        {
            PlaylistService.Initialise();
            service = PlaylistService.Instance;
        }

        [TestMethod]
        public void TestParse()
        {
            DirectoryInfo di = new DirectoryInfo(@"X:\Media\Music\");
            foreach (var f in di.EnumerateFiles("*.m3u", SearchOption.AllDirectories))
            {
                service.CreateAndAddPlaylistFromFile(f.FullName);
            }

            service.SaveAllPlaylists(@"X:\Test\Playlists");

            StringBuilder sb = new StringBuilder();
            foreach (var pl in service.GetPlaylists())
            {
                sb.AppendLine(String.Format("{0} - {1}", pl.Name, pl.SizeWithSuffix));
            }
            using (StreamWriter sr = new StreamWriter(@"X:\Test\zzzFilesizes.txt"))
            {
                sr.Write(sb.ToString());
            }
        }
    }
}
