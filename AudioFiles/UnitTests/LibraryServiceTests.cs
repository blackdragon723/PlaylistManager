using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace PlaylistManager.ApplicationServices.UnitTests
{
    [TestClass]
    public class LibraryServiceTests
    {
        [TestMethod]
        public void TestAddingDirectory()
        {
            string folder = @"X:\Test\Library";
            string folderB = @"X:\Test\Library B";
            var service = LibraryService.Instance;

            service.AddLibraryFolder(new DirectoryInfo(folder));
            service.AddLibraryFolder(new DirectoryInfo(folderB));

            DirectoryInfo di = new DirectoryInfo(folder + @"\New Folder");
            di.Create();

            var file = new FileInfo(@"D:\SciLor's Grooveshark.com Downloader\Downloads\Royal Teeth - Heartbeats - Act Naturally.mp3");
            file.CopyTo(@"X:\Test\Library\New Folder\Royal Teeth - Heartbeats - Act Naturally.mp3");

            /*
            using (FileStream source = File.Open(@"D:\SciLor's Grooveshark.com Downloader\Downloads\Royal Teeth - Heartbeats - Act Naturally.mp3", FileMode.Open))
            {
                File.Create(@"X:\Test\Library\New Folder\Royal Teeth - Heartbeats - Act Naturally.mp3");
                using (FileStream dest = File.Open(@"X:\Test\Library\New Folder\Royal Teeth - Heartbeats - Act Naturally.mp3", FileMode.Open))
                {
                    source.CopyTo(dest);
                }
            }
            */
        }
    }
}
